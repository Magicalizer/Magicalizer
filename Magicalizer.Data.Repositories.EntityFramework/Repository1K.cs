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
  public class Repository<TKey, TEntity, TFilter> : RepositoryBase<TEntity>, IRepository<TKey, TEntity, TFilter>
    where TEntity : class, IEntity, new()
    where TFilter : class, IFilter
  {
    public virtual async Task<TEntity> GetByIdAsync(TKey id, params Inclusion<TEntity>[] inclusions)
    {
      IProperty key = this.GetPrimaryKeyProperty(0);

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
      IProperty key = this.GetPrimaryKeyProperty(0);
      PropertyInfo property = typeof(TEntity).GetProperty(key.Name);
      TEntity local = this.dbSet.Local.FirstOrDefault(e => property.GetValue(e).Equals(property.GetValue(entity)));

      if (local != null)
        this.storageContext.Entry(local).State = EntityState.Detached;

      (this.storageContext as DbContext).Entry(entity).State = EntityState.Modified;
    }

    public virtual void Delete(TKey id)
    {
      TEntity entity = new TEntity();
      IProperty key = this.GetPrimaryKeyProperty(0);
      PropertyInfo property = typeof(TEntity).GetProperty(key.Name);

      property.SetValue(entity, id);
      this.dbSet.Remove(entity);
    }

    private IProperty GetPrimaryKeyProperty(int index)
    {
      return this.storageContext.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties[index];
    }
  }
}