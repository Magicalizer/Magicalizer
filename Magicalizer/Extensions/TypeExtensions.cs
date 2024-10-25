// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Extensions;

/// <summary>
/// Provides extension methods for working with the <see cref="Type"/> class.
/// </summary>
public static class TypeExtensions
{
  /// <summary>
  /// Gets all the non-abstract classes that implement the specified interface.
  /// </summary>
  /// <param name="type">The interface type to find implementations for.</param>
  /// <returns>A collection of non-abstract classes that implement the interface.</returns>
  public static IEnumerable<Type> GetImplementations(this Type type)
  {
    return AppDomain.CurrentDomain.GetAssemblies()
      .SelectMany(assembly => assembly.GetTypes())
      .Where(t => type.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
      .ToList();
  }

  /// <summary>
  /// Gets the first generic type parameter of a specified interface that implements a given base type.
  /// </summary>
  /// <param name="type">The type that implements the generic interface.</param>
  /// <param name="genericInterfaceBaseType">The generic interface to look for.</param>
  /// <param name="genericTypeParameterBaseType">The base type the generic type parameter must implement.</param>
  /// <returns>The first matching generic type parameter, or <c>null</c> if not found.</returns>
  public static Type? GetGenericInterfaceTypeParameter(this Type type, Type genericInterfaceBaseType, Type genericTypeParameterBaseType)
  {
    return type.GetGenericInterfaceTypeParameters(genericInterfaceBaseType)?.FirstOrDefault(genericTypeParameterBaseType.IsAssignableFrom);
  }

  /// <summary>
  /// Gets all the type parameters of a specified generic interface that the given type implements.
  /// </summary>
  /// <param name="type">The type that implements the specified generic interface.</param>
  /// <param name="genericInterfaceBaseType">The generic interface to search for.</param>
  /// <returns>
  /// A collection of type parameters from the generic interface implemented by the given type,
  /// or <c>null</c> if the interface is not implemented.
  /// </returns>
  public static IEnumerable<Type>? GetGenericInterfaceTypeParameters(this Type type, Type genericInterfaceBaseType)
  {
    Type? genericInterfaceType = type.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericInterfaceBaseType);

    return genericInterfaceType?.GetGenericArguments();
  }
}