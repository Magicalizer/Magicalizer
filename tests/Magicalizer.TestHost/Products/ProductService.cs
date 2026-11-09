// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using FluentValidation;
using Magicalizer.Domain.Services;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Filters.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Magicalizer.TestHost.Products;

/// <summary>
/// Custom service of products: it's registered by Magicalizer instead of the default one. It converts the search text
/// to the name condition and sets the creation time of a new product if it isn't set.
/// </summary>
public class ProductService : Service<int, ProductEntity, ProductModel, ProductFilter>, IProductService
{
  /// <summary>
  /// Creates the service.
  /// </summary>
  /// <param name="dbContext">Database context.</param>
  /// <param name="queryPrefilters">Query prefilters that apply initial restrictions to queries.</param>
  /// <param name="validator">Model validator.</param>
  public ProductService(DbContext dbContext, IEnumerable<IQueryPrefilter<ProductEntity, ProductFilter>>? queryPrefilters = null, IValidator<ProductModel>? validator = null) : base(dbContext, queryPrefilters, validator) { }

  /// <inheritdoc/>
  public override Task<ProductModel> CreateAsync(ProductModel model, CancellationToken cancellationToken = default)
  {
    if (model.Created == default)
      model.Created = DateTime.UtcNow;

    return base.CreateAsync(model, cancellationToken);
  }

  /// <inheritdoc/>
  protected override ProductFilter? GetPreprocessedFilter(ProductFilter? filter = null)
  {
    if (filter?.Search != null)
      filter.Name = new StringFilter(contains: filter.Search);

    return filter;
  }
}
