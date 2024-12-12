//// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
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
  /// Applies inclusions to the queryable result using a collection of <see cref="Inclusion{TEntity}"/> objects.
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity being queried.</typeparam>
  /// <param name="result">The queryable entity set to apply the inclusions to.</param>
  /// <param name="inclusions">A collection of <see cref="Inclusion{TEntity}"/> objects specifying related entities to include.</param>
  /// <returns>The modified queryable with the applied inclusions.</returns>
  public static IQueryable<TEntity> ApplyInclusions<TEntity>(this IQueryable<TEntity> result, IEnumerable<Inclusion<TEntity>> inclusions)
    where TEntity : class, IEntity
  {
    if (!inclusions.Any()) return result;

    foreach (Inclusion<TEntity> inclusion in inclusions)
    {
      string fixedInclusion = PropertyPathFixer.FixPropertyPath<TEntity>(inclusion.PropertyPath);

      if (fixedInclusion.Length != 0)
        result = result.Include(fixedInclusion);
    }

    return result;
  }
}