// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Services.Abstractions;

/// <summary>
/// Service of models with a composite primary key of two properties.
/// </summary>
/// <typeparam name="TKey1">Type of the first primary key property.</typeparam>
/// <typeparam name="TKey2">Type of the second primary key property.</typeparam>
/// <typeparam name="TModel">Model type.</typeparam>
/// <typeparam name="TFilter">Filter type.</typeparam>
public interface IService<TKey1, TKey2, TModel, TFilter> : IService<TModel, TFilter>
  where TModel : class, IModel
  where TFilter : class, IFilter
{
  /// <summary>
  /// Gets the model by the composite primary key.
  /// </summary>
  /// <param name="id1">Value of the first primary key property.</param>
  /// <param name="id2">Value of the second primary key property.</param>
  /// <param name="inclusions">Inclusions to apply (property paths of related models).</param>
  /// <param name="cancellationToken">Token to cancel the operation.</param>
  /// <returns>Model, or <c>null</c> if it doesn't exist.</returns>
  Task<TModel?> GetByIdAsync(TKey1 id1, TKey2 id2, IEnumerable<IInclusion<TModel>>? inclusions = null, CancellationToken cancellationToken = default);

  /// <summary>
  /// Deletes the model by the composite primary key.
  /// </summary>
  /// <param name="id1">Value of the first primary key property.</param>
  /// <param name="id2">Value of the second primary key property.</param>
  /// <param name="cancellationToken">Token to cancel the operation.</param>
  /// <exception cref="ModelNotFoundException">
  /// The model doesn't exist (or isn't available through the query prefilters).
  /// </exception>
  /// <exception cref="ModelInUseException">
  /// The database restricts the deletion of the model (e.g. other models reference it).
  /// </exception>
  Task DeleteAsync(TKey1 id1, TKey2 id2, CancellationToken cancellationToken = default);
}
