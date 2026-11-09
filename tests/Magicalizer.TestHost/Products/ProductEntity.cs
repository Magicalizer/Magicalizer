// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.TestHost.Categories;
using Magicalizer.TestHost.Photos;
using Magicalizer.TestHost.Tags;

namespace Magicalizer.TestHost.Products;

/// <summary>
/// Product with the properties of all the types the filters support.
/// </summary>
public class ProductEntity : IEntity<int>
{
  /// <summary>
  /// Product ID.
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// ID of the category of the product.
  /// </summary>
  public int CategoryId { get; set; }

  /// <summary>
  /// Product name (e.g. "Margherita").
  /// </summary>
  public string Name { get; set; } = null!;

  /// <summary>
  /// Price.
  /// </summary>
  public decimal Price { get; set; }

  /// <summary>
  /// Quantity in stock.
  /// </summary>
  public int Quantity { get; set; }

  /// <summary>
  /// Whether the product can be ordered.
  /// </summary>
  public bool IsAvailable { get; set; }

  /// <summary>
  /// Creation time (UTC).
  /// </summary>
  public DateTime Created { get; set; }

  /// <summary>
  /// Code of the product in an external system.
  /// </summary>
  public Guid Code { get; set; }

  /// <summary>
  /// Description, or <c>null</c> if there is none.
  /// </summary>
  public string? Description { get; set; }

  /// <summary>
  /// Whether the product is archived (only the users with the "archive" permission see it).
  /// </summary>
  public bool IsArchived { get; set; }

  /// <summary>
  /// Diet the product suits.
  /// </summary>
  public Diet Diet { get; set; }

  /// <summary>
  /// Time of the last update, or <c>null</c> if the product isn't updated.
  /// </summary>
  public DateTimeOffset? Updated { get; set; }

  /// <summary>
  /// Category of the product (if it's loaded).
  /// </summary>
  public virtual CategoryEntity? Category { get; set; }

  /// <summary>
  /// Photos of the product (if they are loaded).
  /// </summary>
  public virtual ICollection<PhotoEntity>? Photos { get; set; }

  /// <summary>
  /// Tags of the product (if they are loaded).
  /// </summary>
  public virtual ICollection<ProductTagEntity>? Tags { get; set; }
}
