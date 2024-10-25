// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Extensions;

namespace Magicalizer.Domain;

/// <summary>
/// Represents an inclusion rule, defining a property path for related models.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
public class Inclusion<TModel> where TModel : class, IModel
{
  /// <summary>
  /// The property path for including related models (e.g., "Category.Products").
  /// </summary>
  public string PropertyPath { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="Inclusion{TModel}"/> class using an expression.
  /// </summary>
  /// <param name="property">The expression defining the property path.</param>
  public Inclusion(Expression<Func<TModel, object>> property)
  {
    this.PropertyPath = property.GetPropertyPath();
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="Inclusion{TModel}"/> class using a string property path.
  /// </summary>
  /// <param name="propertyPath">The string representing the property path (property names separated by dot).</param>
  public Inclusion(string propertyPath)
  {
    this.PropertyPath = propertyPath;
  }
}