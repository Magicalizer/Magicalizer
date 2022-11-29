// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Data.Repositories.Abstractions
{
  /// <summary>
  /// Extends the ExtCore's <see cref="ExtCore.Data.Abstractions.IStorage"/> interface with the methods
  /// to get a repository (specific or generic one) for the given entity and filter types.
  /// </summary>
  public interface IStorage : ExtCore.Data.Abstractions.IStorage
  {
    /// <summary>
    /// Gets a repository (specific or generic one). If there is the specific implementation of the
    /// <see cref="IRepository{TKey, TModel, TFilter}" />, it will be returned.
    /// Otherwise, the generic implementation will be returned.
    /// </summary>
    /// <typeparam name="TKey">An entity's primary key type.</typeparam>
    /// <typeparam name="TEntity">An entity type.</typeparam>
    /// <typeparam name="TFilter">An entity filter type.</typeparam>
    IRepository<TKey, TEntity, TFilter> GetRepository<TKey, TEntity, TFilter>()
      where TEntity : class, IEntity, new()
      where TFilter : class, IFilter;

    /// <summary>
    /// Gets a repository (specific or generic one). If there is the specific implementation of the
    /// <see cref="IRepository{TKey1, TKey2, TModel, TFilter}" />, it will be returned.
    /// Otherwise, the generic implementation will be returned.
    /// </summary>
    /// <typeparam name="TKey1">The first entity's composite primary key type.</typeparam>
    /// <typeparam name="TKey2">The second entity's composite primary key type.</typeparam>
    /// <typeparam name="TEntity">An entity type.</typeparam>
    /// <typeparam name="TFilter">An entity filter type.</typeparam>
    IRepository<TKey1, TKey2, TEntity, TFilter> GetRepository<TKey1, TKey2, TEntity, TFilter>()
      where TEntity : class, IEntity, new()
      where TFilter : class, IFilter;

    /// <summary>
    /// Gets a repository (specific or generic one). If there is the specific implementation of the
    /// <see cref="IRepository{TKey1, TKey2, TKey3, TModel, TFilter}" />, it will be returned.
    /// Otherwise, the generic implementation will be returned.
    /// </summary>
    /// <typeparam name="TKey1">The first entity's composite primary key type.</typeparam>
    /// <typeparam name="TKey2">The second entity's composite primary key type.</typeparam>
    /// <typeparam name="TKey3">The third entity's composite primary key type.</typeparam>
    /// <typeparam name="TEntity">An entity type.</typeparam>
    /// <typeparam name="TFilter">An entity filter type.</typeparam>
    IRepository<TKey1, TKey2, TKey3, TEntity, TFilter> GetRepository<TKey1, TKey2, TKey3, TEntity, TFilter>()
      where TEntity : class, IEntity, new()
      where TFilter : class, IFilter;
  }
}