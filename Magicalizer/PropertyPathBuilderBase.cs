// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Magicalizer.Extensions;

namespace Magicalizer;

/// <summary>
/// Base class for building property paths from expressions.
/// </summary>
/// <typeparam name="TObject">The object type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public abstract class PropertyPathBuilderBase<TObject, TResult>
  where TObject : class
  where TResult : class
{
  protected readonly Func<string, TResult> resultFactory;
  protected readonly IList<Expression> propertyPath;

  public PropertyPathBuilderBase(Func<string, TResult> resultFactory, IList<Expression>? propertyPath = null)
  {
    this.resultFactory = resultFactory;
    this.propertyPath = propertyPath ?? [];
  }

  /// <summary>
  /// Builds the full property path.
  /// </summary>
  public TResult Build()
  {
    return this.resultFactory(string.Join(".", this.propertyPath.Select(e => e.GetPropertyPath())));
  }
}

/// <summary>
/// Builds property paths for a given object and result type.
/// </summary>
/// <typeparam name="TObject">The object type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public class PropertyPathBuilder<TObject, TResult> : PropertyPathBuilderBase<TObject, TResult>
  where TObject : class
  where TResult : class
{
  public PropertyPathBuilder(Func<string, TResult> resultFactory) : base(resultFactory)
  {
  }

  /// <summary>
  /// Adds a property to the path and returns a nested builder for further additions.
  /// </summary>
  public virtual NestedPropertyPathBuilder<TObject, TProperty, TResult> Add<TProperty>(Expression<Func<TObject, TProperty?>> property)
    where TProperty : class
  {
    this.propertyPath.Add(property.Body);
    return new NestedPropertyPathBuilder<TObject, TProperty, TResult>(this.resultFactory, this.propertyPath);
  }

  /// <summary>
  /// Adds a property to the path and returns a nested builder for further additions.
  /// </summary>
  public NestedPropertyPathBuilder<TObject, TProperty, TResult> Add<TProperty>(Expression<Func<TObject, IList<TProperty>?>> property)
    where TProperty : class
  {
    this.propertyPath.Add(property.Body);
    return new NestedPropertyPathBuilder<TObject, TProperty, TResult>(this.resultFactory, this.propertyPath);
  }

  /// <summary>
  /// Adds a property to the path and returns a nested builder for further additions.
  /// </summary>
  public NestedPropertyPathBuilder<TObject, TProperty, TResult> Add<TProperty>(Expression<Func<TObject, ICollection<TProperty>?>> property)
    where TProperty : class
  {
    this.propertyPath.Add(property.Body);
    return new NestedPropertyPathBuilder<TObject, TProperty, TResult>(this.resultFactory, this.propertyPath);
  }

  /// <summary>
  /// Adds a property to the path and returns a nested builder for further additions.
  /// </summary>
  public virtual NestedPropertyPathBuilder<TObject, TProperty, TResult> Add<TProperty>(Expression<Func<TObject, IEnumerable<TProperty>?>> property)
    where TProperty : class
  {
    this.propertyPath.Add(property.Body);
    return new NestedPropertyPathBuilder<TObject, TProperty, TResult>(this.resultFactory, this.propertyPath);
  }
}

/// <summary>
/// Builds nested property paths for a given object and result type.
/// </summary>
/// <typeparam name="TOridinalTObject">The original object type.</typeparam>
/// <typeparam name="TObject">The object type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public class NestedPropertyPathBuilder<TOridinalTObject, TObject, TResult> : PropertyPathBuilderBase<TOridinalTObject, TResult>
  where TOridinalTObject : class
  where TObject : class
  where TResult : class
{
  public NestedPropertyPathBuilder(Func<string, TResult> resultFactory, IList<Expression> propertyPath) : base(resultFactory, propertyPath)
  {
  }

  /// <summary>
  /// Adds a nested property to the path and returns a nested builder for further additions.
  /// </summary>
  public virtual NestedPropertyPathBuilder<TOridinalTObject, TProperty, TResult> ThenAdd<TProperty>(Expression<Func<TObject, TProperty?>> property)
    where TProperty : class
  {
    this.propertyPath.Add(property.Body);
    return new NestedPropertyPathBuilder<TOridinalTObject, TProperty, TResult>(this.resultFactory, this.propertyPath);
  }

  /// <summary>
  /// Adds a nested property to the path and returns a nested builder for further additions.
  /// </summary>
  public NestedPropertyPathBuilder<TOridinalTObject, TProperty, TResult> ThenAdd<TProperty>(Expression<Func<TObject, IList<TProperty>?>> property)
    where TProperty : class
  {
    this.propertyPath.Add(property.Body);
    return new NestedPropertyPathBuilder<TOridinalTObject, TProperty, TResult>(this.resultFactory, this.propertyPath);
  }

  /// <summary>
  /// Adds a nested property to the path and returns a nested builder for further additions.
  /// </summary>
  public NestedPropertyPathBuilder<TOridinalTObject, TProperty, TResult> ThenAdd<TProperty>(Expression<Func<TObject, ICollection<TProperty>?>> property)
    where TProperty : class
  {
    this.propertyPath.Add(property.Body);
    return new NestedPropertyPathBuilder<TOridinalTObject, TProperty, TResult>(this.resultFactory, this.propertyPath);
  }

  /// <summary>
  /// Adds a nested property to the path and returns a nested builder for further additions.
  /// </summary>
  public virtual NestedPropertyPathBuilder<TOridinalTObject, TProperty, TResult> ThenAdd<TProperty>(Expression<Func<TObject, IEnumerable<TProperty>?>> property)
    where TProperty : class
  {
    this.propertyPath.Add(property.Body);
    return new NestedPropertyPathBuilder<TOridinalTObject, TProperty, TResult>(this.resultFactory, this.propertyPath);
  }
}