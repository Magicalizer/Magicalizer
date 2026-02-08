//// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
//// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Dynamic.Core;
using Magicalizer.Data.Entities.Abstractions;

namespace Magicalizer.Data.Extensions;

/// <summary>
/// Provides extension method for applying sorting to queries.
/// </summary>
public static class SortingQueryableExtensions
{
  /// <summary>
  /// Applies sorting to the specified query using a collection of <see cref="Sorting{TEntity}"/> objects.
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity being queried.</typeparam>
  /// <param name="query">The source query to apply sorting to.</param>
  /// <param name="sortings">A collection of <see cref="Sorting{TEntity}"/> objects.</param>
  /// <returns>The query with the applied sorting.</returns>
  public static IQueryable<TEntity> ApplySorting<TEntity>(this IQueryable<TEntity> query, IEnumerable<Sorting<TEntity>>? sortings)
    where TEntity : class, IEntity
  {
    if (sortings?.Any() != true) return query;

    IOrderedQueryable<TEntity>? orderedQuery = null;

    foreach (Sorting<TEntity> sorting in sortings)
    {
      string fixedPropertyPath = PropertyPathFixer.FixPropertyPath<TEntity>(sorting.PropertyPath);

      if (fixedPropertyPath.Length == 0) continue;

      orderedQuery = orderedQuery == null ?
        query.OrderBy(FormatSortingOrder(sorting)) :
        orderedQuery.ThenBy(FormatSortingOrder(sorting));
    }

    return orderedQuery ?? query;
  }

  private static string FormatSortingOrder<TEntity>(Sorting<TEntity> sorting)
    where TEntity : class, IEntity
  {
    return sorting.PropertyPath + " " + (sorting.IsAscending ? "ASC" : "DESC");
  }
}