//// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
//// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Data.Entities.Abstractions;

namespace Magicalizer.Data.Extensions;

/// <summary>
/// Provides extension method for applying pagination to queries.
/// </summary>
public static class PagingQueryableExtensions
{
  /// <summary>
  /// Applies pagination to the queryable result by skipping and limiting the number of entities.
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity being queried.</typeparam>
  /// <param name="result">The queryable entity set to apply pagination to.</param>
  /// <param name="offset">The number of entities to skip.</param>
  /// <param name="limit">The maximum number of entities to return.</param>
  /// <returns>The modified queryable with the applied pagination.</returns>
  public static IQueryable<TEntity> ApplyPaging<TEntity>(this IQueryable<TEntity> result, int? offset, int? limit) where TEntity : class, IEntity
  {
    if (offset != null)
      result = result.Skip((int)offset);

    if (limit != null)
      result = result.Take((int)limit);

    return result;
  }
}