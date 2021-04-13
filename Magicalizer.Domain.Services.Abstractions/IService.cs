// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Services.Abstractions
{
  public interface IService<TKey, TModel, TFilter>
    where TModel : class, IModel
    where TFilter : class, IFilter
  {
    Task<TModel> GetByIdAsync(TKey id, params string[] inclusions);
    Task<IEnumerable<TModel>> GetAllAsync(TFilter filter = null, string sorting = null, int? offset = null, int? limit = null, params string[] inclusions);
    Task<int> CountAsync(TFilter filter = null);
    Task<TModel> CreateAsync(TModel model);
    Task EditAsync(TModel model);
    Task<bool> DeleteAsync(TKey id);
  }

  public interface IService<TKey1, TKey2, TModel, TFilter>
    where TModel : class, IModel
    where TFilter : class, IFilter
  {
    Task<TModel> GetByIdAsync(TKey1 id1, TKey2 id2, params string[] inclusions);
    Task<IEnumerable<TModel>> GetAllAsync(TFilter filter = null, string sorting = null, int? offset = null, int? limit = null, params string[] inclusions);
    Task<int> CountAsync(TFilter filter = null);
    Task<TModel> CreateAsync(TModel model);
    Task EditAsync(TModel model);
    Task<bool> DeleteAsync(TKey1 id1, TKey2 id2);
  }
}