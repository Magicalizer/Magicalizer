// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;
using Magicalizer.Api.Dto.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Magicalizer.Api;

/// <summary>
/// A generated controller has no class to mark with <see cref="ApiExplorerSettingsAttribute"/>, so the same settings
/// are set by <see cref="MagicalizedAttribute"/> of its DTO (e.g. <c>[Magicalized("v1/products", IgnoreApi = true)]</c>
/// to hide it, or <c>GroupName</c> to put it into another document). More specific settings win, as in ASP.NET Core:
/// the ones of an action, then of a custom controller, then of the DTO, and the controller is visible if none of them
/// is set.
/// </summary>
internal class MagicalizedControllerApiExplorerConvention : IControllerModelConvention
{
  /// <inheritdoc/>
  /// <remarks>
  /// It applies API explorer settings of the DTO to the controller (unless the controller has its own ones) and makes
  /// it visible if they don't say otherwise.
  /// </remarks>
  public void Apply(ControllerModel controller)
  {
    Type? dtoType = MagicalizedControllerConventionHelper.GetDtoType(controller.ControllerType);

    if (dtoType == null) return;

    MagicalizedAttribute? magicalizedAttribute = dtoType.GetCustomAttribute<MagicalizedAttribute>();

    // The settings of a custom controller are applied by ASP.NET Core already (the actions have their own ones too).
    controller.ApiExplorer.IsVisible ??= magicalizedAttribute?.IgnoreApi != true;
    controller.ApiExplorer.GroupName ??= magicalizedAttribute?.GroupName;

    // ApiControllerAttribute makes the actions visible before the conventions are applied, so the visibility of the
    // controller is applied to the actions without their own settings.
    foreach (ActionModel action in controller.Actions.Where(a => !a.Attributes.OfType<ApiExplorerSettingsAttribute>().Any()))
      action.ApiExplorer.IsVisible = controller.ApiExplorer.IsVisible;
  }
}
