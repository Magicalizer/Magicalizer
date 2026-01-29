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
/// Provides a service for managing models with a composite primary key consisting of two properties.
/// Supports CRUD operations and querying with filtering, sorting, and pagination.
/// </summary>
/// <typeparam name="TKey1">The type of the first property in the composite primary key.</typeparam>
/// <typeparam name="TKey2">The type of the second property in the composite primary key.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <typeparam name="TFilter">The type of the filter used to query models.</typeparam>
public class Service<TKey1, TKey2, TEntity, TModel, TFilter> : ServiceBase<TEntity, TModel, TFilter>, IService<TKey1, TKey2, TModel, TFilter>
  where TEntity : class, IEntity, new()
  where TModel : class, IModel
  where TFilter : class, IFilter
{
  /// <summary>
  /// Initializes a new instance of the <see cref="Service{TKey1, TKey2, TEntity, TModel, TFilter}"/> class.
  /// </summary>
  /// <param name="dbContext">The database context.</param>
  /// <param name="queryScopes">The optional collection of query scopes to apply global visibility rules or restrictions.</param>
  /// <param name="validator">The optional model validator.</param>
  public Service(DbContext dbContext, IEnumerable<IQueryScope<TEntity, TFilter>>? queryScopes = null, IValidator<TModel>? validator = null) : base(dbContext, queryScopes, validator) { }

  /// <summary>
  /// Retrieves a model by its composite primary key.
  /// </summary>
  /// <param name="id1">The value of the first property in the composite primary key.</param>
  /// <param name="id2">The value of the second property in the composite primary key.</param>
  /// <param name="inclusions">The inclusion property paths to include related models.</param>
  /// <returns>The model with the specified composite primary key, or <c>null</c> if no such model exists.</returns>
  public virtual async Task<TModel?> GetByIdAsync(TKey1 id1, TKey2 id2, params IInclusion<TModel>[] inclusions)
  {
    Microsoft.EntityFrameworkCore.Metadata.IProperty? key1 = this.GetPrimaryKeyProperty(0);
    Microsoft.EntityFrameworkCore.Metadata.IProperty? key2 = this.GetPrimaryKeyProperty(1);

    if (key1 == null || key2 == null) return null;

    TEntity? entity = await this.GetScopedQuery()
      .ApplyInclusions(inclusions.Select(i => new Data.Inclusion<TEntity>(i.PropertyPath)))
      .FirstOrDefaultAsync(
        e => EF.Property<TKey1>(e, key1.Name)!.Equals(id1) &&
             EF.Property<TKey2>(e, key2.Name)!.Equals(id2)
      );

    return this.EntityToModel(entity);
  }

  /// <summary>
  /// Deletes a model by its composite primary key.
  /// </summary>
  /// <param name="id1">The value of the first property in the composite primary key.</param>
  /// <param name="id2">The value of the second property in the composite primary key.</param>
  /// <returns>A boolean indicating success.</returns>
  public virtual async Task<bool> DeleteAsync(TKey1 id1, TKey2 id2)
  {
    try
    {
      TEntity entity = new TEntity();
      Microsoft.EntityFrameworkCore.Metadata.IProperty? key1 = GetPrimaryKeyProperty(0);
      Microsoft.EntityFrameworkCore.Metadata.IProperty? key2 = GetPrimaryKeyProperty(1);

      if (key1 == null || key2 == null) return false;

      PropertyInfo? property1 = typeof(TEntity).GetProperty(key1.Name);
      PropertyInfo? property2 = typeof(TEntity).GetProperty(key2.Name);

      if (property1 == null || property2 == null) return false;

      property1.SetValue(entity, id1);
      property2.SetValue(entity, id2);
      this.dbContext.Set<TEntity>().Remove(entity);
      await this.dbContext.SaveChangesAsync();
      return true;
    }

    catch { return false; }
  }
}