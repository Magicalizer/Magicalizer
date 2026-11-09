using App.Domain.Models;
using App.Filters;
using Magicalizer.Domain.Services.Abstractions;

namespace App.Domain.Services;

public interface IProductService : IService<int, Product, ProductFilter>
{
  Task<int> CountArchivedAsync(CancellationToken cancellationToken = default);
}
