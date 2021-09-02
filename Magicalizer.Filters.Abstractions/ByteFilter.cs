// Copyright © 2021 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions
{
  public class ByteFilter : IFilter
  {
    public bool? IsNull { get; set; }
    public bool? IsNotNull { get; set; }
    new public byte? Equals { get; set; }
    public byte? NotEquals { get; set; }
    public byte? From { get; set; }
    public byte? To { get; set; }

    public ByteFilter() { }

    public ByteFilter(bool? isNull = null, bool? isNotNull = null, byte? equals = null, byte? notEquals = null, byte? from = null, byte? to = null)
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