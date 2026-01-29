// Copyright © 2026 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Services.Abstractions;

/// <summary>
/// Defines a query scope, applying global restrictions or visibility rules to an entity query.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TFilter">The filter type.</typeparam>
public interface IQueryScope<TEntity, TFilter>
  where TEntity : class, IEntity, new()
  where TFilter : class, IFilter
{
  /// <summary>
  /// Applies the scoping logic to the specified query.
  /// </summary>
  /// <param name="query">The query to restrict.</param>
  /// <param name="filter">The optional filter context that may influence the scope.</param>
  IQueryable<TEntity> Apply(IQueryable<TEntity> query, TFilter? filter);
}