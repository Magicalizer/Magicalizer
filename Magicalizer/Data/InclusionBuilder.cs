// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Data.Entities.Abstractions;

namespace Magicalizer.Data;

/// <summary>
/// Builds an inclusion rule for related entities.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class InclusionBuilder<TEntity> : PropertyPathBuilder<TEntity, Inclusion<TEntity>> where TEntity : class, IEntity
{
  /// <summary>
  /// Initializes a new instance of the <see cref="InclusionBuilder{TEntity}"/> class.
  /// </summary>
  public InclusionBuilder() : base(propertyPath => new Inclusion<TEntity>(propertyPath))
  {
  }
}