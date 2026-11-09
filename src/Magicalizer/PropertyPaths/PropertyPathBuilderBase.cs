// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Linq.Expressions;
using Magicalizer.Extensions;

namespace Magicalizer.PropertyPaths;

/// <summary>
/// Base of property path builders.
/// </summary>
/// <typeparam name="TObject">Root object type.</typeparam>
/// <typeparam name="TResult">Type of the built result.</typeparam>
public abstract class PropertyPathBuilderBase<TObject, TResult>
  where TObject : class
  where TResult : class
{
  // Creates the result from the path.
  private readonly Func<string, TResult> resultFactory;

  // Path built so far. It's shared with nested builders, so they extend the same path.
  private readonly IList<Expression> propertyPath;

  /// <summary>
  /// Creates a builder.
  /// </summary>
  /// <param name="resultFactory">Creates the result from the path.</param>
  /// <param name="propertyPath">Path built so far.</param>
  protected PropertyPathBuilderBase(Func<string, TResult> resultFactory, IList<Expression>? propertyPath = null)
  {
    this.resultFactory = resultFactory;
    this.propertyPath = propertyPath ?? [];
  }

  /// <summary>
  /// Builds the result from the property path.
  /// </summary>
  /// <returns>Result.</returns>
  public TResult Build()
  {
    return this.resultFactory(string.Join(".", this.propertyPath.Select(e => e.GetPropertyPath())));
  }

  /// <summary>
  /// Adds the property to the path.
  /// </summary>
  /// <typeparam name="TProperty">Property type (or element type of a collection).</typeparam>
  /// <param name="property">Property to add.</param>
  /// <returns>Builder to add nested properties.</returns>
  protected NestedPropertyPathBuilder<TObject, TProperty, TResult> AddToPath<TProperty>(LambdaExpression property)
    where TProperty : class
  {
    this.propertyPath.Add(property.Body);

    return new NestedPropertyPathBuilder<TObject, TProperty, TResult>(this.resultFactory, this.propertyPath);
  }
}
