// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Magicalizer
{
  /// <summary>
  /// Contains the extension methods of the <see cref="Type"/>.
  /// </summary>
  public static class TypeExtensions
  {
    /// <summary>
    /// Gets all the type parameters of the specified generic interface (<paramref name="genericInterfaceBaseType"/>)
    /// that the given <paramref name="type"/> implements, and then finds the first one that implements a given interface
    /// (<paramref name="genericTypeParameterBaseType"/>).
    /// </summary>
    /// <param name="type">The given type that implements the specified generic interface (<paramref name="genericInterfaceBaseType"/>).</param>
    /// <param name="genericInterfaceBaseType">The generic interface that the given <paramref name="type"/> implements.</param>
    /// <param name="genericTypeParameterBaseType">The generic interface's type parameter's base type.</param>
    public static Type GetGenericInterfaceTypeParameter(this Type type, Type genericInterfaceBaseType, Type genericTypeParameterBaseType)
    {
      Type genericInterfaceType = type.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericInterfaceBaseType);

      return genericInterfaceType?.GetGenericArguments().FirstOrDefault(t => genericTypeParameterBaseType.IsAssignableFrom(t));
    }

    /// <summary>
    /// Gets all the type parameters of the specified generic interface (<paramref name="genericInterfaceBaseType"/>)
    /// that the given <paramref name="type"/> implements.
    /// </summary>
    /// <param name="type">The given type that implements the specified generic interface (<paramref name="genericInterfaceBaseType"/>).</param>
    /// <param name="genericInterfaceBaseType">The generic interface that the given <paramref name="type"/> implements.</param>
    public static IEnumerable<Type> GetGenericInterfaceTypeParameters(this Type type, Type genericInterfaceBaseType)
    {
      Type genericInterfaceType = type.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericInterfaceBaseType);

      return genericInterfaceType?.GetGenericArguments();
    }
  }
}