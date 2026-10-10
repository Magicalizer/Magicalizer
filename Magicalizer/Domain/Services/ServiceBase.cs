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
  /// Initializes a new instance of the <see cref="ServiceBase{TEntity, TModel, TFilter}"/> class.
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
  /// <param name="sortings">The sorting rules (property paths with sorting directions).</param>
  /// <param name="offset">The number of models to skip.</param>
  /// <param name="limit">The maximum number of models to return.</param>
  /// <param name="inclusions">The inclusion rules (property paths of the related models to include).</param>
  /// <param name="cancellationToken">The token to cancel the operation.</param>
  /// <returns>The collection of models that match the filter.</returns>
  public virtual async Task<IEnumerable<TModel>> GetAllAsync(TFilter? filter = null, IEnumerable<ISorting<TModel>>? sortings = null, int? offset = null, int? limit = null, IEnumerable<IInclusion<TModel>>? inclusions = null, CancellationToken cancellationToken = default)
  {
    filter = this.GetPreprocessedFilter(filter);

    IQueryable<TEntity> query = this.GetPrefilteredQuery(filter)
      .ApplyFiltering(filter)
      .ApplySorting(sortings?.Select(s => new Data.Sorting<TEntity>(s.IsAscending, s.PropertyPath)))
      .ApplyPaging(offset, limit)
      .ApplyInclusions(inclusions?.Select(i => new Data.Inclusion<TEntity>(i.PropertyPath)));

    return (await query.ToListAsync(cancellationToken)).Select(e => EntityToModel(e)!);
  }

  /// <summary>
  /// Retrieves the total count of models that match the optional filter.
  /// </summary>
  /// <param name="filter">The filter to count models.</param>
  /// <param name="cancellationToken">The token to cancel the operation.</param>
  /// <returns>The count of models that match the filter.</returns>
  public virtual async Task<int> CountAsync(TFilter? filter = null, CancellationToken cancellationToken = default)
  {
    filter = this.GetPreprocessedFilter(filter);
    return await this.GetPrefilteredQuery(filter).ApplyFiltering(filter).CountAsync(cancellationToken);
  }

  /// <summary>
  /// Creates a new model and returns it.
  /// </summary>
  /// <param name="model">The model to create.</param>
  /// <param name="cancellationToken">The token to cancel the operation.</param>
  /// <returns>The created model.</returns>
  public virtual async Task<TModel> CreateAsync(TModel model, CancellationToken cancellationToken = default)
  {
    if (this.validator != null)
      await this.validator.ValidateAndThrowAsync(model, cancellationToken);

    TEntity entity = (model as IModel<TEntity>)!.ToEntity();

    this.dbContext.Add(entity);
    await this.dbContext.SaveChangesAsync(cancellationToken);
    return this.EntityToModel(entity)!;
  }

  /// <summary>
  /// Updates an existing model.
  /// </summary>
  /// <param name="model">The model to update.</param>
  /// <param name="cancellationToken">The token to cancel the operation.</param>
  public virtual async Task EditAsync(TModel model, CancellationToken cancellationToken = default)
  {
    if (this.validator != null)
      await this.validator.ValidateAndThrowAsync(model, cancellationToken);

    TEntity entity = (model as IModel<TEntity>)!.ToEntity();

    // The entity with the same primary key may already be tracked (e.g. loaded during the validation), so it is detached first.
    Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity>? local = this.dbContext.Set<TEntity>().Local.FindEntryUntyped(this.GetEntityKeyValues(entity));

    if (local != null)
      local.State = EntityState.Detached;

    this.dbContext.Entry(entity).State = EntityState.Modified;
    await this.dbContext.SaveChangesAsync(cancellationToken);
  }

  /// <summary>
  /// Gets the primary key values of a model (e.g. to build the URL of a created model).
  /// </summary>
  /// <param name="model">The model to get the primary key values of.</param>
  /// <returns>The primary key values in the order of the primary key properties.</returns>
  public virtual object?[] GetKeyValues(TModel model)
  {
    return this.GetEntityKeyValues((model as IModel<TEntity>)!.ToEntity());
  }

  /// <summary>
  /// Preprocesses the filter before it is applied to a query (e.g. to add mandatory conditions).
  /// </summary>
  /// <param name="filter">The filter to preprocess.</param>
  /// <returns>The preprocessed filter.</returns>
  protected virtual TFilter? GetPreprocessedFilter(TFilter? filter = null)
  {
    return filter;
  }

  /// <summary>
  /// Gets the non-tracking query of the entities with all the query prefilters applied.
  /// </summary>
  /// <param name="filter">The filter that is passed to the query prefilters.</param>
  /// <returns>The prefiltered query.</returns>
  protected virtual IQueryable<TEntity> GetPrefilteredQuery(TFilter? filter)
  {
    IQueryable<TEntity> entities = this.dbContext.Set<TEntity>().AsNoTracking();

    if (this.queryPrefilters != null)
      foreach (IQueryPrefilter<TEntity, TFilter> queryPrefilter in this.queryPrefilters)
        entities = queryPrefilter.Apply(entities, filter);

    return entities;
  }

  /// <summary>
  /// Converts the given entity to its corresponding model.
  /// </summary>
  /// <param name="entity">The entity to convert.</param>
  /// <returns>The converted model, or <c>null</c> if the entity is <c>null</c>.</returns>
  protected virtual TModel? EntityToModel(TEntity? entity)
  {
    return entity == null ? null : mapper(entity);
  }

  /// <summary>
  /// Gets a property of the entity's primary key by its index (composite primary keys consist of several properties).
  /// </summary>
  /// <param name="index">The index of the property in the primary key.</param>
  /// <returns>The primary key property, or <c>null</c> if the entity type or its primary key is not found.</returns>
  protected virtual Microsoft.EntityFrameworkCore.Metadata.IProperty? GetPrimaryKeyProperty(int index)
  {
    return this.dbContext.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties[index];
  }

  // Gets the primary key values of an entity in the order of the primary key properties.
  private object?[] GetEntityKeyValues(TEntity entity)
  {
    IReadOnlyList<Microsoft.EntityFrameworkCore.Metadata.IProperty>? keyProperties = this.dbContext.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties;

    return keyProperties?.Select(p => p.PropertyInfo?.GetValue(entity)).ToArray() ?? [];
  }

  // Creates a compiled function that calls the model constructor that accepts the entity.
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