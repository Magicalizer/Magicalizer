// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Filters.Abstractions;

namespace Magicalizer.TestHost.Notes;

/// <summary>
/// Filter for notes. Only the conditions that are set are applied.
/// </summary>
public class NoteFilter : IFilter
{
  /// <summary>
  /// Filters by ID.
  /// </summary>
  public IntegerFilter? Id { get; set; }

  /// <summary>
  /// Filters by text.
  /// </summary>
  public StringFilter? Text { get; set; }
}
