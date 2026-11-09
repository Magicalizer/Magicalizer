// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.TestHost.Products;

namespace Magicalizer.TestHost.Photos;

/// <summary>
/// Photo of a product.
/// </summary>
public class PhotoModel : IModel<PhotoEntity, PhotoFilter>
{
  /// <summary>
  /// Photo ID.
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// Product of the photo (only with the ID if it isn't included).
  /// </summary>
  public ProductModel? Product { get; set; }

  /// <summary>
  /// Filename of the photo (e.g. "margherita.jpg").
  /// </summary>
  public string? Filename { get; set; }

  /// <summary>
  /// Creates an empty model.
  /// </summary>
  public PhotoModel() { }

  /// <summary>
  /// Creates a model from the entity.
  /// </summary>
  public PhotoModel(PhotoEntity photoEntity) : this(photoEntity, ignoreProduct: false) { }

  /// <summary>
  /// Creates a model from the entity.
  /// </summary>
  /// <param name="photoEntity">Entity to create the model from.</param>
  /// <param name="ignoreProduct">Whether the product is left out (to avoid cyclic references).</param>
  public PhotoModel(PhotoEntity photoEntity, bool ignoreProduct)
  {
    this.Id = photoEntity.Id;

    if (!ignoreProduct)
      this.Product = photoEntity.Product == null ? new ProductModel { Id = photoEntity.ProductId } : new ProductModel(photoEntity.Product);

    this.Filename = photoEntity.Filename;
  }

  /// <inheritdoc/>
  public PhotoEntity ToEntity()
  {
    return new PhotoEntity {
      Id = this.Id,
      ProductId = this.Product?.Id ?? 0,
      Filename = this.Filename!
    };
  }
}
