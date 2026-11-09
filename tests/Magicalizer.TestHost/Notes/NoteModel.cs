// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Models.Abstractions;

namespace Magicalizer.TestHost.Notes;

/// <summary>
/// Note.
/// </summary>
public class NoteModel : IModel<NoteEntity, NoteFilter>
{
  /// <summary>
  /// Note ID.
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// Text of the note.
  /// </summary>
  public string? Text { get; set; }

  /// <summary>
  /// Creates an empty model.
  /// </summary>
  public NoteModel() { }

  /// <summary>
  /// Creates a model from the entity.
  /// </summary>
  public NoteModel(NoteEntity noteEntity)
  {
    this.Id = noteEntity.Id;
    this.Text = noteEntity.Text;
  }

  /// <inheritdoc/>
  public NoteEntity ToEntity()
  {
    return new NoteEntity {
      Id = this.Id,
      Text = this.Text!
    };
  }
}
