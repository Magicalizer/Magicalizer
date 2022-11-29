// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExtCore.Infrastructure;
using Magicalizer.Api.Controllers;
using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.Data.Entities.Abstractions;
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
        Type modelType = dtoType.GetGenericInterfaceTypeParameter(typeof(IDto<>), typeof(IModel));
        Type entityType = modelType.GetGenericInterfaceTypeParameter(typeof(IModel<,>), typeof(IEntity));
        Type filterType = modelType.GetGenericInterfaceTypeParameter(typeof(IModel<,>), typeof(IFilter));

        if (entityType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>)))
        {
          IEnumerable<Type> keyTypes = entityType.GetGenericInterfaceTypeParameters(typeof(IEntity<>));
          
          feature.Controllers.Add(
            typeof(DefaultController<,,,>).MakeGenericType(keyTypes.First(), modelType, dtoType, filterType).GetTypeInfo()
          );
        }

        else if (entityType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<,>)))
        {
          IEnumerable<Type> keyTypes = entityType.GetGenericInterfaceTypeParameters(typeof(IEntity<,>));

          feature.Controllers.Add(
            typeof(DefaultController<,,,,>).MakeGenericType(keyTypes.ElementAt(0), keyTypes.ElementAt(1), modelType, dtoType, filterType).GetTypeInfo()
          );
        }

        else if (entityType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<,,>)))
        {
          IEnumerable<Type> keyTypes = entityType.GetGenericInterfaceTypeParameters(typeof(IEntity<,,>));

          feature.Controllers.Add(
            typeof(DefaultController<,,,,>).MakeGenericType(keyTypes.ElementAt(0), keyTypes.ElementAt(1), keyTypes.ElementAt(2), modelType, dtoType, filterType).GetTypeInfo()
          );
        }
      }
    }
  }
}
