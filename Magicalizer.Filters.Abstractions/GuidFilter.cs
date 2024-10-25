// Copyright © 2021 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// A filter for <see cref="Guid"/> properties, allowing for comparisons and null checks.
/// </summary>
public class GuidFilter : IFilter
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
  new public Guid? Equals { get; set; }

  /// <summary>
  /// Specifies if the property value should not be equal to the provided one.
  /// </summary>
  public Guid? NotEquals { get; set; }

  /// <summary>
  /// Specifies if the property value should be one of the provided comma-separated ones.
  /// </summary>
  public IEnumerable<Guid>? In { get; set; }

  /// <summary>
  /// Initializes a new instance of the <see cref="GuidFilter"/> class.
  /// </summary>
  public GuidFilter() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="GuidFilter"/> class with specific conditions.
  /// </summary>
  /// <param name="isNull">If the property value should be null.</param>
  /// <param name="isNotNull">If the property value should be non-null.</param>
  /// <param name="equals">If the property value should be equal to the provided one.</param>
  /// <param name="notEquals">If the property value should not be equal to the provided one.</param>
  /// <param name="in">If the property value should be one of the provided comma-separated ones.</param>
  public GuidFilter(bool? isNull = null, bool? isNotNull = null, Guid? equals = null, Guid? notEquals = null, IEnumerable<Guid>? @in = null)
  {
    this.IsNull = isNull;
    this.IsNotNull = isNotNull;
    this.Equals = equals;
    this.NotEquals = notEquals;
    this.In = @in;
  }
}