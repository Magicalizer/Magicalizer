// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using FluentValidation;
using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.TestHost.Categories;
using Magicalizer.TestHost.Photos;
using Magicalizer.TestHost.Tags;
using Magicalizer.Validators.Abstractions;

namespace Magicalizer.TestHost.Products;

/// <summary>
/// Product, served at "/api/products" with the standard API (open for everyone, but the archived products are hidden,
/// see <see cref="ArchivedProductPrefilter"/>).
/// </summary>
[Magicalized("api/products")]
public class ProductDto : IDto<ProductModel>
{
  /// <summary>
  /// Product ID.
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// Category of the product (only with the ID if it isn't included).
  /// </summary>
  public CategoryDto? Category { get; set; }

  /// <summary>
  /// Product name (e.g. "Margherita").
  /// </summary>
  public string? Name { get; set; }

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
  /// Creation time (UTC), the current one if it isn't set on creation.
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
  /// Whether the product is archived.
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
  /// Photos of the product (if they are included).
  /// </summary>
  public IEnumerable<PhotoDto>? Photos { get; set; }

  /// <summary>
  /// Tags of the product (if they are included).
  /// </summary>
  public IEnumerable<ProductTagDto>? Tags { get; set; }

  /// <summary>
  /// Creates an empty DTO.
  /// </summary>
  public ProductDto() { }

  /// <summary>
  /// Creates a DTO from the model.
  /// </summary>
  public ProductDto(ProductModel productModel) : this(productModel, ignoreCategory: false) { }

  /// <summary>
  /// Creates a DTO from the model.
  /// </summary>
  /// <param name="productModel">Model to create the DTO from.</param>
  /// <param name="ignoreCategory">Whether the category is left out (to avoid cyclic references).</param>
  public ProductDto(ProductModel productModel, bool ignoreCategory)
  {
    this.Id = productModel.Id;

    if (!ignoreCategory)
      this.Category = productModel.Category == null ? null : new CategoryDto(productModel.Category, ignoreProducts: true);

    this.Name = productModel.Name;
    this.Price = productModel.Price;
    this.Quantity = productModel.Quantity;
    this.IsAvailable = productModel.IsAvailable;
    this.Created = productModel.Created;
    this.Code = productModel.Code;
    this.Description = productModel.Description;
    this.IsArchived = productModel.IsArchived;
    this.Diet = productModel.Diet;
    this.Updated = productModel.Updated;
    this.Photos = productModel.Photos?.Select(p => new PhotoDto(p, ignoreProduct: true)).ToList();
    this.Tags = productModel.Tags?.Select(t => new ProductTagDto(t, ignoreProduct: true)).ToList();
  }

  /// <inheritdoc/>
  public ProductModel ToModel()
  {
    return new ProductModel {
      Id = this.Id,
      Category = this.Category?.ToModel(),
      Name = this.Name,
      Price = this.Price,
      Quantity = this.Quantity,
      IsAvailable = this.IsAvailable,
      Created = this.Created,
      Code = this.Code,
      Description = this.Description,
      IsArchived = this.IsArchived,
      Diet = this.Diet,
      Updated = this.Updated
    };
  }

  /// <summary>
  /// Validator of the product.
  /// </summary>
  public class Validator : AbstractValidator<ProductDto>
  {
    /// <summary>
    /// Creates the validator.
    /// </summary>
    public Validator()
    {
      this.RuleFor(p => p.Category).NotNull().DependentRules(() => {
        this.RuleFor(p => p.Category!.Id).GreaterThan(0);
      });

      this.RuleFor(p => p.Name).NotEmpty().MaximumLength(64);
      this.RuleFor(p => p.Price).GreaterThanOrEqualTo(0);
      this.RuleFor(p => p.Quantity).GreaterThanOrEqualTo(0);
      this.RuleSet(RuleSetName.Edit, () => {
        this.RuleFor(p => p.Id).GreaterThan(0);
      });
    }
  }
}
