// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.TestHost.Notes;

namespace Magicalizer.TestHost.Diagnostics;

/// <summary>
/// Another DTO of the notes without a constructor that takes the model, so its controller can't return it
/// (Magicalizer reports it on the start).
/// </summary>
[Magicalized("api/constructorless", IgnoreApi = true)]
public class ConstructorlessDto : IDto<NoteModel>
{
  /// <summary>
  /// Note ID.
  /// </summary>
  public int Id { get; set; }

  /// <inheritdoc/>
  public NoteModel ToModel()
  {
    return new NoteModel {
      Id = this.Id
    };
  }
}
