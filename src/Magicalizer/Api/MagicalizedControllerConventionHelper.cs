// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;
using Magicalizer.Api.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using HttpMethod = Magicalizer.Api.Dto.Abstractions.HttpMethod;

namespace Magicalizer.Api;

/// <summary>
/// Shared logic of the magicalized controller conventions (also applied to custom controllers derived from them).
/// </summary>
internal static class MagicalizedControllerConventionHelper
{
  // Generic type definitions of the magicalized controllers, which declare the standard actions.
  private static readonly Type[] DefaultControllerTypes = [
    typeof(MagicalizedControllerBase<,,>),
    typeof(MagicalizedController<,,,>),
    typeof(MagicalizedController<,,,,>),
    typeof(MagicalizedController<,,,,,>)
  ];

  /// <summary>
  /// Gets the DTO type of a magicalized controller or of a custom controller derived from it.
  /// </summary>
  /// <param name="controllerType">Controller type.</param>
  /// <returns>DTO type, or <c>null</c> if the controller isn't derived from the magicalized controllers.</returns>
  public static Type? GetDtoType(Type controllerType)
  {
    for (Type? type = controllerType; type != null; type = type.BaseType)
    {
      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(MagicalizedControllerBase<,,>))
        return type.GenericTypeArguments[1];
    }

    return null;
  }

  /// <summary>
  /// Gets the HTTP method of a standard action that is declared in the magicalized controllers (even if it's overridden).
  /// </summary>
  /// <param name="actionMethod">Action method.</param>
  /// <returns>
  /// HTTP method, or <c>null</c> if the action isn't a standard one (e.g. it's added by a custom controller).
  /// </returns>
  public static HttpMethod? GetHttpMethod(MethodInfo actionMethod)
  {
    MethodInfo baseDefinition = actionMethod.GetBaseDefinition();
    Type? declaringType = baseDefinition.DeclaringType;

    if (declaringType == null || !declaringType.IsGenericType || !DefaultControllerTypes.Contains(declaringType.GetGenericTypeDefinition()))
      return null;

    string? httpMethod = baseDefinition.GetCustomAttribute<HttpMethodAttribute>()?.HttpMethods.FirstOrDefault();

    return Enum.TryParse(httpMethod, ignoreCase: true, out HttpMethod result) ? result : null;
  }

  /// <summary>
  /// Checks whether the action is the standard one that gets a list (even if it's overridden), not the one that gets a
  /// model by the primary key.
  /// </summary>
  /// <param name="actionMethod">Action method.</param>
  /// <returns><c>true</c> if the action gets a list.</returns>
  public static bool IsListAction(MethodInfo actionMethod)
  {
    Type? declaringType = actionMethod.GetBaseDefinition().DeclaringType;

    // The actions by the primary key are declared in the controllers with the keys.
    return declaringType is { IsGenericType: true } && declaringType.GetGenericTypeDefinition() == typeof(MagicalizedControllerBase<,,>) && GetHttpMethod(actionMethod) == HttpMethod.Get;
  }

  /// <summary>
  /// Checks whether a rule that is set for an HTTP method (or for any one) applies to the HTTP method.
  /// </summary>
  /// <param name="ruleHttpMethod">HTTP method of the rule.</param>
  /// <param name="httpMethod">HTTP method to check.</param>
  /// <returns><c>true</c> if the rule applies.</returns>
  public static bool Matches(HttpMethod ruleHttpMethod, HttpMethod httpMethod)
  {
    return ruleHttpMethod == HttpMethod.Any || ruleHttpMethod == httpMethod;
  }
}
