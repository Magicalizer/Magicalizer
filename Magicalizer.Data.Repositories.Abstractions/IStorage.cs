// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Data.Repositories.Abstractions
{
  public interface IStorage : ExtCore.Data.Abstractions.IStorage
  {
    IRepository<TKey, TEntity, TFilter> GetRepository<TKey, TEntity, TFilter>()
      where TEntity : class, IEntity, new()
      where TFilter : class, IFilter;
  }
}