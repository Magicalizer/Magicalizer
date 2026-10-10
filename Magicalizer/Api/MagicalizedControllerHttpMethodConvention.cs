// Copyright © 2026 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Magicalizer.Api.Dto.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using HttpMethod = Magicalizer.Api.Dto.Abstractions.HttpMethod;

namespace Magicalizer.Api;

/// <summary>
/// Removes the standard actions whose HTTP methods are not supported for a DTO (see <see cref="MagicalizedAttribute.Methods"/>)
/// from the magicalized controllers, so such requests are rejected by the routing and the actions are not described by the API explorer.
/// </summary>
public class MagicalizedControllerHttpMethodConvention : IControllerModelConvention
{
  /// <summary>
  /// Removes the actions of the unsupported HTTP methods from the controller.
  /// </summary>
  /// <param name="controller">The controller model to which the convention is applied.</param>
  public void Apply(ControllerModel controller)
  {
    Type? dtoType = MagicalizedControllerConventionHelper.GetDtoType(controller.ControllerType);
    MagicalizedAttribute? magicalizedAttribute = dtoType?.GetCustomAttribute<MagicalizedAttribute>();

    // No methods are specified, so all of them are supported.
    if (magicalizedAttribute?.Methods == null || magicalizedAttribute.Methods.Length == 0) return;

    foreach (ActionModel action in controller.Actions.ToList())
    {
      HttpMethod? httpMethod = MagicalizedControllerConventionHelper.GetHttpMethod(action.ActionMethod);

      if (httpMethod != null && !magicalizedAttribute.Methods.Any(m => MagicalizedControllerConventionHelper.Matches(m, httpMethod.Value)))
        controller.Actions.Remove(action);
    }
  }
}
