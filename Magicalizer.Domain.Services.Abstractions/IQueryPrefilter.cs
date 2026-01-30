// Copyright © 2026 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Services.Abstractions;

/// <summary>
/// Defines a query prefilter that applies initial restrictions, security rules, 
/// or mandatory logic to an entity query before user-defined filtering is applied.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TFilter">The filter type.</typeparam>
public interface IQueryPrefilter<TEntity, TFilter>
  where TEntity : class, IEntity, new()
  where TFilter : class, IFilter
{
  /// <summary>
  /// Applies the prefiltering logic to the specified query.
  /// </summary>
  /// <param name="query">The query to prefilter.</param>
  /// <param name="filter">The optional filter context that may influence the prefiltering logic.</param>
  /// <returns>The prefiltered query.</returns>
  IQueryable<TEntity> Apply(IQueryable<TEntity> query, TFilter? filter);
}