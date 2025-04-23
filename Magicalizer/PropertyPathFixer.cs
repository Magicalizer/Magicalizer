// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace Magicalizer;

/// <summary>
/// Provides methods to correct property paths for a given type.
/// </summary>
public static class PropertyPathFixer
{
  /// <summary>
  /// Fixes the property path by matching it to the actual property names of the given type.
  /// </summary>
  /// <typeparam name="T">The type to fix the property path for.</typeparam>
  /// <param name="propertyPath">The property path to fix.</param>
  /// <returns>The corrected property path.</returns>
  public static string FixPropertyPath<T>(string propertyPath)
  {
    Type type = typeof(T);
    IEnumerable<string> propertyPathSegments = propertyPath.Split('.');
    IList<string> fixedPropertyPath = [];

    foreach (string propertyPathSegment in propertyPathSegments)
    {
      PropertyInfo? property = type.GetProperty(propertyPathSegment, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

      if (property == null)
        break;

      fixedPropertyPath.Add(property.Name);
      type = property.PropertyType.IsGenericType ?
        property.PropertyType.GetGenericArguments().First() :
        property.PropertyType;
    }

    return string.Join('.', fixedPropertyPath);
  }
}