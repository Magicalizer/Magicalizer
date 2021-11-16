// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions
{
  /// <summary>
  /// Represents a <see cref="bool"/> filter.
  /// </summary>
  public class BooleanFilter : IFilter
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
    new public bool? Equals { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BooleanFilter"/> class.
    /// </summary>
    public BooleanFilter() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BooleanFilter"/> class.
    /// </summary>
    /// <param name="isNull">Determines if a property value should be null.</param>
    /// <param name="isNotNull">Determines if a property value should be equal to the given one.</param>
    /// <param name="equals">Initializes a new instance of the <see cref="BooleanFilter"/> class.</param>
    public BooleanFilter(bool? isNull = null, bool? isNotNull = null, bool? equals = null)
    {
      this.IsNull = isNull;
      this.IsNotNull = isNotNull;
      this.Equals = equals;
    }
  }
}