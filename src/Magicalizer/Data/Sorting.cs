// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Linq.Expressions;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Extensions;

namespace Magicalizer.Data;

/// <summary>
/// Sorting of an entity query by a property.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
public class Sorting<TEntity> where TEntity : class, IEntity
{
  /// <summary>
  /// Whether the sorting is ascending (otherwise descending).
  /// </summary>
  public bool IsAscending { get; }

  /// <summary>
  /// Property path to sort by (e.g. "Category.Name").
  /// </summary>
  public string PropertyPath { get; }

  /// <summary>
  /// Creates a sorting from the expression.
  /// </summary>
  /// <param name="isAscending">Whether the sorting is ascending (otherwise descending).</param>
  /// <param name="property">Expression of the property path.</param>
  public Sorting(bool isAscending, Expression<Func<TEntity, object?>> property)
  {
    this.IsAscending = isAscending;
    this.PropertyPath = property.GetPropertyPath();
  }

  /// <summary>
  /// Creates a sorting from the property path.
  /// </summary>
  /// <param name="isAscending">Whether the sorting is ascending (otherwise descending).</param>
  /// <param name="propertyPath">Property path (property names separated by dots).</param>
  public Sorting(bool isAscending, string propertyPath)
  {
    this.IsAscending = isAscending;
    this.PropertyPath = propertyPath;
  }
}
