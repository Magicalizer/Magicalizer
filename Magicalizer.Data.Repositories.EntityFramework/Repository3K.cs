// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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
  public class Repository<TKey1, TKey2, TKey3, TEntity, TFilter> : RepositoryBase<TEntity>, IRepository<TKey1, TKey2, TKey3, TEntity, TFilter>
    where TEntity : class, IEntity, new()
    where TFilter : class, IFilter
  {
    public virtual async Task<TEntity> GetByIdAsync(TKey1 id1, TKey2 id2, TKey3 id3, params Inclusion<TEntity>[] inclusions)
    {
      IProperty key1 = this.GetPrimaryKeyProperty(0);
      IProperty key2 = this.GetPrimaryKeyProperty(1);
      IProperty key3 = this.GetPrimaryKeyProperty(2);

      return await this.dbSet
        .AsNoTracking()
        .ApplyInclusions(inclusions)
        .FirstOrDefaultAsync(e => EF.Property<TKey1>(e, key1.Name).Equals(id1) && EF.Property<TKey2>(e, key2.Name).Equals(id2) && EF.Property<TKey3>(e, key3.Name).Equals(id3));
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
      IProperty key1 = this.GetPrimaryKeyProperty(0);
      PropertyInfo property1 = typeof(TEntity).GetProperty(key1.Name);
      IProperty key2 = this.GetPrimaryKeyProperty(1);
      PropertyInfo property2 = typeof(TEntity).GetProperty(key2.Name);
      IProperty key3 = this.GetPrimaryKeyProperty(2);
      PropertyInfo property3 = typeof(TEntity).GetProperty(key3.Name);
      TEntity local = this.dbSet.Local.FirstOrDefault(
        e => property1.GetValue(e).Equals(property1.GetValue(entity)) &&
          property2.GetValue(e).Equals(property2.GetValue(entity)) &&
          property3.GetValue(e).Equals(property3.GetValue(entity))
      );

      if (local != null)
        this.storageContext.Entry(local).State = EntityState.Detached;

      (this.storageContext as DbContext).Entry(entity).State = EntityState.Modified;
    }

    public virtual void Delete(TKey1 id1, TKey2 id2, TKey3 id3)
    {
      TEntity entity = new TEntity();
      IProperty key1 = this.GetPrimaryKeyProperty(0);
      PropertyInfo property1 = typeof(TEntity).GetProperty(key1.Name);

      property1.SetValue(entity, id1);

      IProperty key2 = this.GetPrimaryKeyProperty(1);
      PropertyInfo property2 = typeof(TEntity).GetProperty(key2.Name);

      property2.SetValue(entity, id2);

      IProperty key3 = this.GetPrimaryKeyProperty(2);
      PropertyInfo property3 = typeof(TEntity).GetProperty(key3.Name);

      property3.SetValue(entity, id3);
      this.dbSet.Remove(entity);
    }

    private IProperty GetPrimaryKeyProperty(int index)
    {
      return this.storageContext.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties[index];
    }
  }
}