// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using FluentValidation;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.TestHost.Categories;
using Magicalizer.TestHost.Photos;
using Magicalizer.TestHost.Tags;
using Magicalizer.Validators.Abstractions;

namespace Magicalizer.TestHost.Products;

/// <summary>
/// Product with the properties of all the types the filters support.
/// </summary>
public class ProductModel : IModel<ProductEntity, ProductFilter>
{
  /// <summary>
  /// Product ID.
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// Category of the product (only with the ID if it isn't included).
  /// </summary>
  public CategoryModel? Category { get; set; }

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
  public IEnumerable<PhotoModel>? Photos { get; set; }

  /// <summary>
  /// Tags of the product (if they are included).
  /// </summary>
  public IEnumerable<ProductTagModel>? Tags { get; set; }

  /// <summary>
  /// Creates an empty model.
  /// </summary>
  public ProductModel() { }

  /// <summary>
  /// Creates a model from the entity.
  /// </summary>
  public ProductModel(ProductEntity productEntity) : this(productEntity, ignoreCategory: false) { }

  /// <summary>
  /// Creates a model from the entity.
  /// </summary>
  /// <param name="productEntity">Entity to create the model from.</param>
  /// <param name="ignoreCategory">Whether the category is left out (to avoid cyclic references).</param>
  public ProductModel(ProductEntity productEntity, bool ignoreCategory)
  {
    this.Id = productEntity.Id;

    if (!ignoreCategory)
      this.Category = productEntity.Category == null ? new CategoryModel { Id = productEntity.CategoryId } : new CategoryModel(productEntity.Category, ignoreProducts: true);

    this.Name = productEntity.Name;
    this.Price = productEntity.Price;
    this.Quantity = productEntity.Quantity;
    this.IsAvailable = productEntity.IsAvailable;
    this.Created = productEntity.Created;
    this.Code = productEntity.Code;
    this.Description = productEntity.Description;
    this.IsArchived = productEntity.IsArchived;
    this.Diet = productEntity.Diet;
    this.Updated = productEntity.Updated;
    this.Photos = productEntity.Photos?.Select(p => new PhotoModel(p, ignoreProduct: true)).ToList();
    this.Tags = productEntity.Tags?.Select(t => new ProductTagModel(t, ignoreProduct: true)).ToList();
  }

  /// <inheritdoc/>
  /// <remarks>
  /// The category is saved by its ID only, the photos and the tags are saved separately.
  /// </remarks>
  public ProductEntity ToEntity()
  {
    return new ProductEntity {
      Id = this.Id,
      CategoryId = this.Category?.Id ?? 0,
      Name = this.Name!,
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
  /// Validator of the product in the service (the rules the DTO validator doesn't have): a new product can't be created
  /// in the future, and an edited one keeps its creation time.
  /// </summary>
  public class Validator : AbstractValidator<ProductModel>
  {
    /// <summary>
    /// Creates the validator.
    /// </summary>
    public Validator()
    {
      this.RuleFor(p => p.Price).LessThanOrEqualTo(1_000_000);
      this.RuleSet(RuleSetName.Create, () => {
        this.RuleFor(p => p.Created).LessThanOrEqualTo(_ => DateTime.UtcNow);
      });

      this.RuleSet(RuleSetName.Edit, () => {
        this.RuleFor(p => p.Created).NotEqual(default(DateTime));
      });
    }
  }
}
