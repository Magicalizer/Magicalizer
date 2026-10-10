// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Magicalizer.Api.Controllers;
using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Diagnostics;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Extensions;
using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;

namespace Magicalizer.Api;

/// <summary>
/// Dynamically registers controllers for DTOs that implement the <see cref="IDto"/> interface and are associated with models, entities, and filters.
/// This feature provider automatically generates controllers with appropriate generic types based on the DTO, model, and entity's composite key.
/// </summary>
public class MagicalizedControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
  private readonly MagicalizerDiagnostics? diagnostics;

  /// <summary>
  /// Initializes a new instance of the <see cref="MagicalizedControllerFeatureProvider"/> class.
  /// </summary>
  public MagicalizedControllerFeatureProvider() { }

  // Initializes a new instance of the class that reports the events (e.g. why a DTO is skipped) to the diagnostics.
  internal MagicalizedControllerFeatureProvider(MagicalizerDiagnostics diagnostics)
  {
    this.diagnostics = diagnostics;
  }

  /// <summary>
  /// Populates the MVC application's controller feature with controllers for DTOs.
  /// For each DTO that implements <see cref="IDto"/>, it identifies the corresponding model, entity, and filter types,
  /// and creates controllers with the appropriate generic parameters, including support for single or composite keys.
  /// No magicalized controller is created for a DTO that is already handled by a custom controller derived from the magicalized ones.
  /// </summary>
  /// <param name="parts">The list of application parts in the MVC application.</param>
  /// <param name="feature">The feature object that holds the controllers to be added to the application.</param>
  public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
  {
    foreach (Type dtoType in typeof(IDto).GetImplementations())
    {
      if (dtoType.GetCustomAttribute<MagicalizedAttribute>() == null) continue;

      // A custom controller (derived from the magicalized ones) already handles the DTO.
      if (feature.Controllers.Any(c => MagicalizedControllerConventionHelper.GetDtoType(c) == dtoType)) continue;

      Type? controllerType = this.GetControllerType(dtoType);

      if (controllerType != null)
        feature.Controllers.Add(controllerType.GetTypeInfo());
    }
  }

  // Gets the magicalized controller type for the DTO, or null if the DTO, its model, or its entity are not configured properly
  // (the reason is reported to the diagnostics).
  private Type? GetControllerType(Type dtoType)
  {
    Type? modelType = dtoType.GetGenericInterfaceTypeParameter(typeof(IDto<>), typeof(IModel));

    if (modelType == null)
    {
      this.diagnostics?.Add(LogLevel.Warning, "Magicalized DTO {Dto} is skipped: it doesn't implement IDto<TModel>", dtoType.Name);
      return null;
    }

    Type? entityType = modelType.GetGenericInterfaceTypeParameter(typeof(IModel<,>), typeof(IEntity));
    Type? filterType = modelType.GetGenericInterfaceTypeParameter(typeof(IModel<,>), typeof(IFilter));

    if (entityType == null || filterType == null)
    {
      this.diagnostics?.Add(LogLevel.Warning, "Magicalized DTO {Dto} is skipped: its model {Model} doesn't implement IModel<TEntity, TFilter>", dtoType.Name, modelType.Name);
      return null;
    }

    Type[]? keyTypes = entityType.GetEntityKeyTypes();
    Type? controllerType = keyTypes == null ? null : keyTypes.Length switch
    {
      1 => typeof(MagicalizedController<,,,>).MakeGenericType(keyTypes[0], modelType, dtoType, filterType),
      2 => typeof(MagicalizedController<,,,,>).MakeGenericType(keyTypes[0], keyTypes[1], modelType, dtoType, filterType),
      3 => typeof(MagicalizedController<,,,,,>).MakeGenericType(keyTypes[0], keyTypes[1], keyTypes[2], modelType, dtoType, filterType),
      _ => null
    };

    if (controllerType == null)
    {
      this.diagnostics?.Add(
        LogLevel.Warning,
        "Magicalized DTO {Dto} is skipped: its entity {Entity} doesn't implement IEntity<TKey>, IEntity<TKey1, TKey2>, or IEntity<TKey1, TKey2, TKey3>",
        dtoType.Name, entityType.Name
      );

      return null;
    }

    // The controller is created anyway, but the DTOs and the models are created using these constructors.
    if (dtoType.GetConstructor([modelType]) == null)
    {
      this.diagnostics?.Add(
        LogLevel.Warning,
        "Magicalized DTO {Dto} doesn't have a constructor that accepts its model {Model}, so the requests to its controller will fail",
        dtoType.Name, modelType.Name
      );
    }

    if (modelType.GetConstructor([entityType]) == null)
    {
      this.diagnostics?.Add(
        LogLevel.Warning,
        "Model {Model} of the magicalized DTO {Dto} doesn't have a constructor that accepts its entity {Entity}, so the requests to the DTO's controller will fail",
        modelType.Name, dtoType.Name, entityType.Name
      );
    }

    return controllerType;
  }
}
