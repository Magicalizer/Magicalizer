// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Magicalizer.Api.Dto.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Magicalizer.Api
{
  public class DefaultControllerRouteConvention : IControllerModelConvention
  {
    public void Apply(ControllerModel controller)
    {
      if (controller.ControllerType.IsGenericType)
      {
        Type dtoType = controller.ControllerType.GenericTypeArguments.FirstOrDefault(t => typeof(IDto).IsAssignableFrom(t));
        MagicalizedAttribute magicalizedAttribute = dtoType?.GetCustomAttribute<MagicalizedAttribute>();

        if (magicalizedAttribute?.Route != null)
        {
          controller.Selectors.Add(new SelectorModel
          {
            AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(magicalizedAttribute.Route)),
          });
        }
      }
    }
  }
}
