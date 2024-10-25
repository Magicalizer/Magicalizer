// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Extensions;

namespace Magicalizer.Domain;

/// <summary>
/// Represents a sorting rule, defining a property path and sort direction for models.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
public class Sorting<TModel> where TModel : class, IModel
{
  /// <summary>
  /// Indicates if sorting is ascending (true) or descending (false).
  /// </summary>
  public bool IsAscending { get; }

  /// <summary>
  /// The property path for sorting models (e.g., "Category.Name").
  /// </summary>
  public string PropertyPath { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="Sorting{TModel}"/> class using an expression.
  /// </summary>
  /// <param name="isAscending">Indicates if sorting is ascending.</param>
  /// <param name="property">The expression defining the property path.</param>
  public Sorting(bool isAscending, Expression<Func<TModel, object>> property)
  {
    this.IsAscending = isAscending;
    this.PropertyPath = property.GetPropertyPath();
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="Sorting{TModel}"/> class using a string property path.
  /// </summary>
  /// <param name="isAscending">Indicates if sorting is ascending.</param>
  /// <param name="propertyPath">The string representing the property path (property names separated by dot).</param>
  public Sorting(bool isAscending, string propertyPath)
  {
    this.IsAscending = isAscending;
    this.PropertyPath = propertyPath;
  }
}