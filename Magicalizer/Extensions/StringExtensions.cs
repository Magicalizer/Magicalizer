// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="string"/> type.
/// </summary>
public static class StringExtensions
{
  /// <summary>
  /// Converts a string to camel case. For example, "SomeValue" becomes "someValue".
  /// If the string contains dots, each segment is converted separately.
  /// </summary>
  /// <param name="value">The string to convert.</param>
  /// <returns>The camel case version of the string, or the original if it's <c>null</c> or empty.</returns>
  public static string ToCamelCase(this string value)
  {
    if (string.IsNullOrEmpty(value))
      return value;

    if (value.Contains('.'))
      return string.Join('.', value.Split('.').Select(ToCamelCase));

    return value[0].ToString().ToLower() + value.Substring(1);
  }

  /// <summary>
  /// Splits a string by commas and removes empty entries.
  /// </summary>
  /// <param name="value">The string to split.</param>
  /// <returns>An array of non-empty segments.</returns>
  public static string[] SplitByComma(this string? value)
  {
    return (value ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);
  }
}