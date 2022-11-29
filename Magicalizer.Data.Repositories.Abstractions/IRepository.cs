// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Data.Repositories.Abstractions
{
  /// <summary>
  /// Describes a repository to manipulate the entities.
  /// </summary>
  /// <typeparam name="TKey">An entity's primary key type.</typeparam>
  /// <typeparam name="TEntity">An entity type.</typeparam>
  /// <typeparam name="TFilter">An entity filter type.</typeparam>
  public interface IRepository<TKey, TEntity, TFilter> : ExtCore.Data.Abstractions.IRepository
    where TEntity : class, IEntity, new()
    where TFilter : class, IFilter
  {
    /// <summary>
    /// Gets an entity by identifier.
    /// </summary>
    /// <param name="id">An entity's identifier.</param>
    /// <param name="inclusions">Navigation property paths that should be loaded with the entity.</param>
    Task<TEntity> GetByIdAsync(TKey id, params Inclusion<TEntity>[] inclusions);

    /// <summary>
    /// Gets the entities.
    /// </summary>
    /// <param name="filter">A filter object to filter the entities.</param>
    /// <param name="sorting">
    /// Comma-separated navigation property paths to sort the entities by
    /// (user "+" and "-" to specify sorting direction; example: "+category.name,-position").
    /// </param>
    /// <param name="offset">Number of the entities that should be skipped.</param>
    /// <param name="limit">Number of the entities that should be returned.</param>
    /// <param name="inclusions">Navigation property paths that should be loaded with the entities.</param>
    Task<IEnumerable<TEntity>> GetAllAsync(TFilter filter = null, string sorting = null, int? offset = null, int? limit = null, params Inclusion<TEntity>[] inclusions);

    /// <summary>
    /// Gets number of the entities.
    /// </summary>
    /// <param name="filter">A filter object to filter the entities.</param>
    Task<int> CountAsync(TFilter filter = null);

    /// <summary>
    /// Creates and returns an entity.
    /// </summary>
    /// <param name="entity">An entity to create.</param>
    void Create(TEntity entity);

    /// <summary>
    /// Edits an entity.
    /// </summary>
    /// <param name="entity">An entity to edit.</param>
    void Edit(TEntity entity);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="id">An entity's identifier.</param>
    void Delete(TKey id);
  }

  /// <summary>
  /// Describes a repository to manipulate the entities.
  /// </summary>
  /// <typeparam name="TKey1">The first entity's composite primary key type.</typeparam>
  /// <typeparam name="TKey2">The second entity's composite primary key type.</typeparam>
  /// <typeparam name="TEntity">An entity type.</typeparam>
  /// <typeparam name="TFilter">An entity filter type.</typeparam>
  public interface IRepository<TKey1, TKey2, TEntity, TFilter> : ExtCore.Data.Abstractions.IRepository
    where TEntity : class, IEntity, new()
    where TFilter : class, IFilter
  {
    /// <summary>
    /// Gets an entity by identifier.
    /// </summary>
    /// <param name="id1">The first entity's identifier.</param>
    /// <param name="id2">The second entity's identifier.</param>
    /// <param name="inclusions">Navigation property paths that should be loaded with the entity.</param>
    Task<TEntity> GetByIdAsync(TKey1 id1, TKey2 id2, params Inclusion<TEntity>[] inclusions);

    /// <summary>
    /// Gets the entities.
    /// </summary>
    /// <param name="filter">A filter object to filter the entities.</param>
    /// <param name="sorting">
    /// Comma-separated navigation property paths to sort the entities by
    /// (user "+" and "-" to specify sorting direction; example: "+category.name,-position").
    /// </param>
    /// <param name="offset">Number of the entities that should be skipped.</param>
    /// <param name="limit">Number of the entities that should be returned.</param>
    /// <param name="inclusions">Navigation property paths that should be loaded with the entities.</param>
    Task<IEnumerable<TEntity>> GetAllAsync(TFilter filter = null, string sorting = null, int? offset = null, int? limit = null, params Inclusion<TEntity>[] inclusions);

    /// <summary>
    /// Gets number of the entities.
    /// </summary>
    /// <param name="filter">A filter object to filter the entities.</param>
    Task<int> CountAsync(TFilter filter = null);

    /// <summary>
    /// Creates and returns an entity.
    /// </summary>
    /// <param name="entity">An entity to create.</param>
    void Create(TEntity entity);

    /// <summary>
    /// Edits an entity.
    /// </summary>
    /// <param name="entity">An entity to edit.</param>
    void Edit(TEntity entity);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="id1">The first entity's identifier.</param>
    /// <param name="id2">The second entity's identifier.</param>
    void Delete(TKey1 id1, TKey2 id2);
  }

  /// <summary>
  /// Describes a repository to manipulate the entities.
  /// </summary>
  /// <typeparam name="TKey1">The first entity's composite primary key type.</typeparam>
  /// <typeparam name="TKey2">The second entity's composite primary key type.</typeparam>
  /// <typeparam name="TKey3">The third entity's composite primary key type.</typeparam>
  /// <typeparam name="TEntity">An entity type.</typeparam>
  /// <typeparam name="TFilter">An entity filter type.</typeparam>
  public interface IRepository<TKey1, TKey2, TKey3, TEntity, TFilter> : ExtCore.Data.Abstractions.IRepository
    where TEntity : class, IEntity, new()
    where TFilter : class, IFilter
  {
    /// <summary>
    /// Gets an entity by identifier.
    /// </summary>
    /// <param name="id1">The first entity's identifier.</param>
    /// <param name="id2">The second entity's identifier.</param>
    /// <param name="id3">The third entity's identifier.</param>
    /// <param name="inclusions">Navigation property paths that should be loaded with the entity.</param>
    Task<TEntity> GetByIdAsync(TKey1 id1, TKey2 id2, TKey3 id3, params Inclusion<TEntity>[] inclusions);

    /// <summary>
    /// Gets the entities.
    /// </summary>
    /// <param name="filter">A filter object to filter the entities.</param>
    /// <param name="sorting">
    /// Comma-separated navigation property paths to sort the entities by
    /// (user "+" and "-" to specify sorting direction; example: "+category.name,-position").
    /// </param>
    /// <param name="offset">Number of the entities that should be skipped.</param>
    /// <param name="limit">Number of the entities that should be returned.</param>
    /// <param name="inclusions">Navigation property paths that should be loaded with the entities.</param>
    Task<IEnumerable<TEntity>> GetAllAsync(TFilter filter = null, string sorting = null, int? offset = null, int? limit = null, params Inclusion<TEntity>[] inclusions);

    /// <summary>
    /// Gets number of the entities.
    /// </summary>
    /// <param name="filter">A filter object to filter the entities.</param>
    Task<int> CountAsync(TFilter filter = null);

    /// <summary>
    /// Creates and returns an entity.
    /// </summary>
    /// <param name="entity">An entity to create.</param>
    void Create(TEntity entity);

    /// <summary>
    /// Edits an entity.
    /// </summary>
    /// <param name="entity">An entity to edit.</param>
    void Edit(TEntity entity);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="id1">The first entity's identifier.</param>
    /// <param name="id2">The second entity's identifier.</param>
    /// <param name="id3">The third entity's identifier.</param>
    void Delete(TKey1 id1, TKey2 id2, TKey3 id3);
  }
}