// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Magicalizer.Data.Entities.Abstractions;

namespace Magicalizer.Data.Extensions;

/// <summary>
/// Provides extension method for applying sorting to queries.
/// </summary>
public static class SortingQueryableExtensions
{
  private static readonly ConcurrentDictionary<Type, PropertyInfo[]> propertiesByTypes = [];
  private static readonly MethodInfo orderByMethod = GetQueryableMethod(nameof(Queryable.OrderBy));
  private static readonly MethodInfo orderByDescendingMethod = GetQueryableMethod(nameof(Queryable.OrderByDescending));
  private static readonly MethodInfo thenByMethod = GetQueryableMethod(nameof(Queryable.ThenBy));
  private static readonly MethodInfo thenByDescendingMethod = GetQueryableMethod(nameof(Queryable.ThenByDescending));

  /// <summary>
  /// Applies sorting to the specified query using a collection of <see cref="Sorting{TEntity}"/> objects.
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity being queried.</typeparam>
  /// <param name="query">The source query to apply sorting to.</param>
  /// <param name="sortings">A collection of <see cref="Sorting{TEntity}"/> objects.</param>
  /// <returns>The query with the applied sorting.</returns>
  /// <exception cref="InvalidSortingException">
  /// Thrown when a property path doesn't point to a sortable (scalar) property of the entity.
  /// </exception>
  public static IQueryable<TEntity> ApplySorting<TEntity>(this IQueryable<TEntity> query, IEnumerable<Sorting<TEntity>>? sortings)
    where TEntity : class, IEntity
  {
    if (sortings == null) return query;

    bool isOrdered = false;

    foreach (Sorting<TEntity> sorting in sortings)
    {
      LambdaExpression keySelector = BuildKeySelector(typeof(TEntity), sorting.PropertyPath);
      MethodInfo method = isOrdered ?
        (sorting.IsAscending ? thenByMethod : thenByDescendingMethod) :
        (sorting.IsAscending ? orderByMethod : orderByDescendingMethod);

      // The same as calling Queryable.OrderBy(query, keySelector) etc., but with the key type known only at runtime.
      query = query.Provider.CreateQuery<TEntity>(
        Expression.Call(null, method.MakeGenericMethod(typeof(TEntity), keySelector.ReturnType), query.Expression, Expression.Quote(keySelector))
      );

      isOrdered = true;
    }

    return query;
  }

  // Builds a strongly typed key selector (e.g. "e => e.Category.Name") for the given property path.
  // Property names are matched case-insensitively. Every segment except the last one must be
  // a single (non-collection) navigation, and the last one must be a scalar property.
  private static LambdaExpression BuildKeySelector(Type entityType, string propertyPath)
  {
    string[] segments = propertyPath.Split('.');

    if (segments.Any(string.IsNullOrWhiteSpace))
      throw new InvalidSortingException(propertyPath, "the property path is empty or malformed");

    ParameterExpression parameter = Expression.Parameter(entityType, "e");
    Expression body = parameter;

    for (int i = 0; i < segments.Length; i++)
    {
      PropertyInfo[] properties = propertiesByTypes.GetOrAdd(body.Type, static t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
      PropertyInfo? property = Array.Find(properties, p => p.Name.Equals(segments[i], StringComparison.OrdinalIgnoreCase) && p.GetIndexParameters().Length == 0);

      if (property == null)
        throw new InvalidSortingException(propertyPath, $"the type '{body.Type.Name}' doesn't have the property '{segments[i]}'");

      if (IsCollection(property.PropertyType))
        throw new InvalidSortingException(propertyPath, $"the property '{property.Name}' is a collection");

      bool isLast = i == segments.Length - 1;

      if (isLast && !IsScalar(property.PropertyType))
        throw new InvalidSortingException(propertyPath, $"the property '{property.Name}' is not a scalar value");

      body = Expression.Property(body, property);
    }

    return Expression.Lambda(body, parameter);
  }

  // Checks if the type is a collection (strings and byte arrays are treated as scalar values).
  private static bool IsCollection(Type type)
  {
    return type != typeof(string) && type != typeof(byte[]) && typeof(IEnumerable).IsAssignableFrom(type);
  }

  // Checks if the type is a scalar value (a value type or a string) that can be used as a sorting key.
  private static bool IsScalar(Type type)
  {
    type = Nullable.GetUnderlyingType(type) ?? type;
    return type.IsValueType || type == typeof(string);
  }

  // Gets the queryable sorting method (e.g. OrderBy) that accepts a query and a key selector.
  private static MethodInfo GetQueryableMethod(string name)
  {
    return typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
      .Single(m => m.Name == name && m.GetParameters().Length == 2);
  }
}