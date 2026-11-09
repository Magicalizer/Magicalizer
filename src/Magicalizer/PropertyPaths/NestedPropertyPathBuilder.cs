// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Linq.Expressions;

namespace Magicalizer.PropertyPaths;

/// <summary>
/// Builder of the nested part of a property path.
/// </summary>
/// <typeparam name="TOriginalObject">Root object type.</typeparam>
/// <typeparam name="TObject">Type of the object of the last added property.</typeparam>
/// <typeparam name="TResult">Type of the built result.</typeparam>
public class NestedPropertyPathBuilder<TOriginalObject, TObject, TResult> : PropertyPathBuilderBase<TOriginalObject, TResult>
  where TOriginalObject : class
  where TObject : class
  where TResult : class
{
  /// <summary>
  /// Creates a builder.
  /// </summary>
  /// <param name="resultFactory">Creates the result from the path.</param>
  /// <param name="propertyPath">Path built so far.</param>
  public NestedPropertyPathBuilder(Func<string, TResult> resultFactory, IList<Expression> propertyPath) : base(resultFactory, propertyPath) { }

  /// <summary>
  /// Adds the nested property to the path.
  /// </summary>
  /// <typeparam name="TProperty">Property type (or element type of a collection).</typeparam>
  /// <param name="property">Property to add.</param>
  /// <returns>Builder to add nested properties.</returns>
  public NestedPropertyPathBuilder<TOriginalObject, TProperty, TResult> ThenAdd<TProperty>(Expression<Func<TObject, TProperty?>> property)
    where TProperty : class
  {
    return this.AddToPath<TProperty>(property);
  }

  /// <summary>
  /// Adds the nested property to the path.
  /// </summary>
  /// <typeparam name="TProperty">Property type (or element type of a collection).</typeparam>
  /// <param name="property">Property to add.</param>
  /// <returns>Builder to add nested properties.</returns>
  public NestedPropertyPathBuilder<TOriginalObject, TProperty, TResult> ThenAdd<TProperty>(Expression<Func<TObject, IList<TProperty>?>> property)
    where TProperty : class
  {
    return this.AddToPath<TProperty>(property);
  }

  /// <summary>
  /// Adds the nested property to the path.
  /// </summary>
  /// <typeparam name="TProperty">Property type (or element type of a collection).</typeparam>
  /// <param name="property">Property to add.</param>
  /// <returns>Builder to add nested properties.</returns>
  public NestedPropertyPathBuilder<TOriginalObject, TProperty, TResult> ThenAdd<TProperty>(Expression<Func<TObject, IReadOnlyList<TProperty>?>> property)
    where TProperty : class
  {
    return this.AddToPath<TProperty>(property);
  }

  /// <summary>
  /// Adds the nested property to the path.
  /// </summary>
  /// <typeparam name="TProperty">Property type (or element type of a collection).</typeparam>
  /// <param name="property">Property to add.</param>
  /// <returns>Builder to add nested properties.</returns>
  public NestedPropertyPathBuilder<TOriginalObject, TProperty, TResult> ThenAdd<TProperty>(Expression<Func<TObject, ICollection<TProperty>?>> property)
    where TProperty : class
  {
    return this.AddToPath<TProperty>(property);
  }

  /// <summary>
  /// Adds the nested property to the path.
  /// </summary>
  /// <typeparam name="TProperty">Property type (or element type of a collection).</typeparam>
  /// <param name="property">Property to add.</param>
  /// <returns>Builder to add nested properties.</returns>
  public NestedPropertyPathBuilder<TOriginalObject, TProperty, TResult> ThenAdd<TProperty>(Expression<Func<TObject, IEnumerable<TProperty>?>> property)
    where TProperty : class
  {
    return this.AddToPath<TProperty>(property);
  }
}
