// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using ExtCore.Infrastructure;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Data.Repositories.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Services.Defaults
{
  public class ServiceResolver : IServiceResolver
  {
    private IStorage storage;
    private IServiceProvider serviceProvider;

    public ServiceResolver(IStorage storage, IServiceProvider serviceProvider)
    {
      this.storage = storage;
      this.serviceProvider = serviceProvider;
    }

    public IService<TKey, TModel, TFilter> GetService<TKey, TModel, TFilter>()
      where TModel : class, IModel
      where TFilter : class, IFilter
    {
      Type entityType = typeof(TModel).GetGenericInterfaceGenericArgument(typeof(IModel<>), typeof(IEntity));
      Type serviceType = typeof(Service<,,,>).MakeGenericType(typeof(TKey), entityType, typeof(TModel), typeof(TFilter));

      foreach (Type type in ExtensionManager.GetImplementations<IService<TKey, TModel, TFilter>>(useCaching: true))
        if (type != serviceType)
          return Activator.CreateInstance(type, this.storage, this.serviceProvider) as IService<TKey, TModel, TFilter>;

      return Activator.CreateInstance(serviceType, this.storage, this.serviceProvider) as IService<TKey, TModel, TFilter>;
    }
  }
}