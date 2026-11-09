using Magicalizer.Domain.Models.Abstractions;

namespace App.Domain.Models;

public class Category : IModel<Data.Entities.Category, Filters.CategoryFilter>
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public IEnumerable<Product>? Products { get; set; }

  public Category() { }

  public Category(Data.Entities.Category categoryEntity) : this(categoryEntity, ignoreProducts: false) { }

  public Category(Data.Entities.Category categoryEntity, bool ignoreProducts)
  {
    this.Id = categoryEntity.Id;
    this.Name = categoryEntity.Name;

    if (!ignoreProducts)
      this.Products = categoryEntity.Products?.Select(p => new Product(p, ignoreCategory: true, ignorePhotos: false)).ToList();
  }

  public Data.Entities.Category ToEntity()
  {
    return new Data.Entities.Category {
      Id = this.Id,
      Name = this.Name
    };
  }
}
