// Copyright © 2021 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// A filter for <see cref="float"/> properties, allowing for comparisons and null checks.
/// </summary>
public class FloatFilter : IFilter
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
  new public float? Equals { get; set; }

  /// <summary>
  /// Specifies if the property value should not be equal to the provided one.
  /// </summary>
  public float? NotEquals { get; set; }

  /// <summary>
  /// Specifies if the property value should be greater than the provided one.
  /// </summary>
  public float? From { get; set; }

  /// <summary>
  /// Specifies if the property value should be less than the provided one.
  /// </summary>
  public float? To { get; set; }

  /// <summary>
  /// Specifies if the property value should be one of the provided comma-separated ones.
  /// </summary>
  public IEnumerable<float>? In { get; set; }

  /// <summary>
  /// Initializes a new instance of the <see cref="FloatFilter"/> class.
  /// </summary>
  public FloatFilter() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="FloatFilter"/> class with specific conditions.
  /// </summary>
  /// <param name="isNull">If the property value should be null.</param>
  /// <param name="isNotNull">If the property value should be non-null.</param>
  /// <param name="equals">If the property value should be equal to the provided one.</param>
  /// <param name="notEquals">If the property value should not be equal to the provided one.</param>
  /// <param name="from">If the property value should be greater than the provided one.</param>
  /// <param name="to">If the property value should be less than the provided one.</param>
  /// <param name="in">If the property value should be one of the provided comma-separated ones.</param>
  public FloatFilter(bool? isNull = null, bool? isNotNull = null, float? equals = null, float? notEquals = null, float? from = null, float? to = null, IEnumerable<float>? @in = null)
  {
    this.IsNull = isNull;
    this.IsNotNull = isNotNull;
    this.Equals = equals;
    this.NotEquals = notEquals;
    this.From = from;
    this.To = to;
    this.In = @in;
  }
}