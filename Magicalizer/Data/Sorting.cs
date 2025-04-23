// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Extensions;

namespace Magicalizer.Data;

/// <summary>
/// Represents a sorting rule, defining a property path and sort direction.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class Sorting<TEntity> where TEntity : class, IEntity
{
  /// <summary>
  /// Indicates if sorting is ascending (true) or descending (false).
  /// </summary>
  public bool IsAscending { get; }

  /// <summary>
  /// The property path for sorting (e.g., "Category.Name").
  /// </summary>
  public string PropertyPath { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="Sorting{TEntity}"/> class using a sorting rule.
  /// </summary>
  /// <param name="inclusion">The sorting rule.</param>
  public Sorting(ISorting<IModel> inclusion)
  {
    this.IsAscending = inclusion.IsAscending;
    this.PropertyPath = inclusion.PropertyPath;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="Sorting{TEntity}"/> class using an expression.
  /// </summary>
  /// <param name="isAscending">Indicates if sorting is ascending.</param>
  /// <param name="property">The expression defining the property path.</param>
  public Sorting(bool isAscending, Expression<Func<TEntity, object>> property)
  {
    this.IsAscending = isAscending;
    this.PropertyPath = property.GetPropertyPath();
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="Sorting{TEntity}"/> class using a string property path.
  /// </summary>
  /// <param name="isAscending">Indicates if sorting is ascending.</param>
  /// <param name="propertyPath">The string representing the property path (property names separated by dot).</param>
  public Sorting(bool isAscending, string propertyPath)
  {
    this.IsAscending = isAscending;
    this.PropertyPath = propertyPath;
  }
}