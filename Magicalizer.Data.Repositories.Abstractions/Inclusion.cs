// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Magicalizer.Data.Entities.Abstractions;

namespace Magicalizer.Data.Repositories.Abstractions
{
  /// <summary>
  /// Contains an entity's navigation path that should be loaded together with the entity.
  /// </summary>
  /// <typeparam name="TEntity">An entity type.</typeparam>
  public class Inclusion<TEntity> where TEntity : class, IEntity
  {
    /// <summary>
    /// An expression that represents a navigation path.
    /// </summary>
    public Expression<Func<TEntity, object>> Property { get; }

    /// <summary>
    /// A string that represents a navigation path (property names are separated by a dot).
    /// </summary>
    public string PropertyPath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Inclusion{TEntity}"/> class.
    /// </summary>
    /// <param name="property">An expression that represents a navigation path.</param>
    public Inclusion(Expression<Func<TEntity, object>> property)
    {
      this.Property = property;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Inclusion{TEntity}"/> class.
    /// </summary>
    /// <param name="propertyPath">A string that represents a navigation path (property names are separated by a dot).</param>
    public Inclusion(string propertyPath)
    {
      this.PropertyPath = propertyPath;
    }
  }
}