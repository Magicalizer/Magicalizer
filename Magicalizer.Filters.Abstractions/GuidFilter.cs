// Copyright © 2021 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Magicalizer.Filters.Abstractions
{
  /// <summary>
  /// Represents a <see cref="Guid"/> filter.
  /// </summary>
  public class GuidFilter : IFilter
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
    new public Guid? Equals { get; set; }

    /// <summary>
    /// Determines if a property value should not be equal to the given one.
    /// </summary>
    public Guid? NotEquals { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GuidFilter"/> class.
    /// </summary>
    public GuidFilter() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GuidFilter"/> class.
    /// </summary>
    /// <param name="isNull">Determines if a property value should be null.</param>
    /// <param name="isNotNull">Determines if a property value should be non-null.</param>
    /// <param name="equals">Determines if a property value should be equal to the given one.</param>
    /// <param name="notEquals">Determines if a property value should not be equal to the given one.</param>
    public GuidFilter(bool? isNull = null, bool? isNotNull = null, Guid? equals = null, Guid? notEquals = null)
    {
      this.IsNull = isNull;
      this.IsNotNull = isNotNull;
      this.Equals = equals;
      this.NotEquals = notEquals;
    }
  }
}