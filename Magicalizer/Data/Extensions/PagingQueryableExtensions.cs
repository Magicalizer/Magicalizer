// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Data.Entities.Abstractions;

namespace Magicalizer.Data.Extensions;

/// <summary>
/// Provides extension method for applying paging to queries.
/// </summary>
public static class PagingQueryableExtensions
{
  /// <summary>
  /// Applies paging to the specified query, skipping and limiting the entities.
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity being queried.</typeparam>
  /// <param name="query">The source query to apply paging to.</param>
  /// <param name="offset">The number of entities to skip.</param>
  /// <param name="limit">The maximum number of entities to return.</param>
  /// <returns>The query with the applied paging.</returns>
  public static IQueryable<TEntity> ApplyPaging<TEntity>(this IQueryable<TEntity> query, int? offset, int? limit) where TEntity : class, IEntity
  {
    if (offset != null)
      query = query.Skip((int)offset);

    if (limit != null)
      query = query.Take((int)limit);

    return query;
  }
}