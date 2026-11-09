// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Linq.Expressions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Extensions;

namespace Magicalizer.Domain;

/// <summary>
/// Inclusion of related models into a query.
/// </summary>
/// <typeparam name="TModel">Model type.</typeparam>
public class Inclusion<TModel> : IInclusion<TModel> where TModel : class, IModel
{
  /// <inheritdoc/>
  public string PropertyPath { get; }

  /// <summary>
  /// Creates an inclusion from the expression.
  /// </summary>
  /// <param name="property">Expression of the property path.</param>
  public Inclusion(Expression<Func<TModel, object?>> property)
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
