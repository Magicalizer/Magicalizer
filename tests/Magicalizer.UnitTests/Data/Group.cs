// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Entities.Abstractions;

namespace Magicalizer.UnitTests.Data;

/// <summary>
/// Test entity the items refer to.
/// </summary>
public class Group : IEntity<int>
{
  public int Id { get; set; }
  public string? Name { get; set; }
}
