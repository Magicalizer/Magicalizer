// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

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
/// Adds magicalized controllers of the DTOs marked with <see cref="MagicalizedAttribute"/>. The generic arguments of a
/// controller are the types of the DTO, its model and filter, and the primary key of its entity.
/// </summary>
internal class MagicalizedControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
  private readonly MagicalizerDiagnostics? diagnostics;

  /// <summary>
  /// Creates a provider.
  /// </summary>
  public MagicalizedControllerFeatureProvider() { }

  // Creates a provider that reports the events (e.g. why a DTO is skipped) to the diagnostics.
  internal MagicalizedControllerFeatureProvider(MagicalizerDiagnostics diagnostics)
  {
    this.diagnostics = diagnostics;
  }

  /// <inheritdoc/>
  /// <remarks>
  /// No magicalized controller is added for a DTO that is already handled by a custom controller derived from the
  /// magicalized ones.
  /// </remarks>
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

  // Gets the magicalized controller type of the DTO, or null if the DTO, its model or entity isn't set up properly (the
  // reason is reported to the diagnostics).
  private Type? GetControllerType(Type dtoType)
  {
    Type? modelType = dtoType.GetGenericInterfaceTypeArgument(typeof(IDto<>), typeof(IModel));

    if (modelType == null)
    {
      this.diagnostics?.Add(LogLevel.Warning, "The magicalized DTO {Dto} is skipped: it doesn't implement IDto<TModel>.", dtoType.Name);
      return null;
    }

    Type? entityType = modelType.GetGenericInterfaceTypeArgument(typeof(IModel<,>), typeof(IEntity));
    Type? filterType = modelType.GetGenericInterfaceTypeArgument(typeof(IModel<,>), typeof(IFilter));

    if (entityType == null || filterType == null)
    {
      this.diagnostics?.Add(LogLevel.Warning, "The magicalized DTO {Dto} is skipped: its model {Model} doesn't implement IModel<TEntity, TFilter>.", dtoType.Name, modelType.Name);
      return null;
    }

    Type[]? keyTypes = entityType.GetEntityKeyTypes();
    Type? controllerType = keyTypes == null ? null : keyTypes.Length switch {
      1 => typeof(MagicalizedController<,,,>).MakeGenericType(keyTypes[0], modelType, dtoType, filterType),
      2 => typeof(MagicalizedController<,,,,>).MakeGenericType(keyTypes[0], keyTypes[1], modelType, dtoType, filterType),
      3 => typeof(MagicalizedController<,,,,,>).MakeGenericType(keyTypes[0], keyTypes[1], keyTypes[2], modelType, dtoType, filterType),
      _ => null
    };

    if (controllerType == null)
    {
      this.diagnostics?.Add(
        LogLevel.Warning,
        "The magicalized DTO {Dto} is skipped: its entity {Entity} doesn't implement IEntity<TKey>, IEntity<TKey1, TKey2> or IEntity<TKey1, TKey2, TKey3>.",
        dtoType.Name, entityType.Name
      );

      return null;
    }

    // The controller is created anyway, but DTOs and models are created with these constructors.
    if (dtoType.GetConstructor([modelType]) == null)
    {
      this.diagnostics?.Add(
        LogLevel.Warning,
        "The magicalized DTO {Dto} doesn't have a constructor that takes its model {Model}, so the requests to its controller will fail.",
        dtoType.Name, modelType.Name
      );
    }

    if (modelType.GetConstructor([entityType]) == null)
    {
      this.diagnostics?.Add(
        LogLevel.Warning,
        "The model {Model} of the magicalized DTO {Dto} doesn't have a constructor that takes its entity {Entity}, so the requests to the controller of the DTO will fail.",
        modelType.Name, dtoType.Name, entityType.Name
      );
    }

    return controllerType;
  }
}
