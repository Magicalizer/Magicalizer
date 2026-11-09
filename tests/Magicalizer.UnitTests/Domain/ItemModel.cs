// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Models.Abstractions;

namespace Magicalizer.UnitTests.Domain;

/// <summary>
/// Test model.
/// </summary>
public class ItemModel : IModel
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public ItemModel? Parent { get; set; }
  public IReadOnlyList<ItemModel>? Children { get; set; }
  public IList<ItemModel>? List { get; set; }
  public ICollection<ItemModel>? Collection { get; set; }
  public IEnumerable<ItemModel>? Enumerable { get; set; }
}
