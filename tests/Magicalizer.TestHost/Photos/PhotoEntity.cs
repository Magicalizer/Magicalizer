// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.TestHost.Products;

namespace Magicalizer.TestHost.Photos;

/// <summary>
/// Photo of a product.
/// </summary>
public class PhotoEntity : IEntity<int>
{
  /// <summary>
  /// Photo ID.
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// ID of the product of the photo.
  /// </summary>
  public int ProductId { get; set; }

  /// <summary>
  /// Filename of the photo (e.g. "margherita.jpg").
  /// </summary>
  public string Filename { get; set; } = null!;

  /// <summary>
  /// Product of the photo (if it's loaded).
  /// </summary>
  public virtual ProductEntity? Product { get; set; }
}
