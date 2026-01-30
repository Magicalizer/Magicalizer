// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using FluentValidation;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Data.Extensions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Filters.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Magicalizer.Domain.Services;

/// <summary>
/// A service for managing models with a single-property primary key,
/// supporting CRUD, filtering, sorting, pagination, and inclusion.
/// </summary>
/// <typeparam name="TKey">The primary key type.</typeparam>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TFilter">The filter type.</typeparam>
public class Service<TKey, TEntity, TModel, TFilter> : ServiceBase<TEntity, TModel, TFilter>, IService<TKey, TModel, TFilter>
where TEntity : class, IEntity, new()
where TModel : class, IModel
where TFilter : class, IFilter
{
  /// <summary>
  /// Initializes a new instance of the <see cref="Service{TKey, TEntity, TModel, TFilter}"/> class.
  /// </summary>
  /// <param name="dbContext">The database context.</param>
  /// <param name="queryPrefilters">The optional collection of query prefilters to apply initial restrictions, security rules, or mandatory logic.</param>
  /// <param name="validator">The optional model validator.</param>
  public Service(DbContext dbContext, IEnumerable<IQueryPrefilter<TEntity, TFilter>>? queryPrefilters = null, IValidator<TModel>? validator = null) : base(dbContext, queryPrefilters, validator) { }

  /// <summary>
  /// Retrieves a model by its primary key.
  /// </summary>
  /// <param name="id">The value of the primary key.</param>
  /// <param name="inclusions">The inclusion property paths to include related models.</param>
  /// <returns>The model with the specified primary key, or <c>null</c> if no such model exists.</returns>
  public virtual async Task<TModel?> GetByIdAsync(TKey id, params IInclusion<TModel>[] inclusions)
  {
    Microsoft.EntityFrameworkCore.Metadata.IProperty? key = this.GetPrimaryKeyProperty(0);

    if (key == null) return null;

    TFilter? filter = this.GetPreprocessedFilter();
    TEntity? entity = await this.GetPrefilteredQuery(filter)
      .ApplyFiltering(filter)
      .ApplyInclusions(inclusions.Select(i => new Data.Inclusion<TEntity>(i.PropertyPath)))
      .FirstOrDefaultAsync(e => EF.Property<TKey>(e, key.Name)!.Equals(id));

    return this.EntityToModel(entity);
  }

  /// <summary>
  /// Deletes a model by its primary key.
  /// </summary>
  /// <param name="id">The value of the primary key property.</param>
  /// <returns>A boolean indicating success.</returns>
  public virtual async Task<bool> DeleteAsync(TKey id)
  {
    try
    {
      TEntity entity = new TEntity();
      Microsoft.EntityFrameworkCore.Metadata.IProperty? key = GetPrimaryKeyProperty(0);

      if (key == null) return false;

      PropertyInfo? property = typeof(TEntity).GetProperty(key.Name);

      if (property == null) return false;

      property.SetValue(entity, id);
      this.dbContext.Set<TEntity>().Remove(entity);
      await this.dbContext.SaveChangesAsync();
      return true;
    }

    catch { return false; }
  }
}