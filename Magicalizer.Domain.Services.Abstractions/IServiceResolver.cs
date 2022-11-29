// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Services.Abstractions
{
  /// <summary>
  /// Describes a service resolver. Service resolver either finds a specific service implementation
  /// for a given model type or creates a generic one.
  /// </summary>
  public interface IServiceResolver
  {
    /// <summary>
    /// Gets a service (specific or generic one). If there is the specific implementation of the
    /// <see cref="IService{TKey, TModel, TFilter}" />, it will be returned.
    /// Otherwise, the generic implementation will be returned.
    /// </summary>
    /// <typeparam name="TKey">A model's unique identifier type.</typeparam>
    /// <typeparam name="TModel">A model type.</typeparam>
    /// <typeparam name="TFilter">A model filter type.</typeparam>
    IService<TKey, TModel, TFilter> GetService<TKey, TModel, TFilter>()
      where TModel : class, IModel
      where TFilter : class, IFilter;

    /// <summary>
    /// Gets a service (specific or generic one). If there is the specific implementation of the
    /// <see cref="IService{TKey1, TKey2, TModel, TFilter}" />, it will be returned.
    /// Otherwise, the generic implementation will be returned.
    /// </summary>
    /// <typeparam name="TKey1">The first model's unique identifier type.</typeparam>
    /// <typeparam name="TKey2">The first model's unique identifier type.</typeparam>
    /// <typeparam name="TModel">A model type.</typeparam>
    /// <typeparam name="TFilter">A model filter type.</typeparam>
    IService<TKey1, TKey2, TModel, TFilter> GetService<TKey1, TKey2, TModel, TFilter>()
      where TModel : class, IModel
      where TFilter : class, IFilter;

    /// <summary>
    /// Gets a service (specific or generic one). If there is the specific implementation of the
    /// <see cref="IService{TKey1, TKey2, TKey3, TModel, TFilter}" />, it will be returned.
    /// Otherwise, the generic implementation will be returned.
    /// </summary>
    /// <typeparam name="TKey1">The first model's unique identifier type.</typeparam>
    /// <typeparam name="TKey2">The first model's unique identifier type.</typeparam>
    /// <typeparam name="TKey3">The third model's unique identifier type.</typeparam>
    /// <typeparam name="TModel">A model type.</typeparam>
    /// <typeparam name="TFilter">A model filter type.</typeparam>
    IService<TKey1, TKey2, TKey3, TModel, TFilter> GetService<TKey1, TKey2, TKey3, TModel, TFilter>()
      where TModel : class, IModel
      where TFilter : class, IFilter;
  }
}