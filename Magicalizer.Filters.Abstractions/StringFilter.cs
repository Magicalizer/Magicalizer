// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions
{
  public class StringFilter : IFilter
  {
    public bool? IsNull { get; set; }
    public bool? IsNotNull { get; set; }
    new public string Equals { get; set; }
    public string NotEquals { get; set; }
    public string Contains { get; set; }

    public StringFilter() { }

    public StringFilter(bool? isNull = null, bool? isNotNull = null, string equals = null, string notEquals = null, string contains = null)
    {
      this.IsNull = isNull;
      this.IsNotNull = isNotNull;
      this.Equals = equals;
      this.NotEquals = notEquals;
      this.Contains = contains;
    }
  }
}