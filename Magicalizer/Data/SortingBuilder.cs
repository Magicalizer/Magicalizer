// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Data.Entities.Abstractions;

namespace Magicalizer.Data;

/// <summary>
/// Builds a sorting rule for entities.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class SortingBuilder<TEntity> : PropertyPathBuilder<TEntity, Sorting<TEntity>> where TEntity : class, IEntity
{
  /// <summary>
  /// Initializes a new instance of the <see cref="SortingBuilder{TEntity}"/> class.
  /// </summary>
  /// <param name="isAscending">Indicates if sorting is ascending.</param>
  public SortingBuilder(bool isAscending) : base(propertyPath => new Sorting<TEntity>(isAscending, propertyPath))
  {
  }
}