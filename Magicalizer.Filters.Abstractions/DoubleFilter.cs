// Copyright © 2021 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions
{
  public class DoubleFilter : IFilter
  {
    public bool? IsNull { get; set; }
    public bool? IsNotNull { get; set; }
    new public double? Equals { get; set; }
    public double? NotEquals { get; set; }
    public double? From { get; set; }
    public double? To { get; set; }

    public DoubleFilter() { }

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