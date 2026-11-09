// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;
using Magicalizer.Api.Dto.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using HttpMethod = Magicalizer.Api.Dto.Abstractions.HttpMethod;

namespace Magicalizer.Api;

/// <summary>
/// Removes standard actions whose HTTP methods aren't supported for a DTO (see
/// <see cref="MagicalizedAttribute.Methods"/>) from the magicalized controllers, so such requests are rejected by
/// routing and the actions aren't described by the API explorer.
/// </summary>
internal class MagicalizedControllerHttpMethodConvention : IControllerModelConvention
{
  /// <inheritdoc/>
  /// <remarks>
  /// It removes actions of unsupported HTTP methods from the controller.
  /// </remarks>
  public void Apply(ControllerModel controller)
  {
    Type? dtoType = MagicalizedControllerConventionHelper.GetDtoType(controller.ControllerType);
    MagicalizedAttribute? magicalizedAttribute = dtoType?.GetCustomAttribute<MagicalizedAttribute>();

    // No methods are set, so all of them are supported.
    if (magicalizedAttribute?.Methods == null || magicalizedAttribute.Methods.Length == 0) return;

    foreach (ActionModel action in controller.Actions.ToList())
    {
      HttpMethod? httpMethod = MagicalizedControllerConventionHelper.GetHttpMethod(action.ActionMethod);

      if (httpMethod != null && !magicalizedAttribute.Methods.Any(m => MagicalizedControllerConventionHelper.Matches(m, httpMethod.Value)))
        controller.Actions.Remove(action);
    }
  }
}
