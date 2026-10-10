// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Magicalizer;

/// <summary>
/// Binds an <see cref="EnumerableFilter{TFilter}"/> from the request values. The <see cref="IEnumerableFilter.Any"/>
/// and <see cref="IEnumerableFilter.None"/> conditions can be specified either as a single filter or as a list of filters.
/// </summary>
public class EnumerableFilterBinder : IModelBinder
{
  // The cached properties of an enumerable filter type.
  private record EnumerableFilterPropertyCache(
    PropertyInfo? IsEmpty,
    PropertyInfo? IsNotEmpty,
    PropertyInfo? Any,
    PropertyInfo? None
  );

  private static readonly ConcurrentDictionary<Type, EnumerableFilterPropertyCache> propertiesByTypes = new();
  private readonly IModelBinderFactory binderFactory;

  /// <summary>
  /// Initializes a new instance of the <see cref="EnumerableFilterBinder"/> class.
  /// </summary>
  /// <param name="binderFactory">The factory that creates the binders for the filters of the collection elements.</param>
  public EnumerableFilterBinder(IModelBinderFactory binderFactory)
  {
    this.binderFactory = binderFactory;
  }

  /// <summary>
  /// Binds the enumerable filter from the request values.
  /// </summary>
  /// <param name="context">The model binding context.</param>
  public async Task BindModelAsync(ModelBindingContext context)
  {
    object enumerableFilter = Activator.CreateInstance(context.ModelType)!;
    Type filterType = context.ModelType.GetGenericArguments()[0];
    EnumerableFilterPropertyCache properties = propertiesByTypes.GetOrAdd(context.ModelType, t => new(
      t.GetProperty(nameof(IEnumerableFilter.IsEmpty)),
      t.GetProperty(nameof(IEnumerableFilter.IsNotEmpty)),
      t.GetProperty(nameof(IEnumerableFilter.Any)),
      t.GetProperty(nameof(IEnumerableFilter.None))
    ));

    this.BindBoolProperty(context, enumerableFilter, properties.IsEmpty, nameof(IEnumerableFilter.IsEmpty));
    this.BindBoolProperty(context, enumerableFilter, properties.IsNotEmpty, nameof(IEnumerableFilter.IsNotEmpty));
    await this.BindSingleFilterOrFilterList(context, enumerableFilter, filterType, properties.Any, nameof(IEnumerableFilter.Any));
    await this.BindSingleFilterOrFilterList(context, enumerableFilter, filterType, properties.None, nameof(IEnumerableFilter.None));
    context.Result = ModelBindingResult.Success(enumerableFilter);
  }

  // Binds a boolean property (e.g. IsEmpty) if its value is provided and valid.
  private void BindBoolProperty(ModelBindingContext context, object enumerableFilter, PropertyInfo? property, string propertyName)
  {
    if (property == null) return;

    string prefix = ModelNames.CreatePropertyModelName(context.ModelName, propertyName);
    ValueProviderResult result = context.ValueProvider.GetValue(prefix);

    if (result == ValueProviderResult.None) return;

    if (bool.TryParse(result.FirstValue, out bool value))
      property.SetValue(enumerableFilter, value);
  }

  // Binds a property that contains the filters of the collection elements (e.g. Any),
  // trying to bind a single filter first, and then a list of filters.
  private async Task BindSingleFilterOrFilterList(ModelBindingContext context, object enumerableFilter, Type filterType, PropertyInfo? property, string propertyName)
  {
    if (property == null) return;

    string prefix = ModelNames.CreatePropertyModelName(context.ModelName, propertyName);

    if (!context.ValueProvider.ContainsPrefix(prefix)) return;

    IList? filterList = null;
    Type filterListType = typeof(List<>).MakeGenericType(filterType);
    object? filter = await this.TryBind(context, filterType, prefix);

    if (filter == null)
      filterList = await this.TryBind(context, filterListType, prefix) as IList;

    else
    {
      filterList = (IList)Activator.CreateInstance(filterListType)!;
      filterList.Add(filter);
    }

    if (filterList != null && filterList.Count != 0)
      property.SetValue(enumerableFilter, filterList);
  }

  // Tries to bind an object of the specified type using the given prefix. Returns null if nothing is bound.
  private async Task<object?> TryBind(ModelBindingContext context, Type filterType, string prefix)
  {
    context.Result = ModelBindingResult.Failed();

    ModelMetadata metadata = context.ModelMetadata.GetMetadataForType(filterType);
    IModelBinder binder = this.binderFactory.CreateBinder(new ModelBinderFactoryContext { Metadata = metadata, BindingInfo = new BindingInfo { BinderModelName = prefix } });

    using (context.EnterNestedScope(metadata, prefix, prefix, null))
    {
      await binder.BindModelAsync(context);
      return context.Result.IsModelSet ? context.Result.Model : null;
    }
  }
}
