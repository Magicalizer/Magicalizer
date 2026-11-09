// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Services.Abstractions;

/// <summary>
/// Prefilter of entity queries. It applies initial restrictions (e.g. security rules or mandatory conditions) before
/// the filter.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
/// <typeparam name="TFilter">Filter type.</typeparam>
public interface IQueryPrefilter<TEntity, TFilter>
  where TEntity : class, IEntity, new()
  where TFilter : class, IFilter
{
  /// <summary>
  /// Applies the prefilter to the query.
  /// </summary>
  /// <param name="query">Query to prefilter.</param>
  /// <param name="filter">Filter, which may affect the prefilter.</param>
  /// <returns>Prefiltered query.</returns>
  IQueryable<TEntity> Apply(IQueryable<TEntity> query, TFilter? filter);
}
