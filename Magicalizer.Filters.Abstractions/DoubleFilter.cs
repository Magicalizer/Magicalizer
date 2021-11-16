// Copyright © 2021 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions
{
  /// <summary>
  /// Represents a <see cref="double"/> filter.
  /// </summary>
  public class DoubleFilter : IFilter
  {
    /// <summary>
    /// Determines if a property value should be null.
    /// </summary>
    public bool? IsNull { get; set; }

    /// <summary>
    /// Determines if a property value should be non-null.
    /// </summary>
    public bool? IsNotNull { get; set; }

    /// <summary>
    /// Determines if a property value should be equal to the given one.
    /// </summary>
    new public double? Equals { get; set; }

    /// <summary>
    /// Determines if a property value should not be equal to the given one.
    /// </summary>
    public double? NotEquals { get; set; }

    /// <summary>
    /// Determines if a property value should be greater than the given one.
    /// </summary>
    public double? From { get; set; }

    /// <summary>
    /// Determines if a property value should be less than the given one.
    /// </summary>
    public double? To { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DoubleFilter"/> class.
    /// </summary>
    public DoubleFilter() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DoubleFilter"/> class.
    /// </summary>
    /// <param name="isNull">Determines if a property value should be null.</param>
    /// <param name="isNotNull">Determines if a property value should be non-null.</param>
    /// <param name="equals">Determines if a property value should be equal to the given one.</param>
    /// <param name="notEquals">Determines if a property value should not be equal to the given one.</param>
    /// <param name="from">Determines if a property value should be greater than the given one.</param>
    /// <param name="to">Determines if a property value should be less than the given one.</param>
    public DoubleFilter(bool? isNull = null, bool? isNotNull = null, double? equals = null, double? notEquals = null, double? from = null, double? to = null)
    {
      this.IsNull = isNull;
      this.IsNotNull = isNotNull;
      this.Equals = equals;
      this.NotEquals = notEquals;
      this.From = from;
      this.To = to;
    }
  }
}