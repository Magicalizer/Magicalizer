// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.TestHost.Products;

namespace Magicalizer.TestHost.Tags;

/// <summary>
/// Tag of a product (an entity with a composite primary key of two properties).
/// </summary>
public class ProductTagEntity : IEntity<int, int>
{
  /// <summary>
  /// ID of the product (the first primary key property).
  /// </summary>
  public int ProductId { get; set; }

  /// <summary>
  /// ID of the tag (the second primary key property).
  /// </summary>
  public int TagId { get; set; }

  /// <summary>
  /// Position of the tag among the tags of the product.
  /// </summary>
  public int Position { get; set; }

  /// <summary>
  /// Product (if it's loaded).
  /// </summary>
  public virtual ProductEntity? Product { get; set; }

  /// <summary>
  /// Tag (if it's loaded).
  /// </summary>
  public virtual TagEntity? Tag { get; set; }
}
