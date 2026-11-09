// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Reflection;
using Magicalizer.Extensions;
using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Magicalizer.Api.ModelBinding;

/// <summary>
/// Binder of collection filters (<see cref="IEnumerableFilter"/>, e.g. <see cref="EnumerableFilter{TFilter}"/>) from
/// request values. The <see cref="IEnumerableFilter.Any"/> and <see cref="IEnumerableFilter.None"/> conditions are set
/// either as a single filter or as a list of filters.
/// </summary>
internal class EnumerableFilterBinder : IModelBinder
{
  private readonly IModelBinderFactory binderFactory;

  /// <summary>
  /// Creates a binder.
  /// </summary>
  /// <param name="binderFactory">Factory of element filter binders.</param>
  public EnumerableFilterBinder(IModelBinderFactory binderFactory)
  {
    this.binderFactory = binderFactory;
  }

  /// <inheritdoc/>
  public async Task BindModelAsync(ModelBindingContext bindingContext)
  {
    object enumerableFilter = Activator.CreateInstance(bindingContext.ModelType)!;

    BindBoolProperty(bindingContext, enumerableFilter, nameof(IEnumerableFilter.IsEmpty));
    await this.BindSingleFilterOrFilterListAsync(bindingContext, enumerableFilter, nameof(IEnumerableFilter.Any));
    await this.BindSingleFilterOrFilterListAsync(bindingContext, enumerableFilter, nameof(IEnumerableFilter.None));
    bindingContext.Result = ModelBindingResult.Success(enumerableFilter);
  }

  // Binds a boolean property (e.g. IsEmpty) if its value is set. An invalid value is a model state error, as the one of
  // a value filter condition (e.g. IsNull) is, while an empty one means the condition isn't set.
  private static void BindBoolProperty(ModelBindingContext bindingContext, object enumerableFilter, string propertyName)
  {
    PropertyInfo? property = bindingContext.ModelType.GetPublicProperty(propertyName);

    if (property == null) return;

    string prefix = ModelNames.CreatePropertyModelName(bindingContext.ModelName, propertyName);
    ValueProviderResult result = bindingContext.ValueProvider.GetValue(prefix);

    if (result == ValueProviderResult.None || string.IsNullOrEmpty(result.FirstValue)) return;

    bindingContext.ModelState.SetModelValue(prefix, result);

    if (bool.TryParse(result.FirstValue, out bool value))
      property.SetValue(enumerableFilter, value);

    else
    {
      ModelMetadata metadata = bindingContext.ModelMetadata.Properties[propertyName] ?? bindingContext.ModelMetadata;

      bindingContext.ModelState.TryAddModelError(prefix, metadata.ModelBindingMessageProvider.AttemptedValueIsInvalidAccessor(result.FirstValue, metadata.DisplayName ?? propertyName));
    }
  }

  // Binds a property with element filters (e.g. Any): a single filter first, then a list of filters.
  private async Task BindSingleFilterOrFilterListAsync(ModelBindingContext bindingContext, object enumerableFilter, string propertyName)
  {
    PropertyInfo? property = bindingContext.ModelType.GetPublicProperty(propertyName);

    if (property == null) return;

    string prefix = ModelNames.CreatePropertyModelName(bindingContext.ModelName, propertyName);

    if (!bindingContext.ValueProvider.ContainsPrefix(prefix)) return;

    Type filterType = bindingContext.ModelType.GetElementFilterType();
    Type filterListType = typeof(List<>).MakeGenericType(filterType);
    object? filter = await this.TryBindAsync(bindingContext, filterType, prefix);
    IList? filterList;

    if (filter == null)
      filterList = await this.TryBindAsync(bindingContext, filterListType, prefix) as IList;

    else
    {
      filterList = (IList)Activator.CreateInstance(filterListType)!;
      filterList.Add(filter);
    }

    if (filterList != null && filterList.Count != 0)
      property.SetValue(enumerableFilter, filterList);
  }

  // Tries to bind an object of the type (a filter or a list of filters) with the prefix. Returns null if nothing is bound.
  private async Task<object?> TryBindAsync(ModelBindingContext bindingContext, Type type, string prefix)
  {
    bindingContext.Result = ModelBindingResult.Failed();

    ModelMetadata metadata = bindingContext.ModelMetadata.GetMetadataForType(type);
    IModelBinder binder = this.binderFactory.CreateBinder(new ModelBinderFactoryContext { Metadata = metadata, BindingInfo = new BindingInfo { BinderModelName = prefix } });

    using (bindingContext.EnterNestedScope(metadata, prefix, prefix, null))
    {
      await binder.BindModelAsync(bindingContext);
      return bindingContext.Result.IsModelSet ? bindingContext.Result.Model : null;
    }
  }
}
