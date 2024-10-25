// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Magicalizer.Api.Dto.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Magicalizer.Api;

/// <summary>
/// Applies a custom routing convention for controllers that use DTOs decorated with the <see cref="MagicalizedAttribute"/>.
/// This convention dynamically sets the route for controllers based on the route specified in the attribute.
/// </summary>
public class DefaultControllerRouteConvention : IControllerModelConvention
{
  /// <summary>
  /// Applies the route convention to the controller.
  /// If the controller is generic and uses a DTO that implements <see cref="IDto"/> and is decorated with
  /// the <see cref="MagicalizedAttribute"/>, the route specified in the attribute is applied to the controller.
  /// </summary>
  /// <param name="controller">The controller model to which the convention is applied.</param>
  public void Apply(ControllerModel controller)
  {
    if (controller.ControllerType.IsGenericType)
    {
      Type? dtoType = controller.ControllerType.GenericTypeArguments.FirstOrDefault(t => typeof(IDto).IsAssignableFrom(t));
      MagicalizedAttribute? magicalizedAttribute = dtoType?.GetCustomAttribute<MagicalizedAttribute>();

      if (magicalizedAttribute?.Route != null)
      {
        controller.Selectors.Add(new SelectorModel {
          AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(magicalizedAttribute.Route)),
        });
      }
    }
  }
}
