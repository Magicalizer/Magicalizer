//// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
//// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Dynamic.Core;
using Magicalizer.Data.Entities.Abstractions;

namespace Magicalizer.Data.Extensions;

/// <summary>
/// Provides extension methods for applying sorting to queries.
/// </summary>
public static class SortingQueryableExtensions
{
  /// <summary>
  /// Applies sorting to the queryable result using a sorting string.
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity being queried.</typeparam>
  /// <param name="result">The queryable entity set to apply sorting to.</param>
  /// <param name="sorting">The sorting string (e.g., "+name,-date").</param>
  /// <returns>The modified queryable with the applied sorting.</returns>
  public static IQueryable<TEntity> ApplySorting<TEntity>(this IQueryable<TEntity> result, string? sorting)
    where TEntity : class, IEntity
  {
    if (sorting == null) return result;

    if (!CheckSortingOrder(sorting)) return result;

    return result.OrderBy(FormatSortingOrder(sorting));
  }

  /// <summary>
  /// Applies sorting to the queryable result using multiple sorting strings.
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity being queried.</typeparam>
  /// <param name="result">The queryable entity set to apply sorting to.</param>
  /// <param name="sortings">A collection of sorting strings (e.g., "+name,-date").</param>
  /// <returns>The modified queryable with the applied sorting.</returns>
  public static IQueryable<TEntity> ApplySorting<TEntity>(this IQueryable<TEntity> result, IEnumerable<string> sortings)
    where TEntity : class, IEntity
  {
    if (sortings == null || !sortings.Any()) return result;

    IOrderedQueryable<TEntity>? orderedResult = null;

    foreach (string sorting in sortings)
    {
      if (!CheckSortingOrder(sorting)) continue;

      orderedResult = orderedResult == null ?
        result.OrderBy(FormatSortingOrder(sorting)) :
        orderedResult.ThenBy(FormatSortingOrder(sorting));
    }

    return orderedResult ?? result;
  }

  /// <summary>
  /// Applies sorting to the queryable result using a collection of <see cref="Sorting{TEntity}"/> objects.
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity being queried.</typeparam>
  /// <param name="result">The queryable entity set to apply sorting to.</param>
  /// <param name="sortings">A collection of <see cref="Sorting{TEntity}"/> objects.</param>
  /// <returns>The modified queryable with the applied sorting.</returns>
  public static IQueryable<TEntity> ApplySorting<TEntity>(this IQueryable<TEntity> result, IEnumerable<Sorting<TEntity>> sortings)
    where TEntity : class, IEntity
  {
    if (sortings == null || !sortings.Any()) return result;

    IOrderedQueryable<TEntity>? orderedResult = null;

    foreach (Sorting<TEntity> sorting in sortings)
    {
      orderedResult = orderedResult == null ?
        result.OrderBy(FormatSortingOrder(sorting)) :
        orderedResult.ThenBy(FormatSortingOrder(sorting));
    }

    return orderedResult ?? result;
  }

  private static bool CheckSortingOrder(string sorting)
  {
    if (sorting.Length < 2) return false;

    if (sorting[0] != '+' && sorting[0] != '-') return false;

    return true;
  }

  private static string FormatSortingOrder(string sorting)
  {
    return sorting.Substring(1) + " " + (sorting[0] == '+' ? "ASC" : "DESC");
  }

  private static string FormatSortingOrder<TEntity>(Sorting<TEntity> sorting)
    where TEntity : class, IEntity
  {
    return sorting.PropertyPath + " " + (sorting.IsAscending ? "ASC" : "DESC");
  }
}