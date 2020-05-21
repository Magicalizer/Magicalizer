// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions
{
  public class IntegerFilter : IFilter
  {
    new public int? Equals { get; set; }
    public int? From { get; set; }
    public int? To { get; set; }

    public IntegerFilter() { }

    public IntegerFilter(int? equals = null, int? from = null, int? to = null)
    {
      this.Equals = equals;
      this.From = from;
      this.To = to;
    }
  }
}