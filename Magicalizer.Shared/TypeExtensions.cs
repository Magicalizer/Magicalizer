// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Magicalizer
{
  public static class TypeExtensions
  {
    public static IEnumerable<Type> GetGenericInterfaceArguments(this Type type, Type genericInterfaceBaseType)
    {
      Type genericInterfaceType = type.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericInterfaceBaseType);

      return genericInterfaceType?.GetGenericArguments();
    }

    public static Type GetGenericInterfaceGenericArgument(this Type type, Type genericInterfaceBaseType, Type genericArgumentBaseType)
    {
      Type genericInterfaceType = type.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericInterfaceBaseType);

      return genericInterfaceType?.GetGenericArguments().FirstOrDefault(t => genericArgumentBaseType.IsAssignableFrom(t));
    }
  }
}