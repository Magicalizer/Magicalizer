// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Magicalizer.Api.ModelBinding;

/// <summary>
/// Base of the binders of the property paths of models from comma-separated values (e.g. the sortings "name,-created"
/// or the inclusions "category,photos"). The parameter may be repeated too (e.g. "sort=name&amp;sort=-price"). The
/// property paths are checked against the entity of the model, so an invalid one is a model state error of the
/// parameter (and the request isn't executed) rather than an exception of the query.
/// </summary>
/// <typeparam name="TModel">Model type.</typeparam>
/// <typeparam name="TItem">Type of the bound items (e.g. <c>ISorting&lt;TModel&gt;</c>).</typeparam>
internal abstract class PropertyPathsBinderBase<TModel, TItem> : IModelBinder
  where TModel : class, IModel
  where TItem : class
{
  // Entity of the model, the property paths are applied to it (they aren't checked if the model has no entity).
  private static readonly Type? EntityType = typeof(TModel).GetGenericInterfaceTypeArgument(typeof(IModel<>), typeof(IEntity));

  /// <inheritdoc/>
  public Task BindModelAsync(ModelBindingContext bindingContext)
  {
    ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

    if (valueProviderResult == ValueProviderResult.None) return Task.CompletedTask;

    bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

    List<TItem> items = [];
    bool isValid = true;

    foreach (string value in valueProviderResult.Values.SelectMany(v => v.SplitByComma()))
    {
      TItem item = this.Parse(value);
      string? error = EntityType == null ? null : this.GetError(bindingContext, EntityType, item);

      if (error == null)
        items.Add(item);

      else
      {
        bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, error);
        isValid = false;
      }
    }

    bindingContext.Result = isValid ? ModelBindingResult.Success(items) : ModelBindingResult.Failed();
    return Task.CompletedTask;
  }

  /// <summary>
  /// Parses the item from a value (e.g. "-created").
  /// </summary>
  /// <param name="value">Value to parse.</param>
  /// <returns>Item with the property path.</returns>
  protected abstract TItem Parse(string value);

  /// <summary>
  /// Gets the error of applying the property path of the item to the entity of the model.
  /// </summary>
  /// <param name="bindingContext">Context of the binding (e.g. to get the services of the request).</param>
  /// <param name="entityType">Entity type of the model.</param>
  /// <param name="item">Item to check.</param>
  /// <returns>Error, or <c>null</c> if the property path is valid.</returns>
  protected abstract string? GetError(ModelBindingContext bindingContext, Type entityType, TItem item);
}
