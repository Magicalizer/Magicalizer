// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Data;

/// <summary>
/// Exception that is thrown if a sorting property path can't be applied to an entity (e.g. the path is malformed, or one
/// of its properties doesn't exist or is a collection, or the last one isn't a scalar value).
/// </summary>
public class InvalidSortingException : ArgumentException
{
  /// <summary>
  /// Property path that can't be applied.
  /// </summary>
  public string PropertyPath { get; }

  /// <summary>
  /// Creates an exception.
  /// </summary>
  /// <param name="propertyPath">Property path that can't be applied.</param>
  /// <param name="reason">Human-readable reason.</param>
  public InvalidSortingException(string propertyPath, string reason)
    : base($"Unable to sort by '{propertyPath}': {reason}.")
  {
    this.PropertyPath = propertyPath;
  }
}
