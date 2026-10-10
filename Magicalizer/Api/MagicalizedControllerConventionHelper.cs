// Copyright © 2026 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Magicalizer.Api.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using HttpMethod = Magicalizer.Api.Dto.Abstractions.HttpMethod;

namespace Magicalizer.Api;

/// <summary>
/// Shared logic of the conventions that are applied to the magicalized controllers (and to the custom controllers derived from them).
/// </summary>
internal static class MagicalizedControllerConventionHelper
{
  private static readonly Type[] defaultControllerTypes = [
    typeof(MagicalizedControllerBase<,,>),
    typeof(MagicalizedController<,,,>),
    typeof(MagicalizedController<,,,,>),
    typeof(MagicalizedController<,,,,,>)
  ];

  /// <summary>
  /// Gets the DTO type of a magicalized controller or of a custom controller derived from it.
  /// </summary>
  /// <param name="controllerType">The controller type.</param>
  /// <returns>The DTO type, or <c>null</c> if the controller is not derived from the magicalized controllers.</returns>
  public static Type? GetDtoType(Type controllerType)
  {
    for (Type? type = controllerType; type != null; type = type.BaseType)
      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(MagicalizedControllerBase<,,>))
        return type.GenericTypeArguments[1];

    return null;
  }

  /// <summary>
  /// Gets the HTTP method of a standard action that is declared in the magicalized controllers (even if it is overridden).
  /// </summary>
  /// <param name="actionMethod">The action method.</param>
  /// <returns>The HTTP method, or <c>null</c> if the action is not a standard one (e.g. it is added by a custom controller).</returns>
  public static HttpMethod? GetHttpMethod(MethodInfo actionMethod)
  {
    MethodInfo baseDefinition = actionMethod.GetBaseDefinition();
    Type? declaringType = baseDefinition.DeclaringType;

    if (declaringType == null || !declaringType.IsGenericType || !defaultControllerTypes.Contains(declaringType.GetGenericTypeDefinition()))
      return null;

    string? httpMethod = baseDefinition.GetCustomAttribute<HttpMethodAttribute>()?.HttpMethods.FirstOrDefault();

    return Enum.TryParse(httpMethod, true, out HttpMethod result) ? result : null;
  }

  /// <summary>
  /// Checks if a rule that is specified for the given HTTP method (or for any one) applies to the specified HTTP method.
  /// </summary>
  /// <param name="ruleHttpMethod">The HTTP method of the rule.</param>
  /// <param name="httpMethod">The HTTP method to check.</param>
  /// <returns><c>true</c> if the rule applies; otherwise, <c>false</c>.</returns>
  public static bool Matches(HttpMethod ruleHttpMethod, HttpMethod httpMethod)
  {
    return ruleHttpMethod == HttpMethod.Any || ruleHttpMethod == httpMethod;
  }
}
