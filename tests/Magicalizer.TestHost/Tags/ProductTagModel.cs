// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.TestHost.Products;

namespace Magicalizer.TestHost.Tags;

/// <summary>
/// Tag of a product.
/// </summary>
public class ProductTagModel : IModel<ProductTagEntity, ProductTagFilter>
{
  /// <summary>
  /// Product (only with the ID if it isn't included).
  /// </summary>
  public ProductModel? Product { get; set; }

  /// <summary>
  /// Tag (only with the ID if it isn't included).
  /// </summary>
  public TagModel? Tag { get; set; }

  /// <summary>
  /// Position of the tag among the tags of the product.
  /// </summary>
  public int Position { get; set; }

  /// <summary>
  /// Creates an empty model.
  /// </summary>
  public ProductTagModel() { }

  /// <summary>
  /// Creates a model from the entity.
  /// </summary>
  public ProductTagModel(ProductTagEntity productTagEntity) : this(productTagEntity, ignoreProduct: false) { }

  /// <summary>
  /// Creates a model from the entity.
  /// </summary>
  /// <param name="productTagEntity">Entity to create the model from.</param>
  /// <param name="ignoreProduct">Whether the product is left out (to avoid cyclic references).</param>
  public ProductTagModel(ProductTagEntity productTagEntity, bool ignoreProduct)
  {
    if (!ignoreProduct)
      this.Product = productTagEntity.Product == null ? new ProductModel { Id = productTagEntity.ProductId } : new ProductModel(productTagEntity.Product);

    this.Tag = productTagEntity.Tag == null ? new TagModel { Id = productTagEntity.TagId } : new TagModel(productTagEntity.Tag);
    this.Position = productTagEntity.Position;
  }

  /// <inheritdoc/>
  public ProductTagEntity ToEntity()
  {
    return new ProductTagEntity {
      ProductId = this.Product?.Id ?? 0,
      TagId = this.Tag?.Id ?? 0,
      Position = this.Position
    };
  }
}
