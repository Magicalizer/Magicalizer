using App.Domain.Models;
using App.Filters;
using FluentValidation;
using Magicalizer.Domain.Services;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Filters.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace App.Domain.Services;

public class ProductService : Service<int, Data.Entities.Product, Product, ProductFilter>, IProductService
{
  public ProductService(DbContext dbContext, IEnumerable<IQueryPrefilter<Data.Entities.Product, ProductFilter>>? queryPrefilters = null, IValidator<Product>? validator = null) : base(dbContext, queryPrefilters, validator) { }

  public override Task<Product> CreateAsync(Product model, CancellationToken cancellationToken = default)
  {
    model.Created = DateTime.UtcNow;
    return base.CreateAsync(model, cancellationToken);
  }

  public Task<int> CountArchivedAsync(CancellationToken cancellationToken = default)
  {
    return this.CountAsync(new ProductFilter { IsArchived = new BooleanFilter { Equals = true } }, cancellationToken);
  }

  protected override ProductFilter? GetPreprocessedFilter(ProductFilter? filter = null)
  {
    // The search text is a condition of the name.
    if (filter?.Search != null)
      filter.Name = new StringFilter { Contains = filter.Search };

    return filter;
  }
}
