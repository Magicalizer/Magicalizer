// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Entities.Abstractions;

namespace Magicalizer.TestHost.Diagnostics;

/// <summary>
/// Entity without a primary key, so its model doesn't have a service and its DTO is skipped (Magicalizer reports
/// them on the start).
/// </summary>
public class KeylessEntity : IEntity
{
  /// <summary>
  /// Value of the entity.
  /// </summary>
  public string? Value { get; set; }
}
