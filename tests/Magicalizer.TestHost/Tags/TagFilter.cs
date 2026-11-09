// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Filters.Abstractions;

namespace Magicalizer.TestHost.Tags;

/// <summary>
/// Filter for tags. Only the conditions that are set are applied.
/// </summary>
public class TagFilter : IFilter
{
  /// <summary>
  /// Filters by ID.
  /// </summary>
  public IntegerFilter? Id { get; set; }

  /// <summary>
  /// Filters by name.
  /// </summary>
  public StringFilter? Name { get; set; }
}
