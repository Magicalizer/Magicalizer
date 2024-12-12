// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentValidation;
using FluentValidation.AspNetCore;
using Magicalizer.Api;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Filters.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer.Extensions;

public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Registers and configures all the services that are required to process the REST API requests.
  /// </summary>
  public static void AddMagicalizer(this IServiceCollection services)
  {
    AddDomainServices(services);

    IMvcBuilder mvcBuilder = services.AddControllers();

    mvcBuilder.AddMvcOptions(setupAction => {
      setupAction.Conventions.Add(new DefaultControllerRouteConvention());
    });

    mvcBuilder.ConfigureApplicationPartManager(setupAction => {
      setupAction.FeatureProviders.Add(new DefaultControllerFeatureProvider());
    });

    mvcBuilder.AddNewtonsoftJson();

    services.AddFluentValidationAutoValidation(config => {
      config.DisableDataAnnotationsValidation = true;
    });

    services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
  }

  private static void AddDomainServices(IServiceCollection services)
  {
    foreach (Type modelType in typeof(IModel).GetImplementations())
    {
      Type? entityType = modelType.GetGenericInterfaceTypeParameter(typeof(IModel<,>), typeof(IEntity));
      Type? filterType = modelType.GetGenericInterfaceTypeParameter(typeof(IModel<,>), typeof(IFilter));

      if (modelType == null || entityType == null || filterType == null) continue;

      if (entityType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>)))
      {
        Type[] keyTypes = GetKeyTypes(entityType);

        if (keyTypes.Length == 0) continue;

        RegisterDomainService(services, keyTypes, entityType, modelType, filterType);
      }
    }
  }

  private static Type[] GetKeyTypes(Type entityType)
  {
    return entityType.GetInterfaces().FirstOrDefault(
      i => i.IsGenericType &&
        (i.GetGenericTypeDefinition() == typeof(IEntity<>) || i.GetGenericTypeDefinition() == typeof(IEntity<,>) || i.GetGenericTypeDefinition() == typeof(IEntity<,,>))
    )?.GetGenericArguments() ?? [];
  }

  private static void RegisterDomainService(IServiceCollection services, Type[] keyTypes, Type entityType, Type modelType, Type filterType)
  {
    Type genericServiceType;
    Type genericServiceImplementationType;

    switch (keyTypes.Length)
    {
      case 1:
        genericServiceType = typeof(IService<,,>).MakeGenericType(keyTypes[0], modelType, filterType);
        genericServiceImplementationType = typeof(Service<,,,>).MakeGenericType(keyTypes[0], entityType, modelType, filterType);
        break;

      case 2:
        genericServiceType = typeof(IService<,,,>).MakeGenericType(keyTypes[0], keyTypes[1], modelType, filterType);
        genericServiceImplementationType = typeof(Service<,,,,>).MakeGenericType(keyTypes[0], keyTypes[1], entityType, modelType, filterType);
        break;

      case 3:
        genericServiceType = typeof(IService<,,,,>).MakeGenericType(keyTypes[0], keyTypes[1], keyTypes[2], modelType, filterType);
        genericServiceImplementationType = typeof(Service<,,,,,>).MakeGenericType(keyTypes[0], keyTypes[1], keyTypes[2], entityType, modelType, filterType);
        break;

      default:
        return;
    }

    Type? serviceImplementationType = genericServiceType.GetImplementations().FirstOrDefault();

    services.AddScoped(genericServiceType, serviceImplementationType ?? genericServiceImplementationType);
  }
}