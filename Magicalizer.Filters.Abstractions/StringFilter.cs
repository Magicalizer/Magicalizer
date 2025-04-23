// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// A filter for <see cref="string"/> properties, allowing for comparisons and null checks.
/// </summary>
public class StringFilter : IFilter
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
  new public string? Equals { get; set; }

  /// <summary>
  /// Specifies if the property value should not be equal to the provided one.
  /// </summary>
  public string? NotEquals { get; set; }

  /// <summary>
  /// Specifies if the property value should contain the provided one.
  /// </summary>
  public string? Contains { get; set; }

  /// <summary>
  /// Specifies if the property value should be one of the provided comma-separated ones.
  /// </summary>
  public IEnumerable<string>? In { get; set; }

  /// <summary>
  /// Initializes a new instance of the <see cref="StringFilter"/> class.
  /// </summary>
  public StringFilter() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="StringFilter"/> class with specific conditions.
  /// </summary>
  /// <param name="isNull">If the property value should be null.</param>
  /// <param name="isNotNull">If the property value should be non-null.</param>
  /// <param name="equals">If the property value should be equal to the provided one.</param>
  /// <param name="notEquals">If the property value should not be equal to the provided one.</param>
  /// <param name="contains">If the property value should contain the provided one.</param>
  /// <param name="in">If the property value should be one of the provided comma-separated ones.</param>
  public StringFilter(bool? isNull = null, bool? isNotNull = null, string? equals = null, string? notEquals = null, string? contains = null, IEnumerable<string>? @in = null)
  {
    this.IsNull = isNull;
    this.IsNotNull = isNotNull;
    this.Equals = equals;
    this.NotEquals = notEquals;
    this.Contains = contains;
    this.In = @in;
  }
}