// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Api.Dto.Abstractions;

namespace Magicalizer.TestHost.Diagnostics;

/// <summary>
/// Magicalized DTO whose entity doesn't have a primary key, so Magicalizer skips it and reports it on the start.
/// </summary>
[Magicalized("api/keyless")]
public class KeylessDto : IDto<KeylessModel>
{
  /// <summary>
  /// Value of the DTO.
  /// </summary>
  public string? Value { get; set; }

  /// <inheritdoc/>
  public KeylessModel ToModel()
  {
    return new KeylessModel {
      Value = this.Value
    };
  }
}
