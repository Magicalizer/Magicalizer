// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Extensions;

namespace Magicalizer.Data.Extensions;

/// <summary>
/// Extensions of queries to apply sortings.
/// </summary>
public static class SortingQueryableExtensions
{
  private static readonly MethodInfo OrderByMethod = GetQueryableMethod(nameof(Queryable.OrderBy));
  private static readonly MethodInfo OrderByDescendingMethod = GetQueryableMethod(nameof(Queryable.OrderByDescending));
  private static readonly MethodInfo ThenByMethod = GetQueryableMethod(nameof(Queryable.ThenBy));
  private static readonly MethodInfo ThenByDescendingMethod = GetQueryableMethod(nameof(Queryable.ThenByDescending));

  /// <summary>
  /// Applies sortings to the query, in their order.
  /// </summary>
  /// <typeparam name="TEntity">Entity type.</typeparam>
  /// <param name="query">Query to apply sortings to.</param>
  /// <param name="sortings">Sortings to apply.</param>
  /// <returns>Sorted query.</returns>
  /// <exception cref="InvalidSortingException">
  /// A property path doesn't point to a sortable (scalar) property of the entity.
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
        (sorting.IsAscending ? ThenByMethod : ThenByDescendingMethod) :
        (sorting.IsAscending ? OrderByMethod : OrderByDescendingMethod);

      // The same as calling Queryable.OrderBy(query, keySelector) etc., but with the key type known only at runtime.
      query = query.Provider.CreateQuery<TEntity>(
        Expression.Call(null, method.MakeGenericMethod(typeof(TEntity), keySelector.ReturnType), query.Expression, Expression.Quote(keySelector))
      );

      isOrdered = true;
    }

    return query;
  }

  // Gets the error of sorting the entities by the property path (the message of the exception the sorting would throw),
  // or null if the property path is valid. It lets the API check a sorting before it's applied.
  internal static string? GetError(Type entityType, string propertyPath)
  {
    try
    {
      BuildKeySelector(entityType, propertyPath);
      return null;
    }

    catch (InvalidSortingException exception)
    {
      return exception.Message;
    }
  }

  // Builds the strongly typed key selector (e.g. "e => e.Category.Name") of the property path. Property names are
  // matched case-insensitively. Every segment except the last one must be a single (not a collection) navigation, and
  // the last one must be a scalar property.
  private static LambdaExpression BuildKeySelector(Type entityType, string propertyPath)
  {
    string[] segments = propertyPath.Split('.');

    if (segments.Any(string.IsNullOrWhiteSpace))
      throw new InvalidSortingException(propertyPath, "the property path is empty or malformed");

    ParameterExpression parameter = Expression.Parameter(entityType, "e");
    Expression body = parameter;

    for (int i = 0; i < segments.Length; i++)
    {
      PropertyInfo? property = body.Type.GetPublicProperty(segments[i]);

      if (property == null)
        throw new InvalidSortingException(propertyPath, $"the type '{body.Type.Name}' doesn't have the property '{segments[i]}'");

      if (IsCollection(property.PropertyType))
        throw new InvalidSortingException(propertyPath, $"the property '{property.Name}' is a collection");

      bool isLast = i == segments.Length - 1;

      if (isLast && !IsScalar(property.PropertyType))
        throw new InvalidSortingException(propertyPath, $"the property '{property.Name}' isn't a scalar value");

      body = Expression.Property(body, property);
    }

    return Expression.Lambda(body, parameter);
  }

  // Checks whether the type is a collection (strings and byte arrays aren't, though a byte array isn't a scalar value
  // either, so it can't be a sorting key).
  private static bool IsCollection(Type type)
  {
    return type != typeof(string) && type != typeof(byte[]) && typeof(IEnumerable).IsAssignableFrom(type);
  }

  // Checks whether the type is a scalar value (a value type or a string) that can be a sorting key.
  private static bool IsScalar(Type type)
  {
    type = Nullable.GetUnderlyingType(type) ?? type;

    return type.IsValueType || type == typeof(string);
  }

  // Gets the queryable sorting method (e.g. OrderBy) that takes a query and a key selector.
  private static MethodInfo GetQueryableMethod(string name)
  {
    return typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
      .Single(m => m.Name == name && m.GetParameters().Length == 2);
  }
}
