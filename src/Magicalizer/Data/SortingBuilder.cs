// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.PropertyPaths;

namespace Magicalizer.Data;

/// <summary>
/// Builder of an entity sorting.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
public class SortingBuilder<TEntity> : PropertyPathBuilder<TEntity, Sorting<TEntity>> where TEntity : class, IEntity
{
  /// <summary>
  /// Creates a builder.
  /// </summary>
  /// <param name="isAscending">Whether the sorting is ascending (otherwise descending).</param>
  public SortingBuilder(bool isAscending) : base(propertyPath => new Sorting<TEntity>(isAscending, propertyPath)) { }
}
