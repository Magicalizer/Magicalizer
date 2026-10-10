// Copyright © 2026 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Data;

/// <summary>
/// The exception that is thrown when a sorting property path can't be applied to an entity
/// (the property doesn't exist, is a collection, or is not a scalar value).
/// </summary>
public class InvalidSortingException : ArgumentException
{
  /// <summary>
  /// The property path that couldn't be applied.
  /// </summary>
  public string PropertyPath { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="InvalidSortingException"/> class.
  /// </summary>
  /// <param name="propertyPath">The property path that couldn't be applied.</param>
  /// <param name="reason">The human-readable reason.</param>
  public InvalidSortingException(string propertyPath, string reason)
    : base($"Unable to sort by '{propertyPath}': {reason}.")
  {
    this.PropertyPath = propertyPath;
  }
}