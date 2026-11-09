// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer.Api.ModelBinding;

/// <summary>
/// Provider of <see cref="EnumerableFilterBinder"/> for collection filter properties of filters.
/// </summary>
internal class EnumerableFilterBinderProvider : IModelBinderProvider
{
  /// <inheritdoc/>
  /// <remarks>
  /// It returns <see cref="EnumerableFilterBinder"/> if the model is a collection filter
  /// (<see cref="IEnumerableFilter"/>).
  /// </remarks>
  public IModelBinder? GetBinder(ModelBinderProviderContext context)
  {
    if (typeof(IEnumerableFilter).IsAssignableFrom(context.Metadata.ModelType))
      return new EnumerableFilterBinder(context.Services.GetRequiredService<IModelBinderFactory>());

    return null;
  }
}
