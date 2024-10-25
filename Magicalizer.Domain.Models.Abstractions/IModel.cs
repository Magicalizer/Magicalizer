// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Models.Abstractions;

/// <summary>
/// Base interface for a model.
/// </summary>
public interface IModel
{
}

/// <summary>
/// Base interface for a model that can be mapped to an entity.
/// </summary>
/// <typeparam name="TEntity">The entity type that this model maps to.</typeparam>
public interface IModel<TEntity> : IModel where TEntity : class, IEntity
{
  /// <summary>
  /// Converts the model to its corresponding entity.
  /// </summary>
  /// <returns>The corresponding entity.</returns>
  TEntity ToEntity();
}

/// <summary>
/// Base interface for a model that can be mapped to an entity and queried using a filter.
/// </summary>
/// <typeparam name="TEntity">The entity type that this model maps to.</typeparam>
/// <typeparam name="TFilter">he filter type used to query models.</typeparam>
public interface IModel<TEntity, TFilter> : IModel<TEntity>
  where TEntity : class, IEntity
  where TFilter : class, IFilter
{
}