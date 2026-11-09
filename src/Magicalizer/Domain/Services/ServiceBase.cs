// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Linq.Expressions;
using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using Magicalizer.Data;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Data.Extensions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Extensions;
using Magicalizer.Filters.Abstractions;
using Magicalizer.Mapping;
using Magicalizer.Validators.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using IForeignKey = Microsoft.EntityFrameworkCore.Metadata.IForeignKey;
using IProperty = Microsoft.EntityFrameworkCore.Metadata.IProperty;
using ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated;

namespace Magicalizer.Domain.Services;

/// <summary>
/// Base of model services: gets models with filtering, sorting, paging and inclusions, creates, edits and deletes
/// them.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
/// <typeparam name="TModel">Model type.</typeparam>
/// <typeparam name="TFilter">Filter type.</typeparam>
public abstract class ServiceBase<TEntity, TModel, TFilter> : IService<TModel, TFilter>
  where TEntity : class, IEntity, new()
  where TModel : class, IModel<TEntity>
  where TFilter : class, IFilter
{
  // Method that checks whether a referenced entity exists, made generic for the type of the entity when it's called.
  private static readonly MethodInfo ReferenceExistsMethod = typeof(ServiceBase<TEntity, TModel, TFilter>).GetMethod(nameof(ReferenceExistsAsync), BindingFlags.NonPublic | BindingFlags.Instance)!;

  /// <summary>
  /// Database context.
  /// </summary>
  protected readonly DbContext dbContext;

  /// <summary>
  /// Query prefilters that apply initial restrictions to queries.
  /// </summary>
  protected readonly IEnumerable<IQueryPrefilter<TEntity, TFilter>>? queryPrefilters;

  /// <summary>
  /// Model validator.
  /// </summary>
  protected readonly IValidator<TModel>? validator;

  /// <summary>
  /// Creates a service.
  /// </summary>
  /// <param name="dbContext">Database context.</param>
  /// <param name="queryPrefilters">Query prefilters that apply initial restrictions to queries.</param>
  /// <param name="validator">Model validator.</param>
  protected ServiceBase(DbContext dbContext, IEnumerable<IQueryPrefilter<TEntity, TFilter>>? queryPrefilters = null, IValidator<TModel>? validator = null)
  {
    this.dbContext = dbContext;
    this.queryPrefilters = queryPrefilters;
    this.validator = validator;
  }

  /// <inheritdoc/>
  public virtual async Task<IEnumerable<TModel>> GetAllAsync(TFilter? filter = null, IEnumerable<ISorting<TModel>>? sortings = null, int? offset = null, int? limit = null, IEnumerable<IInclusion<TModel>>? inclusions = null, CancellationToken cancellationToken = default)
  {
    filter = this.GetPreprocessedFilter(filter);

    IQueryable<TEntity> query = this.GetPrefilteredQuery(filter)
      .ApplyFiltering(filter)
      .ApplySorting(sortings?.Select(s => new Data.Sorting<TEntity>(s.IsAscending, s.PropertyPath)))
      .ApplyPaging(offset, limit)
      .ApplyInclusions(inclusions?.Select(i => new Data.Inclusion<TEntity>(i.PropertyPath)));

    // The models are converted once, so the result can be enumerated several times.
    return (await query.ToListAsync(cancellationToken)).Select(e => this.EntityToModel(e)!).ToList();
  }

  /// <inheritdoc/>
  public virtual async Task<int> CountAsync(TFilter? filter = null, CancellationToken cancellationToken = default)
  {
    filter = this.GetPreprocessedFilter(filter);
    return await this.GetPrefilteredQuery(filter).ApplyFiltering(filter).CountAsync(cancellationToken);
  }

  /// <inheritdoc/>
  /// <remarks>
  /// The model is validated by the validator first (if it's registered) with the <see cref="RuleSetName.Default"/> and
  /// <see cref="RuleSetName.Create"/> rule sets. If the database generates the primary key, the primary key values of
  /// the entity are reset, so the database generates new ones. Otherwise, if the database rejects the entity, it's
  /// checked whether an entity with the same primary key exists. In either case, if the database rejects the entity,
  /// it's also checked whether the models it refers to exist.
  /// </remarks>
  /// <exception cref="ValidationException">The model isn't valid or refers to a model that doesn't exist.</exception>
  public virtual async Task<TModel> CreateAsync(TModel model, CancellationToken cancellationToken = default)
  {
    if (this.validator != null)
      await this.validator.ValidateAsync(model, o => o.IncludeRuleSets(RuleSetName.Default, RuleSetName.Create).ThrowOnFailures(), cancellationToken);

    TEntity entity = model.ToEntity();

    // A client can't choose the keys the database generates: some databases reject them, and PostgreSQL would accept
    // them without moving its sequence, so the next generated key would clash with them.
    foreach (IProperty keyProperty in this.GetPrimaryKeyProperties().Where(p => p.ValueGenerated != ValueGenerated.Never))
      keyProperty.PropertyInfo?.SetValue(entity, keyProperty.ClrType.IsValueType ? Activator.CreateInstance(keyProperty.ClrType) : null);

    this.dbContext.Add(entity);

    try
    {
      await this.dbContext.SaveChangesAsync(cancellationToken);
    }

    // The databases report the errors differently, so the reasons are checked only after the database rejects the
    // entity (that costs nothing in the usual case).
    catch (DbUpdateException exception)
    {
      this.dbContext.Entry(entity).State = EntityState.Detached;
      await this.ThrowIfModelAlreadyExistsAsync(entity, exception, cancellationToken);
      await this.ThrowIfReferencedModelsDontExistAsync(entity, cancellationToken);
      throw;
    }

    return this.EntityToModel(entity)!;
  }

  /// <inheritdoc/>
  /// <remarks>
  /// The entity of the model must be available through the query prefilters (as it is for getting it). Then the model
  /// is validated by the validator (if it's registered) with the <see cref="RuleSetName.Default"/> and
  /// <see cref="RuleSetName.Edit"/> rule sets. If the database rejects the entity, it's checked whether the models it
  /// refers to exist.
  /// </remarks>
  /// <exception cref="ValidationException">The model isn't valid or refers to a model that doesn't exist.</exception>
  public virtual async Task EditAsync(TModel model, CancellationToken cancellationToken = default)
  {
    TEntity entity = model.ToEntity();
    object?[] keyValues = this.GetEntityKeyValues(entity);
    TFilter? filter = this.GetPreprocessedFilter();

    // The entity must be available through the query prefilters and the filter, as it is for getting it.
    if (!await this.ExistsAsync(this.GetPrefilteredQuery(filter).ApplyFiltering(filter), keyValues, cancellationToken))
      throw new ModelNotFoundException(typeof(TModel), keyValues);

    if (this.validator != null)
      await this.validator.ValidateAsync(model, o => o.IncludeRuleSets(RuleSetName.Default, RuleSetName.Edit).ThrowOnFailures(), cancellationToken);

    this.Attach(entity, keyValues, EntityState.Modified);

    try
    {
      await this.dbContext.SaveChangesAsync(cancellationToken);
    }

    // The entity may be deleted after it's checked.
    catch (DbUpdateConcurrencyException exception)
    {
      this.dbContext.Entry(entity).State = EntityState.Detached;
      throw new ModelNotFoundException(typeof(TModel), keyValues, exception);
    }

    // The other errors are passed on (unless the entity refers to models that don't exist), but the entity is detached
    // anyway, so the context can still be used.
    catch (DbUpdateException)
    {
      this.dbContext.Entry(entity).State = EntityState.Detached;
      await this.ThrowIfReferencedModelsDontExistAsync(entity, cancellationToken);
      throw;
    }
  }

  /// <inheritdoc/>
  public virtual object?[] GetKeyValues(TModel model)
  {
    return this.GetEntityKeyValues(model.ToEntity());
  }

  /// <summary>
  /// Preprocesses the filter before it's applied to a query (e.g. to add mandatory conditions).
  /// </summary>
  /// <param name="filter">Filter to preprocess.</param>
  /// <returns>Preprocessed filter.</returns>
  protected virtual TFilter? GetPreprocessedFilter(TFilter? filter = null)
  {
    return filter;
  }

  /// <summary>
  /// Gets the non-tracking entity query with all query prefilters applied.
  /// </summary>
  /// <param name="filter">Filter that is passed to the query prefilters.</param>
  /// <returns>Prefiltered query.</returns>
  protected virtual IQueryable<TEntity> GetPrefilteredQuery(TFilter? filter)
  {
    IQueryable<TEntity> entities = this.dbContext.Set<TEntity>().AsNoTracking();

    if (this.queryPrefilters != null)
    {
      foreach (IQueryPrefilter<TEntity, TFilter> queryPrefilter in this.queryPrefilters)
        entities = queryPrefilter.Apply(entities, filter);
    }

    return entities;
  }

  /// <summary>
  /// Converts the entity to the model.
  /// </summary>
  /// <param name="entity">Entity to convert.</param>
  /// <returns>Model, or <c>null</c> if the entity is <c>null</c>.</returns>
  protected virtual TModel? EntityToModel(TEntity? entity)
  {
    return entity == null ? null : ConstructorMapper<TEntity, TModel>.Map(entity);
  }

  /// <summary>
  /// Gets the model by the primary key values. The entity must be available through the query prefilters.
  /// </summary>
  /// <param name="keyValues">Primary key values in the order of primary key properties.</param>
  /// <param name="inclusions">Inclusions to apply (property paths of related models).</param>
  /// <param name="cancellationToken">Token to cancel the operation.</param>
  /// <returns>Model, or <c>null</c> if it doesn't exist.</returns>
  protected async Task<TModel?> GetByKeyValuesAsync(object?[] keyValues, IEnumerable<IInclusion<TModel>>? inclusions = null, CancellationToken cancellationToken = default)
  {
    Expression<Func<TEntity, bool>>? keyPredicate = BuildKeyPredicate<TEntity>(this.GetPrimaryKeyProperties(), keyValues);

    if (keyPredicate == null) return null;

    TFilter? filter = this.GetPreprocessedFilter();
    TEntity? entity = await this.GetPrefilteredQuery(filter)
      .ApplyFiltering(filter)
      .ApplyInclusions(inclusions?.Select(i => new Data.Inclusion<TEntity>(i.PropertyPath)))
      .FirstOrDefaultAsync(keyPredicate, cancellationToken);

    return this.EntityToModel(entity);
  }

  /// <summary>
  /// Deletes the entity by the primary key values. The entity must be available through the query prefilters (as it is
  /// for getting it).
  /// </summary>
  /// <param name="keyValues">Primary key values in the order of primary key properties.</param>
  /// <param name="cancellationToken">Token to cancel the operation.</param>
  /// <exception cref="ModelNotFoundException">
  /// The entity doesn't exist (or isn't available through the query prefilters).
  /// </exception>
  /// <exception cref="ModelInUseException">
  /// The database restricts the deletion of the entity (e.g. other entities reference it).
  /// </exception>
  protected async Task DeleteByKeyValuesAsync(object?[] keyValues, CancellationToken cancellationToken = default)
  {
    TFilter? filter = this.GetPreprocessedFilter();

    // The entity must be available through the query prefilters and the filter, as it is for getting it.
    if (!await this.ExistsAsync(this.GetPrefilteredQuery(filter).ApplyFiltering(filter), keyValues, cancellationToken))
      throw new ModelNotFoundException(typeof(TModel), keyValues);

    // The entity is deleted without loading it, with a stub entity that has only the primary key set.
    TEntity entity = new TEntity();
    IReadOnlyList<IProperty> keyProperties = this.GetPrimaryKeyProperties();

    for (int i = 0; i < keyProperties.Count; i++)
      keyProperties[i].PropertyInfo?.SetValue(entity, keyValues[i]);

    this.Attach(entity, keyValues, EntityState.Deleted);

    try
    {
      await this.dbContext.SaveChangesAsync(cancellationToken);
    }

    // The entity may be deleted after it's checked.
    catch (DbUpdateConcurrencyException exception)
    {
      this.dbContext.Entry(entity).State = EntityState.Detached;
      throw new ModelNotFoundException(typeof(TModel), keyValues, exception);
    }

    // The databases report the errors differently, so the existence is checked only after the database rejects the
    // deletion: if the entity still exists, the database restricts its deletion (e.g. by a foreign key).
    catch (DbUpdateException exception)
    {
      this.dbContext.Entry(entity).State = EntityState.Detached;

      if (await this.ExistsAsync(this.dbContext.Set<TEntity>().AsNoTracking(), keyValues, cancellationToken))
        throw new ModelInUseException(typeof(TModel), keyValues, exception);

      throw;
    }
  }

  // Gets the primary key properties of the entity (none if the entity type or its primary key isn't found).
  private IReadOnlyList<IProperty> GetPrimaryKeyProperties()
  {
    return this.dbContext.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties ?? [];
  }

  // Throws the exception if the model with the same primary key already exists (so the database rejects the entity). It
  // can happen only if the database doesn't generate the primary key, and the query isn't prefiltered, as a hidden
  // model has its primary key too.
  private async Task ThrowIfModelAlreadyExistsAsync(TEntity entity, DbUpdateException exception, CancellationToken cancellationToken)
  {
    if (this.GetPrimaryKeyProperties().Any(p => p.ValueGenerated != ValueGenerated.Never)) return;

    object?[] keyValues = this.GetEntityKeyValues(entity);

    if (await this.ExistsAsync(this.dbContext.Set<TEntity>().AsNoTracking(), keyValues, cancellationToken))
      throw new ModelAlreadyExistsException(typeof(TModel), keyValues, exception);
  }

  // Throws the validation exception if the entity refers to models that don't exist (so the database rejects it by the
  // foreign keys). The errors are of the navigation properties (e.g. "Category" of a product), as the models have them
  // instead of the foreign keys, and their names are resolved as FluentValidation resolves the ones of the validators.
  private async Task ThrowIfReferencedModelsDontExistAsync(TEntity entity, CancellationToken cancellationToken)
  {
    List<ValidationFailure> failures = [];
    IEnumerable<IForeignKey> foreignKeys = this.dbContext.Model.FindEntityType(typeof(TEntity))?.GetForeignKeys() ?? [];

    foreach (IForeignKey foreignKey in foreignKeys.Where(fk => fk.PrincipalKey.IsPrimaryKey()))
    {
      object?[] keyValues = foreignKey.Properties.Select(p => p.PropertyInfo?.GetValue(entity)).ToArray();

      // An optional reference that isn't set doesn't refer to anything.
      if (keyValues.Any(v => v == null)) continue;

      Task<bool> referenceExists = (Task<bool>)ReferenceExistsMethod.MakeGenericMethod(foreignKey.PrincipalEntityType.ClrType)
        .Invoke(this, [foreignKey.PrincipalKey.Properties, keyValues, cancellationToken])!;

      if (!await referenceExists)
      {
        string name = foreignKey.DependentToPrincipal?.Name ?? foreignKey.Properties[0].Name;
        PropertyInfo? modelProperty = typeof(TModel).GetPublicProperty(name);
        string propertyName = (modelProperty == null ? null : ValidatorOptions.Global.PropertyNameResolver(typeof(TModel), modelProperty, null)) ?? name;

        failures.Add(new ValidationFailure(propertyName, $"The referenced model with the primary key ({string.Join(", ", keyValues)}) doesn't exist."));
      }
    }

    if (failures.Count != 0)
      throw new ValidationException(failures);
  }

  // Gets the primary key values of the entity in the order of the primary key properties.
  private object?[] GetEntityKeyValues(TEntity entity)
  {
    return this.GetPrimaryKeyProperties().Select(p => p.PropertyInfo?.GetValue(entity)).ToArray();
  }

  // Checks whether the query has an entity with the primary key values.
  private async Task<bool> ExistsAsync(IQueryable<TEntity> query, object?[] keyValues, CancellationToken cancellationToken)
  {
    Expression<Func<TEntity, bool>>? keyPredicate = BuildKeyPredicate<TEntity>(this.GetPrimaryKeyProperties(), keyValues);

    return keyPredicate != null && await query.AnyAsync(keyPredicate, cancellationToken);
  }

  // Attaches the entity in the state. The entity with the same primary key may already be tracked (e.g. loaded during
  // validation), so it's detached first.
  private void Attach(TEntity entity, object?[] keyValues, EntityState state)
  {
    EntityEntry<TEntity>? local = this.dbContext.Set<TEntity>().Local.FindEntryUntyped(keyValues);

    if (local != null)
      local.State = EntityState.Detached;

    this.dbContext.Entry(entity).State = state;
  }

  // Builds the predicate that matches the entity with the key values (e.g. "e => e.Id == 1"), or returns null if the
  // key has no properties. The values are query parameters, so the query is translated once for all values.
  private static Expression<Func<T, bool>>? BuildKeyPredicate<T>(IReadOnlyList<IProperty> keyProperties, object?[] keyValues)
  {
    ParameterExpression parameter = Expression.Parameter(typeof(T), "e");
    Expression? body = null;

    for (int i = 0; i < keyProperties.Count; i++)
    {
      Expression equal = Expression.Equal(Expression.Property(parameter, keyProperties[i].PropertyInfo!), QueryParameter.Create(keyValues[i], keyProperties[i].ClrType));

      body = body == null ? equal : Expression.AndAlso(body, equal);
    }

    return body == null ? null : Expression.Lambda<Func<T, bool>>(body, parameter);
  }

  // Checks whether the referenced entity with the primary key values exists. The query isn't prefiltered, as the
  // database checks the reference without the prefilters too.
  private Task<bool> ReferenceExistsAsync<TReference>(IReadOnlyList<IProperty> keyProperties, object?[] keyValues, CancellationToken cancellationToken)
    where TReference : class
  {
    return this.dbContext.Set<TReference>().AsNoTracking().AnyAsync(BuildKeyPredicate<TReference>(keyProperties, keyValues)!, cancellationToken);
  }
}
