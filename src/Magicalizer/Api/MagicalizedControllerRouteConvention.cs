// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;
using Magicalizer.Api.Dto.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Magicalizer.Api;

/// <summary>
/// Sets the routes of the magicalized controllers from <see cref="MagicalizedAttribute"/> of their DTOs. The
/// controllers inherit the <see cref="RouteTemplate"/> route, which is replaced with the route of the DTO, so a custom
/// controller gets it too unless it has its own route.
/// </summary>
internal class MagicalizedControllerRouteConvention : IControllerModelConvention
{
  /// <summary>
  /// Route template of the magicalized controllers that the convention replaces with the route of the DTO. It's a
  /// token, so ASP.NET Core reports a controller whose route isn't replaced.
  /// </summary>
  public const string RouteTemplate = "[magicalized]";

  /// <inheritdoc/>
  /// <remarks>
  /// It replaces <see cref="RouteTemplate"/> with the route of the DTO and makes the names of the generic controllers
  /// unique.
  /// </remarks>
  public void Apply(ControllerModel controller)
  {
    Type? dtoType = MagicalizedControllerConventionHelper.GetDtoType(controller.ControllerType);

    if (dtoType == null) return;

    // All magicalized controllers share the same type name, so the DTO name is added to make the controller names
    // unique (e.g. "MagicalizedController<Product>"). Otherwise, links (e.g. to a created model) can be generated for
    // another controller.
    if (controller.ControllerType.IsGenericType)
      controller.ControllerName = $"MagicalizedController<{dtoType.Name}>";

    string? route = dtoType.GetCustomAttribute<MagicalizedAttribute>()?.Route;

    if (route == null) return;

    foreach (SelectorModel selector in controller.Selectors.Where(s => s.AttributeRouteModel?.Template == RouteTemplate))
      selector.AttributeRouteModel!.Template = route;
  }
}
