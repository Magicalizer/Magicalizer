// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Data.Repositories.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Services.Defaults
{
  public class Service<TKey1, TKey2, TEntity, TModel, TFilter> : ServiceBase<TEntity, TModel, TFilter>, IService<TKey1, TKey2, TModel, TFilter>
    where TEntity : class, IEntity, new()
    where TModel : class, IModel
    where TFilter : class, IFilter
  {
    public Service(IStorage storage, IServiceProvider serviceProvider) : base(storage, serviceProvider) { }

    public virtual async Task<TModel> GetByIdAsync(TKey1 id1, TKey2 id2, params string[] inclusions)
    {
      return this.EntityToModel(await this.GetRepository().GetByIdAsync(id1, id2, this.ToInclusions(inclusions)));
    }

    public virtual async Task<IEnumerable<TModel>> GetAllAsync(TFilter filter = null, string sorting = null, int? offset = null, int? limit = null, params string[] inclusions)
    {
      return (await this.GetRepository().GetAllAsync(filter, sorting, offset, limit, this.ToInclusions(inclusions))).Select(e => this.EntityToModel(e));
    }

    public virtual async Task<int> CountAsync(TFilter filter = null)
    {
      return await this.GetRepository().CountAsync(filter);
    }

    public virtual async Task<TModel> CreateAsync(TModel model)
    {
      this.GetValidator()?.ValidateAndThrow(model);

      TEntity entity = (model as IModel<TEntity>).ToEntity();

      this.GetRepository().Create(entity);
      await this.storage.SaveAsync();
      return this.EntityToModel(entity);
    }

    public virtual async Task EditAsync(TModel model)
    {
      this.GetValidator()?.ValidateAndThrow(model);
      this.GetRepository().Edit((model as IModel<TEntity>).ToEntity());
      await this.storage.SaveAsync();
    }

    public virtual async Task<bool> DeleteAsync(TKey1 id1, TKey2 id2)
    {
      try
      {
        this.GetRepository().Delete(id1, id2);
        await this.storage.SaveAsync();
        return true;
      }

      catch { return false; }
    }

    protected virtual IRepository<TKey1, TKey2, TEntity, TFilter> GetRepository()
    {
      return this.storage.GetRepository<TKey1, TKey2, TEntity, TFilter>();
    }
  }
}