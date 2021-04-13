// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using ExtCore.Data.EntityFramework;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Data.Repositories.Abstractions;
using Magicalizer.Data.Repositories.EntityFramework.Extensions;
using Magicalizer.Filters.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Magicalizer.Data.Repositories.EntityFramework
{
  public class Repository<TKey, TEntity, TFilter> : RepositoryBase<TEntity>, IRepository<TKey, TEntity, TFilter>
    where TEntity : class, IEntity, new()
    where TFilter : class, IFilter
  {
    public virtual async Task<TEntity> GetByIdAsync(TKey id, params Inclusion<TEntity>[] inclusions)
    {
      IProperty key = this.storageContext.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties[0];

      return await this.dbSet
        .AsNoTracking()
        .ApplyInclusions(inclusions)
        .FirstOrDefaultAsync(e => EF.Property<TKey>(e, key.Name).Equals(id));
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(TFilter filter = null, string sorting = null, int? offset = null, int? limit = null, params Inclusion<TEntity>[] inclusions)
    {
      return await this.dbSet
        .AsNoTracking()
        .ApplyFiltering(filter)
        .ApplySorting(sorting)
        .ApplyPaging(offset, limit)
        .ApplyInclusions(inclusions)
        .ToListAsync();
    }
    
    public virtual async Task<int> CountAsync(TFilter filter = null)
    {
      return await this.dbSet
        .AsNoTracking()
        .ApplyFiltering(filter)
        .CountAsync();
    }

    public virtual void Create(TEntity entity)
    {
      this.dbSet.Add(entity);
    }

    public virtual void Edit(TEntity entity)
    {
      (this.storageContext as DbContext).Entry(entity).State = EntityState.Modified;
    }

    public virtual void Delete(TKey id)
    {
      TEntity entity = new TEntity();
      IProperty key = this.storageContext.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties[0];
      PropertyInfo property = typeof(TEntity).GetProperty(key.Name);

      property.SetValue(entity, id);
      this.dbSet.Remove(entity);
    }
  }

  public class Repository<TKey1, TKey2, TEntity, TFilter> : RepositoryBase<TEntity>, IRepository<TKey1, TKey2, TEntity, TFilter>
    where TEntity : class, IEntity, new()
    where TFilter : class, IFilter
  {
    public virtual async Task<TEntity> GetByIdAsync(TKey1 id1, TKey2 id2, params Inclusion<TEntity>[] inclusions)
    {
      IProperty key1 = this.storageContext.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties[0];
      IProperty key2 = this.storageContext.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties[1];

      return await this.dbSet
        .AsNoTracking()
        .ApplyInclusions(inclusions)
        .FirstOrDefaultAsync(e => EF.Property<TKey1>(e, key1.Name).Equals(id1) && EF.Property<TKey2>(e, key2.Name).Equals(id2));
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(TFilter filter = null, string sorting = null, int? offset = null, int? limit = null, params Inclusion<TEntity>[] inclusions)
    {
      return await this.dbSet
        .AsNoTracking()
        .ApplyFiltering(filter)
        .ApplySorting(sorting)
        .ApplyPaging(offset, limit)
        .ApplyInclusions(inclusions)
        .ToListAsync();
    }

    public virtual async Task<int> CountAsync(TFilter filter = null)
    {
      return await this.dbSet
        .AsNoTracking()
        .ApplyFiltering(filter)
        .CountAsync();
    }

    public virtual void Create(TEntity entity)
    {
      this.dbSet.Add(entity);
    }

    public virtual void Edit(TEntity entity)
    {
      (this.storageContext as DbContext).Entry(entity).State = EntityState.Modified;
    }

    public virtual void Delete(TKey1 id1, TKey2 id2)
    {
      TEntity entity = new TEntity();
      IProperty key1 = this.storageContext.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties[0];
      PropertyInfo property1 = typeof(TEntity).GetProperty(key1.Name);

      property1.SetValue(entity, id1);

      IProperty key2 = this.storageContext.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties[1];
      PropertyInfo property2 = typeof(TEntity).GetProperty(key2.Name);

      property2.SetValue(entity, id2);
      this.dbSet.Remove(entity);
    }
  }
}