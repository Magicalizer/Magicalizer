// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
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
/// A base service for managing models supporting CRUD, filtering, sorting, pagination, and inclusion.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TFilter">The filter type.</typeparam>
public abstract class ServiceBase<TEntity, TModel, TFilter> : IService<TModel, TFilter>
  where TEntity : class, IEntity, new()
  where TModel : class, IModel
  where TFilter : class, IFilter
{
  private static readonly Func<TEntity, TModel> mapper = CreateMapper();
  protected readonly DbContext dbContext;
  protected readonly IEnumerable<IQueryPrefilter<TEntity, TFilter>>? queryPrefilters;
  protected readonly IValidator<TModel>? validator;

  /// <summary>
  /// Initializes a new instance of the <see cref="ServiceBase{TKey, TEntity, TModel, TFilter}"/> class.
  /// </summary>
  /// <param name="dbContext">The database context.</param>
  /// <param name="queryPrefilters">The optional collection of query prefilters to apply initial restrictions, security rules, or mandatory logic.</param>
  /// <param name="validator">The optional model validator.</param>
  public ServiceBase(DbContext dbContext, IEnumerable<IQueryPrefilter<TEntity, TFilter>>? queryPrefilters = null, IValidator<TModel>? validator = null)
  {
    this.dbContext = dbContext;
    this.queryPrefilters = queryPrefilters;
    this.validator = validator;
  }

  /// <summary>
  /// Retrieves all the models that match filter with sorting, pagination, and inclusions.
  /// </summary>
  /// <param name="filter">The filter to query models.</param>
  /// <param name="sortings">The sorting property paths with sorting direction.</param>
  /// <param name="offset">The number of models to skip.</param>
  /// <param name="limit">The maximum number of models to return.</param>
  /// <param name="inclusions">The inclusion property paths to include related models.</param>
  /// <returns>The collection of models that match the filter.</returns>
  public virtual async Task<IEnumerable<TModel>> GetAllAsync(TFilter? filter = null, IEnumerable<ISorting<TModel>>? sortings = null, int? offset = null, int? limit = null, params IInclusion<TModel>[] inclusions)
  {
    filter = this.GetPreprocessedFilter(filter);

    IQueryable<TEntity> query = this.GetPrefilteredQuery(filter)
      .ApplyFiltering(filter)
      .ApplySorting(sortings?.Select(s => new Data.Sorting<TEntity>(s.IsAscending, s.PropertyPath)))
      .ApplyPaging(offset, limit)
      .ApplyInclusions(inclusions.Select(i => new Data.Inclusion<TEntity>(i.PropertyPath)));

    return (await query.ToListAsync()).Select(e => EntityToModel(e)!);
  }

  /// <summary>
  /// Retrieves the total count of models that match the optional filter.
  /// </summary>
  /// <param name="filter">The filter to count models.</param>
  /// <returns>The count of models that match the filter.</returns>
  public virtual async Task<int> CountAsync(TFilter? filter = null)
  {
    filter = this.GetPreprocessedFilter(filter);
    return await this.GetPrefilteredQuery(filter).ApplyFiltering(filter).CountAsync();
  }

  /// <summary>
  /// Creates a new model and returns it.
  /// </summary>
  /// <param name="model">The model to create.</param>
  /// <returns>The created model.</returns>
  public virtual async Task<TModel> CreateAsync(TModel model)
  {
    this.validator?.ValidateAndThrow(model);

    TEntity entity = (model as IModel<TEntity>)!.ToEntity();

    this.dbContext.Add(entity);
    await this.dbContext.SaveChangesAsync();
    return this.EntityToModel(entity)!;
  }

  /// <summary>
  /// Updates an existing model.
  /// </summary>
  /// <param name="model">The model to update.</param>
  public virtual async Task EditAsync(TModel model)
  {
    this.validator?.ValidateAndThrow(model);

    TEntity entity = (model as IModel<TEntity>)!.ToEntity();
    Microsoft.EntityFrameworkCore.Metadata.IProperty? key = GetPrimaryKeyProperty(0);

    if (key == null) return;

    PropertyInfo? property = typeof(TEntity).GetProperty(key.Name);

    if (property == null) return;

    TEntity? local = this.dbContext.Set<TEntity>().Local.FirstOrDefault(e => property.GetValue(e)?.Equals(property.GetValue(entity)) == true);

    if (local != null)
      this.dbContext.Entry(local).State = EntityState.Detached;

    this.dbContext.Entry(entity).State = EntityState.Modified;
    await this.dbContext.SaveChangesAsync();
  }

  protected virtual TFilter? GetPreprocessedFilter(TFilter? filter = null)
  {
    return filter;
  }

  protected virtual IQueryable<TEntity> GetPrefilteredQuery(TFilter? filter)
  {
    IQueryable<TEntity> entities = this.dbContext.Set<TEntity>().AsNoTracking();

    if (this.queryPrefilters != null)
      foreach (IQueryPrefilter<TEntity, TFilter> queryPrefilter in this.queryPrefilters)
        entities = queryPrefilter.Apply(entities, filter);

    return entities;
  }

  protected virtual TModel? EntityToModel(TEntity? entity)
  {
    return entity == null ? null : mapper(entity);
  }

  protected virtual Microsoft.EntityFrameworkCore.Metadata.IProperty? GetPrimaryKeyProperty(int index)
  {
    return this.dbContext.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties[index];
  }

  private static Func<TEntity, TModel> CreateMapper()
  {
    ConstructorInfo? constructor = typeof(TModel).GetConstructor([ typeof(TEntity) ]);

    if (constructor == null)
      throw new InvalidOperationException($"Type {typeof(TModel).Name} must have a constructor that accepts {typeof(TEntity).Name}");

    ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "entity");
    NewExpression @new = Expression.New(constructor, parameter);

    return Expression.Lambda<Func<TEntity, TModel>>(@new, parameter).Compile();
  }
}