// Copyright © 2026 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Magicalizer.Api.Dto.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Magicalizer.Api;

/// <summary>
/// Makes the magicalized controllers (and the custom controllers derived from them) visible to the API explorer, as
/// <see cref="ApiControllerAttribute"/> does, so the API description tools (e.g. OpenAPI) describe them if the
/// application uses them.
/// A generated controller has no class to mark with <see cref="ApiExplorerSettingsAttribute"/>, so the same settings are
/// set by <see cref="MagicalizedAttribute"/> of its DTO (e.g. <c>[Magicalized("v1/products", IgnoreApi = true)]</c> to
/// hide it, or <c>GroupName</c> to put it into another document). The more specific settings win, as in ASP.NET Core:
/// the ones of an action, then of a custom controller, then of the DTO, and the controller is visible if none of them is set.
/// </summary>
public class MagicalizedControllerApiExplorerConvention : IControllerModelConvention
{
  /// <summary>
  /// Applies the API explorer settings of the DTO to the controller (unless the controller has its own ones) and makes
  /// it visible if they don't say otherwise.
  /// </summary>
  /// <param name="controller">The controller model to which the convention is applied.</param>
  public void Apply(ControllerModel controller)
  {
    Type? dtoType = MagicalizedControllerConventionHelper.GetDtoType(controller.ControllerType);

    if (dtoType == null) return;

    MagicalizedAttribute? magicalizedAttribute = dtoType.GetCustomAttribute<MagicalizedAttribute>();

    // The settings of a custom controller are applied by ASP.NET Core already (the actions have their own ones too).
    controller.ApiExplorer.IsVisible ??= magicalizedAttribute?.IgnoreApi != true;
    controller.ApiExplorer.GroupName ??= magicalizedAttribute?.GroupName;
  }
}
