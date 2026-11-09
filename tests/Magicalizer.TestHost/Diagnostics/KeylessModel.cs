// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Models.Abstractions;

namespace Magicalizer.TestHost.Diagnostics;

/// <summary>
/// Model of <see cref="KeylessEntity"/>.
/// </summary>
public class KeylessModel : IModel<KeylessEntity, KeylessFilter>
{
  /// <summary>
  /// Value of the model.
  /// </summary>
  public string? Value { get; set; }

  /// <inheritdoc/>
  public KeylessEntity ToEntity()
  {
    return new KeylessEntity {
      Value = this.Value
    };
  }
}
