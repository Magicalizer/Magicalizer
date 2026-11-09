using Magicalizer.Domain.Models.Abstractions;

namespace App.Domain.Models;

public class Product : IModel<Data.Entities.Product, Filters.ProductFilter>
{
  public int Id { get; set; }
  public Category? Category { get; set; }
  public string? Name { get; set; }
  public decimal Price { get; set; }
  public DateTime Created { get; set; }
  public IEnumerable<Photo>? Photos { get; set; }

  public Product() { }

  public Product(Data.Entities.Product productEntity) : this(productEntity, ignoreCategory: false, ignorePhotos: false) { }

  public Product(Data.Entities.Product productEntity, bool ignoreCategory, bool ignorePhotos)
  {
    this.Id = productEntity.Id;

    if (!ignoreCategory)
      this.Category = productEntity.Category == null ? new Category { Id = productEntity.CategoryId } : new Category(productEntity.Category, ignoreProducts: true);

    this.Name = productEntity.Name;
    this.Price = productEntity.Price;
    this.Created = productEntity.Created;

    if (!ignorePhotos)
      this.Photos = productEntity.Photos?.Select(p => new Photo(p, ignoreProduct: true)).ToList();
  }

  public Data.Entities.Product ToEntity()
  {
    return new Data.Entities.Product {
      Id = this.Id,
      CategoryId = this.Category?.Id ?? 0,
      Name = this.Name,
      Price = this.Price,
      Created = this.Created == default ? DateTime.UtcNow : this.Created
    };
  }
}
