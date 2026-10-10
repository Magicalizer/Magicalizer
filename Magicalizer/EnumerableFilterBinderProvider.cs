// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer;

/// <summary>
/// Provides the <see cref="EnumerableFilterBinder"/> for the <see cref="EnumerableFilter{TFilter}"/> properties of the filters.
/// </summary>
public class EnumerableFilterBinderProvider : IModelBinderProvider
{
  /// <summary>
  /// Gets the <see cref="EnumerableFilterBinder"/> if the model is an <see cref="EnumerableFilter{TFilter}"/>.
  /// </summary>
  /// <param name="context">The model binder provider context.</param>
  /// <returns>The binder, or <c>null</c> if the model is not an <see cref="EnumerableFilter{TFilter}"/>.</returns>
  public IModelBinder? GetBinder(ModelBinderProviderContext context)
  {
    if (context == null) throw new ArgumentNullException(nameof(context));

    if (context.Metadata.ModelType.IsGenericType && context.Metadata.ModelType.GetGenericTypeDefinition() == typeof(EnumerableFilter<>))
      return new EnumerableFilterBinder(context.Services.GetRequiredService<IModelBinderFactory>());

    return null;
  }
}