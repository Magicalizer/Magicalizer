// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Extensions;

namespace Magicalizer.Domain;

/// <summary>
/// Represents a sorting rule, defining a property path and sort direction for models.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
public class Sorting<TModel> : ISorting<TModel> where TModel : class, IModel
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

  /// <summary>
  /// Parses a string to create a new <see cref="Sorting{TModel}"/> instance.
  /// </summary>
  /// <param name="value">
  /// A string representing the sorting direction and property path. The first character
  /// determines the sort direction: '+' for ascending, '-' for descending. If no sign is present,
  /// sorting defaults to ascending. The remaining characters represent the property path for sorting.
  /// </param>
  /// <returns>A new <see cref="Sorting{TModel}"/> instance with the specified sorting direction and property path.</returns>
  public static Sorting<TModel> Parse(string value)
  {
    return new Sorting<TModel>(value[0] != '-', (value[0] == '+' || value[0] == '-') ? value[1..] : value);
  }
}