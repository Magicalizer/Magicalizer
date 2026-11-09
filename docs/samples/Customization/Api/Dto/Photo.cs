using Magicalizer.Api.Dto.Abstractions;
using HttpMethod = Magicalizer.Api.Dto.Abstractions.HttpMethod;

namespace App.Api.Dto;

[Magicalized("v1/photos", HttpMethod.Get, HttpMethod.Post, HttpMethod.Delete, GroupName = "internal")]
[AuthenticatedOnly]
public class Photo : IDto<Domain.Models.Photo>
{
  public int Id { get; set; }
  public Product? Product { get; set; }
  public string? Filename { get; set; }

  public Photo() { }

  public Photo(Domain.Models.Photo photoModel) : this(photoModel, ignoreProduct: false) { }

  public Photo(Domain.Models.Photo photoModel, bool ignoreProduct)
  {
    this.Id = photoModel.Id;

    if (!ignoreProduct)
      this.Product = photoModel.Product == null ? null : new Product(photoModel.Product, ignoreCategory: false, ignorePhotos: true);

    this.Filename = photoModel.Filename;
  }

  public Domain.Models.Photo ToModel()
  {
    return new Domain.Models.Photo {
      Id = this.Id,
      Product = this.Product?.ToModel(),
      Filename = this.Filename
    };
  }
}
