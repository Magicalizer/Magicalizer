using Magicalizer.Api.Dto.Abstractions;

namespace App.Api.Dto;

[Magicalized("v1/products")]
public class Product : IDto<Domain.Models.Product>
{
  public int Id { get; set; }
  public Category? Category { get; set; }
  public string? Name { get; set; }
  public decimal Price { get; set; }
  public DateTime Created { get; set; }
  public bool IsArchived { get; set; }
  public IEnumerable<Photo>? Photos { get; set; }

  public Product() { }

  public Product(Domain.Models.Product productModel) : this(productModel, ignoreCategory: false, ignorePhotos: false) { }

  public Product(Domain.Models.Product productModel, bool ignoreCategory, bool ignorePhotos)
  {
    this.Id = productModel.Id;

    if (!ignoreCategory)
      this.Category = productModel.Category == null ? null : new Category(productModel.Category, ignoreProducts: true);

    this.Name = productModel.Name;
    this.Price = productModel.Price;
    this.Created = productModel.Created;
    this.IsArchived = productModel.IsArchived;

    if (!ignorePhotos)
      this.Photos = productModel.Photos?.Select(p => new Photo(p, ignoreProduct: true)).ToList();
  }

  public Domain.Models.Product ToModel()
  {
    return new Domain.Models.Product {
      Id = this.Id,
      Category = this.Category?.ToModel(),
      Name = this.Name,
      Price = this.Price,
      Created = this.Created,
      IsArchived = this.IsArchived
    };
  }
}
