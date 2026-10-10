// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Services.Abstractions;

/// <summary>
/// A base service interface for managing models supporting CRUD, filtering, sorting, pagination, and inclusion.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TFilter">The filter type.</typeparam>
public interface IService<TModel, TFilter>
  where TModel : class, IModel
  where TFilter : class, IFilter
{
  /// <summary>
  /// Retrieves all the models that match filter with sorting, pagination, and inclusions.
  /// </summary>
  /// <param name="filter">The filter to query models.</param>
  /// <param name="sortings">The sorting rules (property paths with sorting directions).</param>
  /// <param name="offset">The number of models to skip.</param>
  /// <param name="limit">The maximum number of models to return.</param>
  /// <param name="inclusions">The inclusion rules (property paths of the related models to include).</param>
  /// <param name="cancellationToken">The token to cancel the operation.</param>
  /// <returns>The collection of models that match the filter.</returns>
  Task<IEnumerable<TModel>> GetAllAsync(TFilter? filter = null, IEnumerable<ISorting<TModel>>? sortings = null, int? offset = null, int? limit = null, IEnumerable<IInclusion<TModel>>? inclusions = null, CancellationToken cancellationToken = default);

  /// <summary>
  /// Retrieves the total count of models that match the optional filter.
  /// </summary>
  /// <param name="filter">The filter to count models.</param>
  /// <param name="cancellationToken">The token to cancel the operation.</param>
  /// <returns>The count of models that match the filter.</returns>
  Task<int> CountAsync(TFilter? filter = null, CancellationToken cancellationToken = default);

  /// <summary>
  /// Creates a new model and returns it.
  /// </summary>
  /// <param name="model">The model to create.</param>
  /// <param name="cancellationToken">The token to cancel the operation.</param>
  /// <returns>The created model.</returns>
  Task<TModel> CreateAsync(TModel model, CancellationToken cancellationToken = default);

  /// <summary>
  /// Updates an existing model.
  /// </summary>
  /// <param name="model">The model to update.</param>
  /// <param name="cancellationToken">The token to cancel the operation.</param>
  Task EditAsync(TModel model, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets the primary key values of a model (e.g. to build the URL of a created model).
  /// </summary>
  /// <param name="model">The model to get the primary key values of.</param>
  /// <returns>The primary key values in the order of the primary key properties.</returns>
  object?[] GetKeyValues(TModel model);
}