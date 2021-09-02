// Copyright © 2021 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions
{
  public class LongFilter : IFilter
  {
    public bool? IsNull { get; set; }
    public bool? IsNotNull { get; set; }
    new public long? Equals { get; set; }
    public long? NotEquals { get; set; }
    public long? From { get; set; }
    public long? To { get; set; }

    public LongFilter() { }

    public LongFilter(bool? isNull = null, bool? isNotNull = null, long? equals = null, long? notEquals = null, long? from = null, long? to = null)
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