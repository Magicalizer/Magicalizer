// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Entities.Abstractions;

namespace Magicalizer.TestHost.Notes;

/// <summary>
/// Note (an entity protected by the authorization rules).
/// </summary>
public class NoteEntity : IEntity<int>
{
  /// <summary>
  /// Note ID.
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// Text of the note.
  /// </summary>
  public string Text { get; set; } = null!;
}
