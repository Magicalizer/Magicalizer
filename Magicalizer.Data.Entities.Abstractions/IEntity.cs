// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Data.Entities.Abstractions
{
  /// <summary>
  /// Describes an entity.
  /// </summary>
  public interface IEntity : ExtCore.Data.Entities.Abstractions.IEntity
  {
  }

  /// <summary>
  /// Describes an entity.
  /// </summary>
  /// <typeparam name="TKey">An entity's primary key type.</typeparam>
  public interface IEntity<TKey> : IEntity
  {
  }

  /// <summary>
  /// Describes an entity.
  /// </summary>
  /// <typeparam name="TKey1">The first entity's composite primary key type.</typeparam>
  /// <typeparam name="TKey2">The second entity's composite primary key type.</typeparam>
  public interface IEntity<TKey1, TKey2> : IEntity<TKey1>
  {
  }
}