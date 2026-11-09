// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Data.Entities.Abstractions;

/// <summary>
/// Entity without a primary key.
/// </summary>
public interface IEntity
{
}

/// <summary>
/// Entity with a primary key of one property.
/// </summary>
/// <typeparam name="TKey">Primary key type.</typeparam>
public interface IEntity<TKey> : IEntity
{
}

/// <summary>
/// Entity with a composite primary key of two properties.
/// </summary>
/// <typeparam name="TKey1">Type of the first primary key property.</typeparam>
/// <typeparam name="TKey2">Type of the second primary key property.</typeparam>
public interface IEntity<TKey1, TKey2> : IEntity<TKey1>
{
}

/// <summary>
/// Entity with a composite primary key of three properties.
/// </summary>
/// <typeparam name="TKey1">Type of the first primary key property.</typeparam>
/// <typeparam name="TKey2">Type of the second primary key property.</typeparam>
/// <typeparam name="TKey3">Type of the third primary key property.</typeparam>
public interface IEntity<TKey1, TKey2, TKey3> : IEntity<TKey1, TKey2>
{
}
