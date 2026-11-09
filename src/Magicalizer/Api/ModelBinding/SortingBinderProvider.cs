// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Magicalizer.Api.ModelBinding;

/// <summary>
/// Provider of <see cref="SortingBinder{TModel}"/> for the parameters of the
/// <see cref="IEnumerable{T}"/> of <see cref="ISorting{TModel}"/> type.
/// </summary>
internal class SortingBinderProvider : IModelBinderProvider
{
  /// <inheritdoc/>
  public IModelBinder? GetBinder(ModelBinderProviderContext context)
  {
    Type? modelType = context.Metadata.ModelType.GetEnumerableElementTypeArgument(typeof(ISorting<>));

    if (modelType == null) return null;

    return (IModelBinder)Activator.CreateInstance(typeof(SortingBinder<>).MakeGenericType(modelType))!;
  }
}
