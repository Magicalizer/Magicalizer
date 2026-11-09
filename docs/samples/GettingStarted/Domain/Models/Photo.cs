using Magicalizer.Domain.Models.Abstractions;

namespace App.Domain.Models;

public class Photo : IModel<Data.Entities.Photo, Filters.PhotoFilter>
{
  public int Id { get; set; }
  public Product? Product { get; set; }
  public string? Filename { get; set; }

  public Photo() { }

  public Photo(Data.Entities.Photo photoEntity) : this(photoEntity, ignoreProduct: false) { }

  public Photo(Data.Entities.Photo photoEntity, bool ignoreProduct)
  {
    this.Id = photoEntity.Id;

    if (!ignoreProduct)
      this.Product = photoEntity.Product == null ? new Product { Id = photoEntity.ProductId } : new Product(photoEntity.Product, ignoreCategory: false, ignorePhotos: true);

    this.Filename = photoEntity.Filename;
  }

  public Data.Entities.Photo ToEntity()
  {
    return new Data.Entities.Photo {
      Id = this.Id,
      ProductId = this.Product?.Id ?? 0,
      Filename = this.Filename
    };
  }
}
