// Copyright © 2026 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Data;

/// <summary>
/// The exception that is thrown when an inclusion property path can't be applied to an entity
/// (one of the properties in the path doesn't exist).
/// </summary>
public class InvalidInclusionException : ArgumentException
{
  /// <summary>
  /// The property path that couldn't be applied.
  /// </summary>
  public string PropertyPath { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="InvalidInclusionException"/> class.
  /// </summary>
  /// <param name="propertyPath">The property path that couldn't be applied.</param>
  /// <param name="reason">The human-readable reason.</param>
  public InvalidInclusionException(string propertyPath, string reason)
    : base($"Unable to include '{propertyPath}': {reason}.")
  {
    this.PropertyPath = propertyPath;
  }
}