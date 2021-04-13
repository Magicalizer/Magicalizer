// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Data.Repositories.Abstractions
{
  public interface IRepository<TKey, TEntity, TFilter> : ExtCore.Data.Abstractions.IRepository
    where TEntity : class, IEntity, new()
    where TFilter : class, IFilter
  {
    Task<TEntity> GetByIdAsync(TKey id, params Inclusion<TEntity>[] inclusions);
    Task<IEnumerable<TEntity>> GetAllAsync(TFilter filter = null, string sorting = null, int? offset = null, int? limit = null, params Inclusion<TEntity>[] inclusions);
    Task<int> CountAsync(TFilter filter = null);
    void Create(TEntity entity);
    void Edit(TEntity entity);
    void Delete(TKey id);
  }

  public interface IRepository<TKey1, TKey2, TEntity, TFilter> : ExtCore.Data.Abstractions.IRepository
    where TEntity : class, IEntity, new()
    where TFilter : class, IFilter
  {
    Task<TEntity> GetByIdAsync(TKey1 id1, TKey2 id2, params Inclusion<TEntity>[] inclusions);
    Task<IEnumerable<TEntity>> GetAllAsync(TFilter filter = null, string sorting = null, int? offset = null, int? limit = null, params Inclusion<TEntity>[] inclusions);
    Task<int> CountAsync(TFilter filter = null);
    void Create(TEntity entity);
    void Edit(TEntity entity);
    void Delete(TKey1 id1, TKey2 id2);
  }
}