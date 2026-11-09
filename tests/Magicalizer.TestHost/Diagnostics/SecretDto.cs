// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.TestHost.Notes;
using HttpMethod = Magicalizer.Api.Dto.Abstractions.HttpMethod;

namespace Magicalizer.TestHost.Diagnostics;

/// <summary>
/// Another DTO of the notes, which requires a policy that isn't registered, so Magicalizer reports it on the start.
/// </summary>
[Magicalized("api/secrets", IgnoreApi = true)]
[AuthorizedOnly(MissingPolicyName, HttpMethod.Get)]
public class SecretDto : IDto<NoteModel>
{
  /// <summary>
  /// Name of the policy that isn't registered.
  /// </summary>
  public const string MissingPolicyName = "Missing.Policy";

  /// <summary>
  /// Note ID.
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// Creates an empty DTO.
  /// </summary>
  public SecretDto() { }

  /// <summary>
  /// Creates a DTO from the model.
  /// </summary>
  public SecretDto(NoteModel noteModel)
  {
    this.Id = noteModel.Id;
  }

  /// <inheritdoc/>
  public NoteModel ToModel()
  {
    return new NoteModel {
      Id = this.Id
    };
  }
}
