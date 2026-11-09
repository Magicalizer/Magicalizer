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
/// Service of models with a composite primary key of three properties.
/// </summary>
/// <typeparam name="TKey1">Type of the first primary key property.</typeparam>
/// <typeparam name="TKey2">Type of the second primary key property.</typeparam>
/// <typeparam name="TKey3">Type of the third primary key property.</typeparam>
/// <typeparam name="TEntity">Entity type.</typeparam>
/// <typeparam name="TModel">Model type.</typeparam>
/// <typeparam name="TFilter">Filter type.</typeparam>
public class Service<TKey1, TKey2, TKey3, TEntity, TModel, TFilter> : ServiceBase<TEntity, TModel, TFilter>, IService<TKey1, TKey2, TKey3, TModel, TFilter>
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
  public virtual Task<TModel?> GetByIdAsync(TKey1 id1, TKey2 id2, TKey3 id3, IEnumerable<IInclusion<TModel>>? inclusions = null, CancellationToken cancellationToken = default)
  {
    return this.GetByKeyValuesAsync([id1, id2, id3], inclusions, cancellationToken);
  }

  /// <inheritdoc/>
  public virtual Task DeleteAsync(TKey1 id1, TKey2 id2, TKey3 id3, CancellationToken cancellationToken = default)
  {
    return this.DeleteByKeyValuesAsync([id1, id2, id3], cancellationToken);
  }
}
