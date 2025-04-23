// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Extensions;

namespace Magicalizer.Data;

/// <summary>
/// Represents an inclusion rule, defining a property path for related entities.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class Inclusion<TEntity> where TEntity : class, IEntity
{
  /// <summary>
  /// The property path for including related entities (e.g., "Category.Products").
  /// </summary>
  public string PropertyPath { get; }

  // <summary>
  /// Initializes a new instance of the <see cref="Inclusion{TEntity}"/> class using an inclusion rule.
  /// </summary>
  /// <param name="inclusion">The inclusion rule.</param>
  public Inclusion(IInclusion<IModel> inclusion)
  {
    this.PropertyPath = inclusion.PropertyPath;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="Inclusion{TEntity}"/> class using an expression.
  /// </summary>
  /// <param name="property">The expression defining the property path.</param>
  public Inclusion(Expression<Func<TEntity, object?>> property)
  {
    this.PropertyPath = property.GetPropertyPath();
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="Inclusion{TEntity}"/> class using a string property path.
  /// </summary>
  /// <param name="propertyPath">The string representing the property path (property names separated by dot).</param>
  public Inclusion(string propertyPath)
  {
    this.PropertyPath = propertyPath;
  }
}