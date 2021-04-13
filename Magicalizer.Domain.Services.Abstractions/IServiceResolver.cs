// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Services.Abstractions
{
  public interface IServiceResolver
  {
    IService<TKey, TModel, TFilter> GetService<TKey, TModel, TFilter>()
      where TModel : class, IModel
      where TFilter : class, IFilter;

    IService<TKey1, TKey2, TModel, TFilter> GetService<TKey1, TKey2, TModel, TFilter>()
      where TModel : class, IModel
      where TFilter : class, IFilter;
  }
}