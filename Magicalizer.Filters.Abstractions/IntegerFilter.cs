// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions
{
  public class IntegerFilter : IFilter
  {
    public bool? IsNull { get; set; }
    public bool? IsNotNull { get; set; }
    new public int? Equals { get; set; }
    public int? From { get; set; }
    public int? To { get; set; }

    public IntegerFilter() { }

    public IntegerFilter(bool? isNull = null, bool? isNotNull = null, int? equals = null, int? from = null, int? to = null)
    {
      this.IsNull = isNull;
      this.IsNotNull = isNotNull;
      this.Equals = equals;
      this.From = from;
      this.To = to;
    }
  }
}