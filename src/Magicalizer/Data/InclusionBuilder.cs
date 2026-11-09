// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.PropertyPaths;

namespace Magicalizer.Data;

/// <summary>
/// Builder of an inclusion of related entities.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
public class InclusionBuilder<TEntity> : PropertyPathBuilder<TEntity, Inclusion<TEntity>> where TEntity : class, IEntity
{
  /// <summary>
  /// Creates a builder.
  /// </summary>
  public InclusionBuilder() : base(propertyPath => new Inclusion<TEntity>(propertyPath)) { }
}
