// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Magicalizer.Filters.Abstractions
{
  public class DateTimeFilter : IFilter
  {
    public bool? IsNull { get; set; }
    public bool? IsNotNull { get; set; }
    new public DateTime? Equals { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public DateTimeFilter() { }

    public DateTimeFilter(bool? isNull = null, bool? isNotNull = null, DateTime? equals = null, DateTime? from = null, DateTime? to = null)
    {
      this.IsNull = isNull;
      this.IsNotNull = isNotNull;
      this.Equals = equals;
      this.From = from;
      this.To = to;
    }
  }
}