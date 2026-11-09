// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Linq.Expressions;

namespace Magicalizer.PropertyPaths;

/// <summary>
/// Builder of a property path from the root object.
/// </summary>
/// <typeparam name="TObject">Root object type.</typeparam>
/// <typeparam name="TResult">Type of the built result.</typeparam>
public class PropertyPathBuilder<TObject, TResult> : PropertyPathBuilderBase<TObject, TResult>
  where TObject : class
  where TResult : class
{
  /// <summary>
  /// Creates a builder.
  /// </summary>
  /// <param name="resultFactory">Creates the result from the path.</param>
  public PropertyPathBuilder(Func<string, TResult> resultFactory) : base(resultFactory) { }

  /// <summary>
  /// Adds the property to the path.
  /// </summary>
  /// <typeparam name="TProperty">Property type (or element type of a collection).</typeparam>
  /// <param name="property">Property to add.</param>
  /// <returns>Builder to add nested properties.</returns>
  public NestedPropertyPathBuilder<TObject, TProperty, TResult> Add<TProperty>(Expression<Func<TObject, TProperty?>> property)
    where TProperty : class
  {
    return this.AddToPath<TProperty>(property);
  }

  /// <summary>
  /// Adds the property to the path.
  /// </summary>
  /// <typeparam name="TProperty">Property type (or element type of a collection).</typeparam>
  /// <param name="property">Property to add.</param>
  /// <returns>Builder to add nested properties.</returns>
  public NestedPropertyPathBuilder<TObject, TProperty, TResult> Add<TProperty>(Expression<Func<TObject, IList<TProperty>?>> property)
    where TProperty : class
  {
    return this.AddToPath<TProperty>(property);
  }

  /// <summary>
  /// Adds the property to the path.
  /// </summary>
  /// <typeparam name="TProperty">Property type (or element type of a collection).</typeparam>
  /// <param name="property">Property to add.</param>
  /// <returns>Builder to add nested properties.</returns>
  public NestedPropertyPathBuilder<TObject, TProperty, TResult> Add<TProperty>(Expression<Func<TObject, IReadOnlyList<TProperty>?>> property)
    where TProperty : class
  {
    return this.AddToPath<TProperty>(property);
  }

  /// <summary>
  /// Adds the property to the path.
  /// </summary>
  /// <typeparam name="TProperty">Property type (or element type of a collection).</typeparam>
  /// <param name="property">Property to add.</param>
  /// <returns>Builder to add nested properties.</returns>
  public NestedPropertyPathBuilder<TObject, TProperty, TResult> Add<TProperty>(Expression<Func<TObject, ICollection<TProperty>?>> property)
    where TProperty : class
  {
    return this.AddToPath<TProperty>(property);
  }

  /// <summary>
  /// Adds the property to the path.
  /// </summary>
  /// <typeparam name="TProperty">Property type (or element type of a collection).</typeparam>
  /// <param name="property">Property to add.</param>
  /// <returns>Builder to add nested properties.</returns>
  public NestedPropertyPathBuilder<TObject, TProperty, TResult> Add<TProperty>(Expression<Func<TObject, IEnumerable<TProperty>?>> property)
    where TProperty : class
  {
    return this.AddToPath<TProperty>(property);
  }
}
