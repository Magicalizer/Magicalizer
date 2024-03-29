﻿// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using ExtCore.Data.Abstractions;
using ExtCore.Infrastructure;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Data.Repositories.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Data.Repositories.EntityFramework
{
  public class Storage : ExtCore.Data.EntityFramework.Storage, Abstractions.IStorage
  {
    public Storage(IStorageContext storageContext) : base(storageContext)
    {
    }

    public IRepository<TKey, TEntity, TFilter> GetRepository<TKey, TEntity, TFilter>()
      where TEntity : class, IEntity, new()
      where TFilter : class, IFilter
    {

      foreach (Type type in ExtensionManager.GetImplementations<IRepository<TKey, TEntity, TFilter>>(useCaching: true))
        if (type != typeof(Repository<TKey, TEntity, TFilter>))
        {
          IRepository<TKey, TEntity, TFilter> repository = Activator.CreateInstance(type) as IRepository<TKey, TEntity, TFilter>;

          repository.SetStorageContext(this.StorageContext);
          return repository;
        }

      {
        IRepository<TKey, TEntity, TFilter> repository = new Repository<TKey, TEntity, TFilter>();

        repository.SetStorageContext(this.StorageContext);
        return repository;
      }
    }

    public IRepository<TKey1, TKey2, TEntity, TFilter> GetRepository<TKey1, TKey2, TEntity, TFilter>()
     where TEntity : class, IEntity, new()
     where TFilter : class, IFilter
    {

      foreach (Type type in ExtensionManager.GetImplementations<IRepository<TKey1, TKey2, TEntity, TFilter>>(useCaching: true))
        if (type != typeof(Repository<TKey1, TKey2, TEntity, TFilter>))
        {
          IRepository<TKey1, TKey2, TEntity, TFilter> repository = Activator.CreateInstance(type) as IRepository<TKey1, TKey2, TEntity, TFilter>;

          repository.SetStorageContext(this.StorageContext);
          return repository;
        }

      {
        IRepository<TKey1, TKey2, TEntity, TFilter> repository = new Repository<TKey1, TKey2, TEntity, TFilter>();

        repository.SetStorageContext(this.StorageContext);
        return repository;
      }
    }

    public IRepository<TKey1, TKey2, TKey3, TEntity, TFilter> GetRepository<TKey1, TKey2, TKey3, TEntity, TFilter>()
     where TEntity : class, IEntity, new()
     where TFilter : class, IFilter
    {

      foreach (Type type in ExtensionManager.GetImplementations<IRepository<TKey1, TKey2, TKey3, TEntity, TFilter>>(useCaching: true))
        if (type != typeof(Repository<TKey1, TKey2, TKey3, TEntity, TFilter>))
        {
          IRepository<TKey1, TKey2, TKey3, TEntity, TFilter> repository = Activator.CreateInstance(type) as IRepository<TKey1, TKey2, TKey3, TEntity, TFilter>;

          repository.SetStorageContext(this.StorageContext);
          return repository;
        }

      {
        IRepository<TKey1, TKey2, TKey3, TEntity, TFilter> repository = new Repository<TKey1, TKey2, TKey3, TEntity, TFilter>();

        repository.SetStorageContext(this.StorageContext);
        return repository;
      }
    }
  }
}