// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// A filter for <see cref="bool"/> properties, allowing for comparisons and null checks.
/// </summary>
public class BooleanFilter : IFilter
{
  /// <summary>
  /// Specifies if the property value should be null.
  /// </summary>
  public bool? IsNull { get; set; }

  /// <summary>
  /// Specifies if the property value should be non-null.
  /// </summary>
  public bool? IsNotNull { get; set; }

  /// <summary>
  /// Specifies if the property value should be equal to the provided one.
  /// </summary>
  new public bool? Equals { get; set; }

  /// <summary>
  /// Initializes a new instance of the <see cref="BooleanFilter"/> class.
  /// </summary>
  public BooleanFilter() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="BooleanFilter"/> class with specific conditions.
  /// </summary>
  /// <param name="isNull">If the property value should be null.</param>
  /// <param name="isNotNull">If the property value should be non-null.</param>
  /// <param name="equals">If the property value should be equal to the provided one.</param>
  public BooleanFilter(bool? isNull = null, bool? isNotNull = null, bool? equals = null)
  {
    this.IsNull = isNull;
    this.IsNotNull = isNotNull;
    this.Equals = equals;
  }
}