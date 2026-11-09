using Magicalizer.Api.Dto.Abstractions;
using HttpMethod = Magicalizer.Api.Dto.Abstractions.HttpMethod;

namespace App.Api.Dto;

[Magicalized("v1/categories")]
[AuthorizedOnly(Policies.EditCatalog, HttpMethod.Post)]
[AuthorizedOnly(Policies.EditCatalog, HttpMethod.Put)]
[AuthorizedOnly(Policies.EditCatalog, HttpMethod.Patch)]
[AuthorizedOnly(Policies.EditCatalog, HttpMethod.Delete)]
public class Category : IDto<Domain.Models.Category>
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public IEnumerable<Product>? Products { get; set; }

  public Category() { }

  public Category(Domain.Models.Category categoryModel) : this(categoryModel, ignoreProducts: false) { }

  public Category(Domain.Models.Category categoryModel, bool ignoreProducts)
  {
    this.Id = categoryModel.Id;
    this.Name = categoryModel.Name;

    if (!ignoreProducts)
      this.Products = categoryModel.Products?.Select(p => new Product(p, ignoreCategory: true, ignorePhotos: false)).ToList();
  }

  public Domain.Models.Category ToModel()
  {
    return new Domain.Models.Category {
      Id = this.Id,
      Name = this.Name
    };
  }
}
