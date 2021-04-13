// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Data.Entities.Abstractions
{
  public interface IEntity : ExtCore.Data.Entities.Abstractions.IEntity
  {
  }

  public interface IEntity<TKey> : IEntity
  {
  }

  public interface IEntity<TKey1, TKey2> : IEntity<TKey1>
  {
  }
}