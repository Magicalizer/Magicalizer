// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;

namespace Magicalizer;

/// <summary>
/// Provides methods to correct property paths for a given type.
/// </summary>
public static class PropertyPathFixer
{
  private static readonly ConcurrentDictionary<Type, PropertyInfo[]> propertiesByTypes = [];

  /// <summary>
  /// Fixes the property path by matching it to the actual property names of the given type.
  /// </summary>
  /// <typeparam name="T">The type to fix the property path for.</typeparam>
  /// <param name="propertyPath">The property path to fix.</param>
  /// <returns>The corrected property path.</returns>
  public static string FixPropertyPath<T>(string propertyPath)
  {
    Type type = typeof(T);
    string[] propertyPathSegments = propertyPath.Split('.');
    IList<string> fixedPropertyPath = [];

    foreach (string propertyPathSegment in propertyPathSegments)
    {
      PropertyInfo[] properties = propertiesByTypes.GetOrAdd(type, static t => t.GetProperties());
      PropertyInfo? property = Array.Find(properties, p => p.Name.Equals(propertyPathSegment, StringComparison.OrdinalIgnoreCase));

      if (property == null)
        break;

      fixedPropertyPath.Add(property.Name);
      type = property.PropertyType.IsGenericType ?
        property.PropertyType.GetGenericArguments()[0] :
        property.PropertyType;
    }

    return string.Join('.', fixedPropertyPath);
  }
}