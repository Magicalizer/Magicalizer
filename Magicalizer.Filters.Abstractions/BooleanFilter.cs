// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions
{
  public class BooleanFilter : IFilter
  {
    public bool? IsNull { get; set; }
    public bool? IsNotNull { get; set; }
    new public bool? Equals { get; set; }

    public BooleanFilter() { }

    public BooleanFilter(bool? isNull = null, bool? isNotNull = null, bool? equals = null)
    {
      this.IsNull = isNull;
      this.IsNotNull = isNotNull;
      this.Equals = equals;
    }
  }
}