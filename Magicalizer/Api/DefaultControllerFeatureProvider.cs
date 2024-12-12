// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Magicalizer.Api.Controllers;
using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Extensions;
using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Magicalizer.Api;

/// <summary>
/// Dynamically registers controllers for DTOs that implement the <see cref="IDto"/> interface and are associated with models, entities, and filters.
/// This feature provider automatically generates controllers with appropriate generic types based on the DTO, model, and entity's composite key.
/// </summary>
public class DefaultControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
  /// <summary>
  /// Populates the MVC application's controller feature with controllers for DTOs.
  /// For each DTO that implements <see cref="IDto"/>, it identifies the corresponding model, entity, and filter types,
  /// and creates controllers with the appropriate generic parameters, including support for single or composite keys.
  /// </summary>
  /// <param name="parts">The list of application parts in the MVC application.</param>
  /// <param name="feature">The feature object that holds the controllers to be added to the application.</param>
  public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
  {
    foreach (Type dtoType in typeof(IDto).GetImplementations())
    {
      MagicalizedAttribute? magicalizedAttribute = dtoType.GetCustomAttribute<MagicalizedAttribute>();

      if (magicalizedAttribute == null) continue;

      Type? modelType = dtoType.GetGenericInterfaceTypeParameter(typeof(IDto<>), typeof(IModel));
      Type? entityType = modelType?.GetGenericInterfaceTypeParameter(typeof(IModel<,>), typeof(IEntity));
      Type? filterType = modelType?.GetGenericInterfaceTypeParameter(typeof(IModel<,>), typeof(IFilter));

      if (modelType == null || entityType == null || filterType == null) continue;

      if (entityType?.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<,,>)) == true)
      {
        IEnumerable<Type>? keyTypes = entityType.GetGenericInterfaceTypeParameters(typeof(IEntity<,,>));

        if (keyTypes == null) continue;

        feature.Controllers.Add(
          typeof(DefaultController<,,,,>).MakeGenericType(keyTypes.ElementAt(0), keyTypes.ElementAt(1), keyTypes.ElementAt(2), modelType, dtoType, filterType).GetTypeInfo()
        );
      }

      else if (entityType?.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<,>)) == true)
      {
        IEnumerable<Type>? keyTypes = entityType.GetGenericInterfaceTypeParameters(typeof(IEntity<,>));

        if (keyTypes == null) continue;

        feature.Controllers.Add(
          typeof(DefaultController<,,,,>).MakeGenericType(keyTypes.ElementAt(0), keyTypes.ElementAt(1), modelType, dtoType, filterType).GetTypeInfo()
        );
      }

      else if (entityType?.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>)) == true)
      {
        IEnumerable<Type>? keyTypes = entityType.GetGenericInterfaceTypeParameters(typeof(IEntity<>));

        if (keyTypes == null) continue;

        feature.Controllers.Add(
          typeof(DefaultController<,,,>).MakeGenericType(keyTypes.ElementAt(0), modelType, dtoType, filterType).GetTypeInfo()
        );
      }
    }
  }
}
