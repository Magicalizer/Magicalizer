// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Data;

/// <summary>
/// Exception that is thrown if an inclusion property path can't be applied to an entity (e.g. one of the path
/// properties doesn't exist).
/// </summary>
public class InvalidInclusionException : ArgumentException
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
  public InvalidInclusionException(string propertyPath, string reason)
    : base($"Unable to include '{propertyPath}': {reason}.")
  {
    this.PropertyPath = propertyPath;
  }
}
