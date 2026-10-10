// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using Magicalizer.Data.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Magicalizer.Data.Extensions;

/// <summary>
/// Provides extension method for applying navigation property inclusions to queries.
/// </summary>
public static class InclusionsQueryableExtensions
{
  private static readonly ConcurrentDictionary<Type, PropertyInfo[]> propertiesByTypes = [];

  /// <summary>
  /// Applies inclusions to the specified query using a collection of <see cref="Inclusion{TEntity}"/> objects.
  /// </summary>
  /// <typeparam name="TEntity">The type of the entity being queried.</typeparam>
  /// <param name="query">The source query to apply the inclusions to.</param>
  /// <param name="inclusions">A collection of <see cref="Inclusion{TEntity}"/> objects specifying related entities to include.</param>
  /// <returns>The query with the applied inclusions.</returns>
  /// <exception cref="InvalidInclusionException">
  /// Thrown when a property path refers to a property that doesn't exist or is not a navigation property.
  /// </exception>
  public static IQueryable<TEntity> ApplyInclusions<TEntity>(this IQueryable<TEntity> query, IEnumerable<Inclusion<TEntity>>? inclusions)
    where TEntity : class, IEntity
  {
    if (inclusions == null) return query;

    foreach (Inclusion<TEntity> inclusion in inclusions)
      query = query.Include(ResolvePropertyPath(typeof(TEntity), inclusion.PropertyPath));

    return query;
  }

  // Matches every segment of the property path (case-insensitively) to an actual property and returns
  // the path with the correct property names (e.g. "category.products" -> "Category.Products").
  // Every segment must be a navigation property: a reference to another object or a collection of objects
  // (collection properties are followed into their element type). Scalar properties can't be included.
  private static string ResolvePropertyPath(Type entityType, string propertyPath)
  {
    string[] segments = propertyPath.Split('.');

    if (segments.Any(string.IsNullOrWhiteSpace))
      throw new InvalidInclusionException(propertyPath, "the property path is empty or malformed");

    Type type = entityType;
    string[] resolvedSegments = new string[segments.Length];

    for (int i = 0; i < segments.Length; i++)
    {
      PropertyInfo[] properties = propertiesByTypes.GetOrAdd(type, static t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
      PropertyInfo? property = Array.Find(properties, p => p.Name.Equals(segments[i], StringComparison.OrdinalIgnoreCase) && p.GetIndexParameters().Length == 0);

      if (property == null)
        throw new InvalidInclusionException(propertyPath, $"the type '{type.Name}' doesn't have the property '{segments[i]}'");

      Type targetType = GetElementType(property.PropertyType) ?? property.PropertyType;

      if (!IsNavigationTarget(targetType))
        throw new InvalidInclusionException(propertyPath, $"the property '{property.Name}' is not a navigation property");

      resolvedSegments[i] = property.Name;
      type = targetType;
    }

    return string.Join('.', resolvedSegments);
  }

  // Navigation properties always point to classes (entities), while scalar values are value types or strings.
  private static bool IsNavigationTarget(Type type)
  {
    return type.IsClass && type != typeof(string);
  }

  // Gets the element type of a collection, or null if the type is not a collection.
  private static Type? GetElementType(Type type)
  {
    if (type == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(type)) return null;

    return type.IsArray ?
      type.GetElementType() :
      type.GetInterfaces().Append(type).FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))?.GetGenericArguments()[0];
  }
}