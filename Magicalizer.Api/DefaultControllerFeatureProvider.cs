// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using ExtCore.Infrastructure;
using Magicalizer.Api.Controllers;
using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Magicalizer.Api
{
  public class DefaultControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
  {
    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
      foreach (Type dtoType in ExtensionManager.GetImplementations<IDto>(useCaching: true))
      {
        Type modelType = dtoType.GetGenericInterfaceGenericArgument(typeof(IDto<>), typeof(IModel));
        Type filterType = modelType.GetGenericInterfaceGenericArgument(typeof(IModel<,>), typeof(IFilter));

        feature.Controllers.Add(
          typeof(DefaultController<,,,>).MakeGenericType(typeof(int), modelType, dtoType, filterType).GetTypeInfo()
        );
      }
    }
  }
}
