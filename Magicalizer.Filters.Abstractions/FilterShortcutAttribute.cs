// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Magicalizer.Filters.Abstractions
{
  [AttributeUsage(AttributeTargets.Property)]
  public class FilterShortcutAttribute : Attribute
  {
    public string Path { get; }

    public FilterShortcutAttribute(string path)
    {
      this.Path = path;
    }
  }
}