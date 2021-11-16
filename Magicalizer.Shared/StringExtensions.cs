// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace Magicalizer
{
  /// <summary>
  /// Contains the extension methods of the <see cref="string"/>.
  /// </summary>
  public static class StringExtensions
  {
    /// <summary>
    /// Converts a given string into a camel case. Example: "SomeValue" => "someValue".
    /// </summary>
    /// <param name="value">A string to convert.</param>
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