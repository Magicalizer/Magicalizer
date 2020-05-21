// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions
{
  public class StringFilter : IFilter
  {
    new public string Equals { get; set; }
    public string Contains { get; set; }

    public StringFilter() { }

    public StringFilter(string equals = null, string contains = null)
    {
      this.Equals = equals;
      this.Contains = contains;
    }
  }
}