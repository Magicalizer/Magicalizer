// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Models.Abstractions
{
  public interface IModel
  {
  }

  public interface IModel<TEntity> : IModel where TEntity : class, IEntity
  {
    TEntity ToEntity();
  }

  public interface IModel<TEntity, TFilter> : IModel<TEntity>
    where TEntity : class, IEntity
    where TFilter : class, IFilter
  {
  }
}