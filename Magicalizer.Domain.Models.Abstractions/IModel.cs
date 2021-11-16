// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Models.Abstractions
{
  /// <summary>
  /// Describes a model.
  /// </summary>
  public interface IModel
  {
  }

  /// <summary>
  /// Describes a model.
  /// </summary>
  /// <typeparam name="TEntity">An entity type the model is persisted in.</typeparam>
  public interface IModel<TEntity> : IModel where TEntity : class, IEntity
  {
    /// <summary>
    /// Creates an entity and maps current model on it.
    /// </summary>
    TEntity ToEntity();
  }

  /// <summary>
  /// Describes a model.
  /// </summary>
  /// <typeparam name="TEntity">An entity type the model is persisted in.</typeparam>
  /// <typeparam name="TFilter">A model filter type.</typeparam>
  public interface IModel<TEntity, TFilter> : IModel<TEntity>
    where TEntity : class, IEntity
    where TFilter : class, IFilter
  {
  }
}