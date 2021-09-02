// Copyright © 2021 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Magicalizer.Filters.Abstractions
{
  public class GuidFilter : IFilter
  {
    public Guid? IsNull { get; set; }
    public Guid? IsNotNull { get; set; }
    new public Guid? Equals { get; set; }
    public Guid? NotEquals { get; set; }

    public GuidFilter() { }

    public GuidFilter(Guid? isNull = null, Guid? isNotNull = null, Guid? equals = null, Guid? notEquals = null)
    {
      this.IsNull = isNull;
      this.IsNotNull = isNotNull;
      this.Equals = equals;
      this.NotEquals = notEquals;
    }
  }
}