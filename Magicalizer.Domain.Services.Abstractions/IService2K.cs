// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Services.Abstractions;

/// <summary>
/// A service interface for managing models with a composite primary key (two properties),
/// supporting CRUD, filtering, sorting, pagination, and inclusion.
/// </summary>
/// <typeparam name="TKey1">The first primary key type.</typeparam>
/// <typeparam name="TKey2">The second primary key type.</typeparam>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TFilter">The filter type.</typeparam>
public interface IService<TKey1, TKey2, TModel, TFilter> : IService<TModel, TFilter>
  where TModel : class, IModel
  where TFilter : class, IFilter
{
  /// <summary>
  /// Retrieves a model by its composite primary key.
  /// </summary>
  /// <param name="id1">The value of the first property in the composite primary key.</param>
  /// <param name="id2">The value of the second property in the composite primary key.</param>
  /// <param name="inclusions">The inclusion property paths to include related models.</param>
  /// <returns>The model with the specified composite primary key, or <c>null</c> if no such model exists.</returns>
  Task<TModel?> GetByIdAsync(TKey1 id1, TKey2 id2, params IInclusion<TModel>[] inclusions);

  /// <summary>
  /// Deletes a model by its composite primary key.
  /// </summary>
  /// <param name="id1">The value of the first property in the composite primary key.</param>
  /// <param name="id2">The value of the second property in the composite primary key.</param>
  /// <returns>A boolean indicating success.</returns>
  Task<bool> DeleteAsync(TKey1 id1, TKey2 id2);
}