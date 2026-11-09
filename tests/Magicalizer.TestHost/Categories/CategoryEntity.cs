// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Entities.Abstractions;

namespace Magicalizer.TestHost.Categories;

/// <summary>
/// Category of products.
/// </summary>
public class CategoryEntity : IEntity<int>
{
  /// <summary>
  /// Category ID.
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// Category name (e.g. "Pizza").
  /// </summary>
  public string Name { get; set; } = null!;

  /// <summary>
  /// Description, or <c>null</c> if there is none.
  /// </summary>
  public string? Description { get; set; }

  /// <summary>
  /// Products of the category (if they are loaded).
  /// </summary>
  public virtual ICollection<Products.ProductEntity>? Products { get; set; }
}
