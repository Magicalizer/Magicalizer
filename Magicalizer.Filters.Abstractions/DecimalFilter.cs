// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions
{
  public class DecimalFilter : IFilter
  {
    public bool? IsNull { get; set; }
    public bool? IsNotNull { get; set; }
    new public decimal? Equals { get; set; }
    public decimal? NotEquals { get; set; }
    public decimal? From { get; set; }
    public decimal? To { get; set; }

    public DecimalFilter() { }

    public DecimalFilter(bool? isNull = null, bool? isNotNull = null, decimal? equals = null, decimal? notEquals = null, decimal? from = null, decimal? to = null)
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