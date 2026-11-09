// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using FluentValidation;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Filters.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Magicalizer.Domain.Services;

/// <summary>
/// Service of models with a primary key of one property.
/// </summary>
/// <typeparam name="TKey">Primary key type.</typeparam>
/// <typeparam name="TEntity">Entity type.</typeparam>
/// <typeparam name="TModel">Model type.</typeparam>
/// <typeparam name="TFilter">Filter type.</typeparam>
public class Service<TKey, TEntity, TModel, TFilter> : ServiceBase<TEntity, TModel, TFilter>, IService<TKey, TModel, TFilter>
  where TEntity : class, IEntity, new()
  where TModel : class, IModel<TEntity>
  where TFilter : class, IFilter
{
  /// <summary>
  /// Creates a service.
  /// </summary>
  /// <param name="dbContext">Database context.</param>
  /// <param name="queryPrefilters">Query prefilters that apply initial restrictions to queries.</param>
  /// <param name="validator">Model validator.</param>
  public Service(DbContext dbContext, IEnumerable<IQueryPrefilter<TEntity, TFilter>>? queryPrefilters = null, IValidator<TModel>? validator = null) : base(dbContext, queryPrefilters, validator) { }

  /// <inheritdoc/>
  public virtual Task<TModel?> GetByIdAsync(TKey id, IEnumerable<IInclusion<TModel>>? inclusions = null, CancellationToken cancellationToken = default)
  {
    return this.GetByKeyValuesAsync([id], inclusions, cancellationToken);
  }

  /// <inheritdoc/>
  public virtual Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
  {
    return this.DeleteByKeyValuesAsync([id], cancellationToken);
  }
}
