// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Extensions;

/// <summary>
/// Extensions of <see cref="string"/>.
/// </summary>
internal static class StringExtensions
{
  /// <summary>
  /// Splits the string by commas, without empty segments and spaces around them (e.g. "name, -created").
  /// </summary>
  /// <param name="value">String to split.</param>
  /// <returns>Non-empty segments.</returns>
  public static string[] SplitByComma(this string? value)
  {
    return (value ?? string.Empty).Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
  }
}
