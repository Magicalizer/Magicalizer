// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Linq.Expressions;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Extensions;

namespace Magicalizer.Data;

/// <summary>
/// Inclusion of related entities into a query.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
public class Inclusion<TEntity> where TEntity : class, IEntity
{
  /// <summary>
  /// Property path of related entities to include (e.g. "Category.Products").
  /// </summary>
  public string PropertyPath { get; }

  /// <summary>
  /// Creates an inclusion from the expression.
  /// </summary>
  /// <param name="property">Expression of the property path.</param>
  public Inclusion(Expression<Func<TEntity, object?>> property)
  {
    this.PropertyPath = property.GetPropertyPath();
  }

  /// <summary>
  /// Creates an inclusion from the property path.
  /// </summary>
  /// <param name="propertyPath">Property path (property names separated by dots).</param>
  public Inclusion(string propertyPath)
  {
    this.PropertyPath = propertyPath;
  }
}
