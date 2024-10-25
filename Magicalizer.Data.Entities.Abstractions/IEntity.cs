// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Data.Entities.Abstractions;

/// <summary>
/// Base interface for an entity without a specified primary key.
/// </summary>
public interface IEntity
{
}

/// <summary>
/// Base interface for an entity with a single-property primary key.
/// </summary>
/// <typeparam name="TKey">The primary key type.</typeparam>
public interface IEntity<TKey> : IEntity
{
}

/// <summary>
/// Base interface for an entity with a composite primary key of two properties.
/// </summary>
/// <typeparam name="TKey1">The first primary key type.</typeparam>
/// <typeparam name="TKey2">The second primary key type.</typeparam>
public interface IEntity<TKey1, TKey2> : IEntity<TKey1>
{
}

/// <summary>
/// Base interface for an entity with a composite primary key of three properties.
/// </summary>
/// <typeparam name="TKey1">The first primary key type.</typeparam>
/// <typeparam name="TKey2">The second primary key type.</typeparam>
/// <typeparam name="TKey3">The third primary key type.</typeparam>
public interface IEntity<TKey1, TKey2, TKey3> : IEntity<TKey1, TKey2>
{
}