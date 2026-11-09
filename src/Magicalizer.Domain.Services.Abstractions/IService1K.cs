// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Services.Abstractions;

/// <summary>
/// Service of models with a primary key of one property.
/// </summary>
/// <typeparam name="TKey">Primary key type.</typeparam>
/// <typeparam name="TModel">Model type.</typeparam>
/// <typeparam name="TFilter">Filter type.</typeparam>
public interface IService<TKey, TModel, TFilter> : IService<TModel, TFilter>
  where TModel : class, IModel
  where TFilter : class, IFilter
{
  /// <summary>
  /// Gets the model by the primary key.
  /// </summary>
  /// <param name="id">Primary key value.</param>
  /// <param name="inclusions">Inclusions to apply (property paths of related models).</param>
  /// <param name="cancellationToken">Token to cancel the operation.</param>
  /// <returns>Model, or <c>null</c> if it doesn't exist.</returns>
  Task<TModel?> GetByIdAsync(TKey id, IEnumerable<IInclusion<TModel>>? inclusions = null, CancellationToken cancellationToken = default);

  /// <summary>
  /// Deletes the model by the primary key.
  /// </summary>
  /// <param name="id">Primary key value.</param>
  /// <param name="cancellationToken">Token to cancel the operation.</param>
  /// <exception cref="ModelNotFoundException">
  /// The model doesn't exist (or isn't available through the query prefilters).
  /// </exception>
  /// <exception cref="ModelInUseException">
  /// The database restricts the deletion of the model (e.g. other models reference it).
  /// </exception>
  Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
}
