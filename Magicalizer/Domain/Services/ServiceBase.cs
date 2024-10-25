// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
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
  protected readonly DbContext dbContext;
  protected readonly IValidator<TModel>? validator;

  /// <summary>
  /// Initializes a new instance of the <see cref="ServiceBase{TKey, TEntity, TModel, TFilter}"/> class.
  /// </summary>
  /// <param name="dbContext">The database context.</param>
  /// <param name="validator">The optional model validator.</param>
  public ServiceBase(DbContext dbContext, IValidator<TModel>? validator)
  {
    this.dbContext = dbContext;
    this.validator = validator;
  }

  /// <summary>
  /// Retrieves all the models with sorting, pagination, and inclusions.
  /// </summary>
  /// <param name="sorting">The sorting property path with a "+" for ascending or "-" for descending (e.g., "+category.name", "-created").</param>
  /// <param name="offset">The number of models to skip.</param>
  /// <param name="limit">The maximum number of models to return.</param>
  /// <param name="inclusions">The inclusion property paths to include related models.</param>
  /// <returns>The collection of all the models.</returns>
  public virtual async Task<IEnumerable<TModel>> GetAllAsync(string? sorting = null, int? offset = null, int? limit = null, params string[] inclusions)
  {
    return await this.GetAllAsync(sorting == null ? [] : [sorting!], offset, limit, inclusions);
  }

  /// <summary>
  /// Retrieves all the models with sorting, pagination, and inclusions.
  /// </summary>
  /// <param name="sortings">The sorting property paths with a "+" for ascending or "-" for descending (e.g., "+category.name", "-created").</param>
  /// <param name="offset">The number of models to skip.</param>
  /// <param name="limit">The maximum number of models to return.</param>
  /// <param name="inclusions">The inclusion property paths to include related models.</param>
  /// <returns>The collection of all the models.</returns>
  public virtual async Task<IEnumerable<TModel>> GetAllAsync(IEnumerable<string> sortings, int? offset = null, int? limit = null, params string[] inclusions)
  {
    return (await dbContext.Set<TEntity>()
      .AsNoTracking()
      .ApplySorting(inclusions)
      .ApplyPaging(offset, limit)
      .ApplyInclusions(inclusions)
      .ToListAsync()).Select(e => EntityToModel(e)!);
  }

  /// <summary>
  /// Retrieves all the models with sorting, pagination, and inclusions.
  /// </summary>
  /// <param name="sorting">The sorting property paths with sorting direction.</param>
  /// <param name="offset">The number of models to skip.</param>
  /// <param name="limit">The maximum number of models to return.</param>
  /// <param name="inclusions">The inclusion property paths to include related models.</param>
  /// <returns>The collection of all the models.</returns>
  public virtual async Task<IEnumerable<TModel>> GetAllAsync(ISorting<TModel> sorting, int? offset = null, int? limit = null, params IInclusion<TModel>[] inclusions)
  {
    return await this.GetAllAsync([sorting], offset, limit, inclusions);
  }

  /// <summary>
  /// Retrieves all the models with sorting, pagination, and inclusions.
  /// </summary>
  /// <param name="sortings">The sorting property paths with sorting direction.</param>
  /// <param name="offset">The number of models to skip.</param>
  /// <param name="limit">The maximum number of models to return.</param>
  /// <param name="inclusions">The inclusion property paths to include related models.</param>
  /// <returns>The collection of all the models.</returns>
  public virtual async Task<IEnumerable<TModel>> GetAllAsync(IEnumerable<ISorting<TModel>> sortings, int? offset = null, int? limit = null, params IInclusion<TModel>[] inclusions)
  {
    return (await dbContext.Set<TEntity>()
      .AsNoTracking()
      .ApplySorting(sortings.Select(s => new Data.Sorting<TEntity>(s.IsAscending, s.PropertyPath)))
      .ApplyPaging(offset, limit)
      .ApplyInclusions(inclusions.Select(i => new Data.Inclusion<TEntity>(i.PropertyPath)))
      .ToListAsync()).Select(e => EntityToModel(e)!);
  }

  /// <summary>
  /// Retrieves all the models that match filter with sorting, pagination, and inclusions.
  /// </summary>
  /// <param name="filter">The filter to query models.</param>
  /// <param name="sorting">The sorting property path with a "+" for ascending or "-" for descending (e.g., "+category.name", "-created").</param>
  /// <param name="offset">The number of models to skip.</param>
  /// <param name="limit">The maximum number of models to return.</param>
  /// <param name="inclusions">The inclusion property paths to include related models.</param>
  /// <returns>The collection of models that match the filter.</returns>
  public virtual async Task<IEnumerable<TModel>> GetFilteredAsync(TFilter filter, string? sorting = null, int? offset = null, int? limit = null, params string[] inclusions)
  {
    return await this.GetFilteredAsync(filter, sorting == null ? [] : [sorting!], offset, limit, inclusions);
  }

  /// <summary>
  /// Retrieves all the models that match filter with sorting, pagination, and inclusions.
  /// </summary>
  /// <param name="filter">The filter to query models.</param>
  /// <param name="sortings">The sorting property paths with a "+" for ascending or "-" for descending (e.g., "+category.name", "-created").</param>
  /// <param name="offset">The number of models to skip.</param>
  /// <param name="limit">The maximum number of models to return.</param>
  /// <param name="inclusions">The inclusion property paths to include related models.</param>
  /// <returns>The collection of models that match the filter.</returns>
  public virtual async Task<IEnumerable<TModel>> GetFilteredAsync(TFilter filter, IEnumerable<string> sortings, int? offset = null, int? limit = null, params string[] inclusions)
  {
    return (await dbContext.Set<TEntity>()
      .AsNoTracking()
      .ApplyFiltering(filter)
      .ApplySorting(sortings)
      .ApplyPaging(offset, limit)
      .ApplyInclusions(inclusions)
      .ToListAsync()).Select(e => EntityToModel(e)!);
  }

  /// <summary>
  /// Retrieves all the models that match filter with sorting, pagination, and inclusions.
  /// </summary>
  /// <param name="filter">The filter to query models.</param>
  /// <param name="sorting">The sorting property paths with sorting direction.</param>
  /// <param name="offset">The number of models to skip.</param>
  /// <param name="limit">The maximum number of models to return.</param>
  /// <param name="inclusions">The inclusion property paths to include related models.</param>
  /// <returns>The collection of models that match the filter.</returns>
  public virtual async Task<IEnumerable<TModel>> GetFilteredAsync(TFilter filter, ISorting<TModel> sorting, int? offset = null, int? limit = null, params IInclusion<TModel>[] inclusions)
  {
    return await this.GetFilteredAsync(filter, [sorting], offset, limit, inclusions);
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
  public virtual async Task<IEnumerable<TModel>> GetFilteredAsync(TFilter filter, IEnumerable<ISorting<TModel>> sortings, int? offset = null, int? limit = null, params IInclusion<TModel>[] inclusions)
  {
    return (await dbContext.Set<TEntity>()
      .AsNoTracking()
      .ApplyFiltering(filter)
      .ApplySorting(sortings.Select(s => new Data.Sorting<TEntity>(s.IsAscending, s.PropertyPath)))
      .ApplyPaging(offset, limit)
      .ApplyInclusions(inclusions.Select(i => new Data.Inclusion<TEntity>(i.PropertyPath)))
      .ToListAsync()).Select(e => EntityToModel(e)!);
  }

  /// <summary>
  /// Retrieves the total count of models that match the optional filter.
  /// </summary>
  /// <param name="filter">The filter to count models.</param>
  /// <returns>The count of models that match the filter.</returns>
  public virtual async Task<int> CountAsync(TFilter? filter = null)
  {
    return await dbContext.Set<TEntity>()
      .AsNoTracking()
      .ApplyFiltering(filter)
      .CountAsync();
  }

  /// <summary>
  /// Creates a new model and returns it.
  /// </summary>
  /// <param name="model">The model to create.</param>
  /// <returns>The created model.</returns>
  public virtual async Task<TModel> CreateAsync(TModel model)
  {
    this.validator?.ValidateAndThrow(model);

    TEntity entity = (model as IModel<TEntity>).ToEntity();

    dbContext.Add(entity);
    await dbContext.SaveChangesAsync();
    return this.EntityToModel(entity);
  }

  /// <summary>
  /// Updates an existing model.
  /// </summary>
  /// <param name="model">The model to update.</param>
  public virtual async Task EditAsync(TModel model)
  {
    this.validator?.ValidateAndThrow(model);

    TEntity entity = (model as IModel<TEntity>).ToEntity();
    Microsoft.EntityFrameworkCore.Metadata.IProperty? key = GetPrimaryKeyProperty(0);

    if (key == null) return;

    PropertyInfo? property = typeof(TEntity).GetProperty(key.Name);

    if (property == null) return;

    TEntity? local = dbContext.Set<TEntity>().Local.FirstOrDefault(e => property.GetValue(e)?.Equals(property.GetValue(entity)) == true);

    if (local != null)
      dbContext.Entry(local).State = EntityState.Detached;

    dbContext.Entry(entity).State = EntityState.Modified;
    await dbContext.SaveChangesAsync();
  }

  protected virtual TModel? EntityToModel(TEntity? entity)
  {
    return entity == null ? null : Activator.CreateInstance(typeof(TModel), entity) as TModel;
  }

  protected virtual Microsoft.EntityFrameworkCore.Metadata.IProperty? GetPrimaryKeyProperty(int index)
  {
    return this.dbContext.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties[index];
  }
}