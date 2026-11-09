// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Entities.Abstractions;

namespace Magicalizer.Data.Extensions;

/// <summary>
/// Extensions of queries to apply paging.
/// </summary>
public static class PagingQueryableExtensions
{
  /// <summary>
  /// Applies paging to the query.
  /// </summary>
  /// <typeparam name="TEntity">Entity type.</typeparam>
  /// <param name="query">Query to apply paging to.</param>
  /// <param name="offset">Number of entities to skip.</param>
  /// <param name="limit">Maximum number of entities to return.</param>
  /// <returns>Query with paging applied.</returns>
  public static IQueryable<TEntity> ApplyPaging<TEntity>(this IQueryable<TEntity> query, int? offset, int? limit)
    where TEntity : class, IEntity
  {
    if (offset != null)
      query = query.Skip(offset.Value);

    if (limit != null)
      query = query.Take(limit.Value);

    return query;
  }
}
