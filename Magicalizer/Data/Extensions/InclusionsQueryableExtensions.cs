//// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
//// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Data.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Magicalizer.Data.Extensions;

/// <summary>
/// Provides extension method for applying navigation property inclusions to queries.
/// </summary>
public static class InclusionsQueryableExtensions
{
  /// <summary>
  /// Applies inclusions to the specified query using a collection of <see cref="Inclusion{TEntity}"/> objects.
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity being queried.</typeparam>
  /// <param name="query">The source query to apply the inclusions to.</param>
  /// <param name="inclusions">A collection of <see cref="Inclusion{TEntity}"/> objects specifying related entities to include.</param>
  /// <returns>The query with the applied inclusions.</returns>
  public static IQueryable<TEntity> ApplyInclusions<TEntity>(this IQueryable<TEntity> query, IEnumerable<Inclusion<TEntity>>? inclusions)
    where TEntity : class, IEntity
  {
    if (inclusions?.Any() != true) return query;

    foreach (Inclusion<TEntity> inclusion in inclusions)
    {
      string fixedInclusion = PropertyPathFixer.FixPropertyPath<TEntity>(inclusion.PropertyPath);

      if (fixedInclusion.Length != 0)
        query = query.Include(fixedInclusion);
    }

    return query;
  }
}