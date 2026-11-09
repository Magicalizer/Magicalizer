// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Magicalizer.Data.Extensions;

/// <summary>
/// Extensions of queries to apply inclusions of navigation properties.
/// </summary>
public static class InclusionQueryableExtensions
{
  /// <summary>
  /// Applies inclusions to the query.
  /// </summary>
  /// <typeparam name="TEntity">Entity type.</typeparam>
  /// <param name="query">Query to apply the inclusions to.</param>
  /// <param name="inclusions">Inclusions of related entities.</param>
  /// <returns>Query with inclusions applied.</returns>
  /// <exception cref="InvalidInclusionException">
  /// A property path refers to a property that doesn't exist or isn't a navigation property.
  /// </exception>
  public static IQueryable<TEntity> ApplyInclusions<TEntity>(this IQueryable<TEntity> query, IEnumerable<Inclusion<TEntity>>? inclusions)
    where TEntity : class, IEntity
  {
    if (inclusions == null) return query;

    foreach (Inclusion<TEntity> inclusion in inclusions)
      query = query.Include(ResolvePropertyPath(typeof(TEntity), inclusion.PropertyPath, model: null, maxDepth: null));

    return query;
  }

  // Gets the error of including the property path into the entities (the message of the exception the inclusion would
  // throw), or null if the property path is valid. It lets the API check an inclusion before it's applied. With the
  // model of the database context, the path is also checked for cycles, which EF Core doesn't include in no-tracking
  // queries. With the maximum depth, the path can't have more properties.
  internal static string? GetError(Type entityType, string propertyPath, IModel? model, int? maxDepth)
  {
    try
    {
      ResolvePropertyPath(entityType, propertyPath, model, maxDepth);
      return null;
    }

    catch (InvalidInclusionException exception)
    {
      return exception.Message;
    }
  }

  // Matches every segment of the property path (case-insensitively) to an actual property and returns the path with
  // correct property names (e.g. "category.products" -> "Category.Products"). Every segment must be a navigation
  // property: a reference to another object or a collection of objects (collection properties are followed into their
  // element type). Scalar properties can't be included. With the model, a navigation can't be the inverse of the
  // previous one (e.g. "Category.Products" of a product), as it leads back and makes a cycle.
  private static string ResolvePropertyPath(Type entityType, string propertyPath, IModel? model, int? maxDepth)
  {
    string[] segments = propertyPath.Split('.');

    if (segments.Any(string.IsNullOrWhiteSpace))
      throw new InvalidInclusionException(propertyPath, "the property path is empty or malformed");

    if (segments.Length > maxDepth)
      throw new InvalidInclusionException(propertyPath, $"the property path has more than {maxDepth} properties");

    Type type = entityType;
    string[] resolvedSegments = new string[segments.Length];
    INavigationBase? previousNavigation = null;

    for (int i = 0; i < segments.Length; i++)
    {
      PropertyInfo? property = type.GetPublicProperty(segments[i]);

      if (property == null)
        throw new InvalidInclusionException(propertyPath, $"the type '{type.Name}' doesn't have the property '{segments[i]}'");

      Type targetType = property.PropertyType.GetCollectionElementType() ?? property.PropertyType;

      if (!IsNavigationTarget(targetType))
        throw new InvalidInclusionException(propertyPath, $"the property '{property.Name}' isn't a navigation property");

      IEntityType? modelEntityType = model?.FindEntityType(type);
      INavigationBase? navigation = (INavigationBase?)modelEntityType?.FindNavigation(property.Name) ?? modelEntityType?.FindSkipNavigation(property.Name);

      if (previousNavigation != null && navigation?.Inverse == previousNavigation)
        throw new InvalidInclusionException(propertyPath, $"the property '{property.Name}' is the inverse of the property '{previousNavigation.Name}', so the path makes a cycle");

      resolvedSegments[i] = property.Name;
      previousNavigation = navigation;
      type = targetType;
    }

    return string.Join('.', resolvedSegments);
  }

  // Navigation properties always point to classes (entities), while scalar values are value types or strings.
  private static bool IsNavigationTarget(Type type)
  {
    return type.IsClass && type != typeof(string);
  }
}
