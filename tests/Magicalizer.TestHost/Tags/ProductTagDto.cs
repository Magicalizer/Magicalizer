// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using FluentValidation;
using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.TestHost.Products;

namespace Magicalizer.TestHost.Tags;

/// <summary>
/// Tag of a product, served at "/api/product-tags/{productId}/{tagId}" with the standard API.
/// </summary>
[Magicalized("api/product-tags")]
public class ProductTagDto : IDto<ProductTagModel>
{
  /// <summary>
  /// Product (only with the ID if it isn't included).
  /// </summary>
  public ProductDto? Product { get; set; }

  /// <summary>
  /// Tag (only with the ID if it isn't included).
  /// </summary>
  public TagDto? Tag { get; set; }

  /// <summary>
  /// Position of the tag among the tags of the product.
  /// </summary>
  public int Position { get; set; }

  /// <summary>
  /// Creates an empty DTO.
  /// </summary>
  public ProductTagDto() { }

  /// <summary>
  /// Creates a DTO from the model.
  /// </summary>
  public ProductTagDto(ProductTagModel productTagModel) : this(productTagModel, ignoreProduct: false) { }

  /// <summary>
  /// Creates a DTO from the model.
  /// </summary>
  /// <param name="productTagModel">Model to create the DTO from.</param>
  /// <param name="ignoreProduct">Whether the product is left out (to avoid cyclic references).</param>
  public ProductTagDto(ProductTagModel productTagModel, bool ignoreProduct)
  {
    if (!ignoreProduct)
      this.Product = productTagModel.Product == null ? null : new ProductDto(productTagModel.Product);

    this.Tag = productTagModel.Tag == null ? null : new TagDto(productTagModel.Tag);
    this.Position = productTagModel.Position;
  }

  /// <inheritdoc/>
  public ProductTagModel ToModel()
  {
    return new ProductTagModel {
      Product = this.Product?.ToModel(),
      Tag = this.Tag?.ToModel(),
      Position = this.Position
    };
  }

  /// <summary>
  /// Validator of the tag of a product.
  /// </summary>
  public class Validator : AbstractValidator<ProductTagDto>
  {
    /// <summary>
    /// Creates the validator.
    /// </summary>
    public Validator()
    {
      this.RuleFor(pt => pt.Product).NotNull().DependentRules(() => {
        this.RuleFor(pt => pt.Product!.Id).GreaterThan(0);
      });

      this.RuleFor(pt => pt.Tag).NotNull().DependentRules(() => {
        this.RuleFor(pt => pt.Tag!.Id).GreaterThan(0);
      });

      this.RuleFor(pt => pt.Position).GreaterThanOrEqualTo(0);
    }
  }
}
