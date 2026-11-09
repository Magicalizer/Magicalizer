// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Services.Abstractions;

/// <summary>
/// Service of models: gets them with filtering, sorting, paging and inclusions, creates and edits them.
/// </summary>
/// <typeparam name="TModel">Model type.</typeparam>
/// <typeparam name="TFilter">Filter type.</typeparam>
public interface IService<TModel, TFilter>
  where TModel : class, IModel
  where TFilter : class, IFilter
{
  /// <summary>
  /// Gets the models that match the filter, sorted and paged, with related models included.
  /// </summary>
  /// <param name="filter">Filter to apply.</param>
  /// <param name="sortings">Sortings to apply (property paths with directions).</param>
  /// <param name="offset">Number of models to skip.</param>
  /// <param name="limit">Maximum number of models to return.</param>
  /// <param name="inclusions">Inclusions to apply (property paths of related models).</param>
  /// <param name="cancellationToken">Token to cancel the operation.</param>
  /// <returns>Models that match the filter.</returns>
  Task<IEnumerable<TModel>> GetAllAsync(TFilter? filter = null, IEnumerable<ISorting<TModel>>? sortings = null, int? offset = null, int? limit = null, IEnumerable<IInclusion<TModel>>? inclusions = null, CancellationToken cancellationToken = default);

  /// <summary>
  /// Counts the models that match the filter.
  /// </summary>
  /// <param name="filter">Filter to apply.</param>
  /// <param name="cancellationToken">Token to cancel the operation.</param>
  /// <returns>Number of models that match the filter.</returns>
  Task<int> CountAsync(TFilter? filter = null, CancellationToken cancellationToken = default);

  /// <summary>
  /// Creates the model. If the database generates the primary key, the primary key values of the model are ignored,
  /// and the created model has the generated ones.
  /// </summary>
  /// <param name="model">Model to create.</param>
  /// <param name="cancellationToken">Token to cancel the operation.</param>
  /// <returns>Created model.</returns>
  /// <exception cref="ModelAlreadyExistsException">A model with the same primary key already exists.</exception>
  Task<TModel> CreateAsync(TModel model, CancellationToken cancellationToken = default);

  /// <summary>
  /// Edits the model.
  /// </summary>
  /// <param name="model">Model to edit.</param>
  /// <param name="cancellationToken">Token to cancel the operation.</param>
  /// <exception cref="ModelNotFoundException">
  /// The model doesn't exist (or isn't available through the query prefilters).
  /// </exception>
  Task EditAsync(TModel model, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets the primary key values of the model (e.g. to build the URL of a created model).
  /// </summary>
  /// <param name="model">Model to get the primary key values of.</param>
  /// <returns>Primary key values in the order of primary key properties.</returns>
  object?[] GetKeyValues(TModel model);
}
