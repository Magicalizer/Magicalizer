// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Magicalizer.Filters.Abstractions
{
  public class DateTimeFilter : IFilter
  {
    new public DateTime? Equals { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public DateTimeFilter() { }

    public DateTimeFilter(DateTime? equals = null, DateTime? from = null, DateTime? to = null)
    {
      this.Equals = equals;
      this.From = from;
      this.To = to;
    }
  }
}