// Copyright © 2021 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions
{
  public class ShortFilter : IFilter
  {
    public bool? IsNull { get; set; }
    public bool? IsNotNull { get; set; }
    new public short? Equals { get; set; }
    public short? NotEquals { get; set; }
    public short? From { get; set; }
    public short? To { get; set; }

    public ShortFilter() { }

    public ShortFilter(bool? isNull = null, bool? isNotNull = null, short? equals = null, short? notEquals = null, short? from = null, short? to = null)
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