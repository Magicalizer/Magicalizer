// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using FluentValidation;
using Magicalizer.Api.Dto.Abstractions;
using HttpMethod = Magicalizer.Api.Dto.Abstractions.HttpMethod;

namespace Magicalizer.TestHost.Notes;

/// <summary>
/// Note, served at "/api/notes" and described in the separate OpenAPI document (<see cref="GroupName"/>). The
/// authenticated users can read the notes, the ones that satisfy <see cref="EditPolicyName"/> can change them, and they
/// must satisfy <see cref="DeletePolicyName"/> too to delete them.
/// </summary>
[Magicalized("api/notes", GroupName = GroupName)]
[AuthenticatedOnly(HttpMethod.Get)]
[AuthorizedOnly(EditPolicyName, HttpMethod.Post)]
[AuthorizedOnly(EditPolicyName, HttpMethod.Put)]
[AuthorizedOnly(EditPolicyName, HttpMethod.Patch)]
[AuthorizedOnly(EditPolicyName, HttpMethod.Delete)]
[AuthorizedOnly(DeletePolicyName, HttpMethod.Delete)]
public class NoteDto : IDto<NoteModel>
{
  /// <summary>
  /// Name of the policy that allows to change the notes (the "edit" permission).
  /// </summary>
  public const string EditPolicyName = "Notes.Edit";

  /// <summary>
  /// Name of the policy that allows to delete the notes (the "delete" permission).
  /// </summary>
  public const string DeletePolicyName = "Notes.Delete";

  /// <summary>
  /// Name of the OpenAPI document that describes the notes.
  /// </summary>
  public const string GroupName = "internal";

  /// <summary>
  /// Note ID.
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// Text of the note.
  /// </summary>
  public string? Text { get; set; }

  /// <summary>
  /// Creates an empty DTO.
  /// </summary>
  public NoteDto() { }

  /// <summary>
  /// Creates a DTO from the model.
  /// </summary>
  public NoteDto(NoteModel noteModel)
  {
    this.Id = noteModel.Id;
    this.Text = noteModel.Text;
  }

  /// <inheritdoc/>
  public NoteModel ToModel()
  {
    return new NoteModel {
      Id = this.Id,
      Text = this.Text
    };
  }

  /// <summary>
  /// Validator of the note.
  /// </summary>
  public class Validator : AbstractValidator<NoteDto>
  {
    /// <summary>
    /// Creates the validator.
    /// </summary>
    public Validator()
    {
      this.RuleFor(n => n.Text).NotEmpty();
    }
  }
}
