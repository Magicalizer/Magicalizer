// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Extensions;
using Magicalizer.Domain;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Magicalizer.Api.ModelBinding;

/// <summary>
/// Binder of the sortings of models from comma-separated property paths (e.g. "name,-created"), see
/// <see cref="PropertyPathsBinderBase{TModel, TItem}"/>.
/// </summary>
/// <typeparam name="TModel">Model type.</typeparam>
internal class SortingBinder<TModel> : PropertyPathsBinderBase<TModel, ISorting<TModel>> where TModel : class, IModel
{
  /// <inheritdoc/>
  /// <remarks>
  /// A property path prefixed with "-" is sorted in descending order.
  /// </remarks>
  protected override ISorting<TModel> Parse(string value)
  {
    return Sorting<TModel>.Parse(value);
  }

  /// <inheritdoc/>
  protected override string? GetError(ModelBindingContext bindingContext, Type entityType, ISorting<TModel> item)
  {
    return SortingQueryableExtensions.GetError(entityType, item.PropertyPath);
  }
}
