// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace Magicalizer
{
  public static class StringExtensions
  {
    public static string ToCamelCase(this string value)
    {
      if (string.IsNullOrEmpty(value))
        return value;

      if (value.Contains("."))
        return string.Join(".", value.Split('.').Select(s => s.ToCamelCase()));

      return value[0].ToString().ToLower() + value.Substring(1);
    }
  }
}