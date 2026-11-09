// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Entities.Abstractions;

namespace Magicalizer.TestHost.Tags;

/// <summary>
/// Tag of products.
/// </summary>
public class TagEntity : IEntity<int>
{
  /// <summary>
  /// Tag ID.
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// Tag name (e.g. "Hot").
  /// </summary>
  public string Name { get; set; } = null!;
}
