// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Services.Abstractions;

/// <summary>
/// A service interface for managing models with a single-property primary key,
/// supporting CRUD, filtering, sorting, pagination, and inclusion.
/// </summary>
/// <typeparam name="TKey">The primary key type.</typeparam>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TFilter">The filter type.</typeparam>
public interface IService<TKey, TModel, TFilter> : IService<TModel, TFilter>
  where TModel : class, IModel
  where TFilter : class, IFilter
{
  /// <summary>
  /// Retrieves a model by its primary key.
  /// </summary>
  /// <param name="id">The value of the primary key.</param>
  /// <param name="inclusions">The inclusion property paths to include related models.</param>
  /// <returns>The model with the specified primary key, or <c>null</c> if no such model exists.</returns>
  Task<TModel?> GetByIdAsync(TKey id, params IInclusion<TModel>[] inclusions);

  /// <summary>
  /// Deletes a model by its primary key.
  /// </summary>
  /// <param name="id">The value of the primary key property.</param>
  /// <returns>A boolean indicating success.</returns>
  Task<bool> DeleteAsync(TKey id);
}