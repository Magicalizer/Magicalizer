// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Domain.Models.Abstractions;

/// <summary>
/// Model of the domain.
/// </summary>
public interface IModel
{
}

/// <summary>
/// Model that can be converted to an entity.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
public interface IModel<TEntity> : IModel where TEntity : class, IEntity
{
  /// <summary>
  /// Converts the model to the entity.
  /// </summary>
  /// <returns>Entity.</returns>
  TEntity ToEntity();
}

/// <summary>
/// Model that can be converted to an entity and filtered by a filter.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
/// <typeparam name="TFilter">Filter type.</typeparam>
public interface IModel<TEntity, TFilter> : IModel<TEntity>
  where TEntity : class, IEntity
  where TFilter : class, IFilter
{
}
