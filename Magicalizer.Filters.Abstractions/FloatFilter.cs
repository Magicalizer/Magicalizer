// Copyright © 2021 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions
{
  public class FloatFilter : IFilter
  {
    public bool? IsNull { get; set; }
    public bool? IsNotNull { get; set; }
    new public float? Equals { get; set; }
    public float? NotEquals { get; set; }
    public float? From { get; set; }
    public float? To { get; set; }

    public FloatFilter() { }

    public FloatFilter(bool? isNull = null, bool? isNotNull = null, float? equals = null, float? notEquals = null, float? from = null, float? to = null)
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