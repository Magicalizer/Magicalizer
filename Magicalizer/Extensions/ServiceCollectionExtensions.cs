// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentValidation;
using FluentValidation.AspNetCore;
using Magicalizer.Api;
using Magicalizer.Api.Dto.Abstractions;
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
    foreach (Type dtoType in typeof(IDto).GetImplementations())
    {
      Type? modelType = dtoType.GetGenericInterfaceTypeParameter(typeof(IDto<>), typeof(IModel));
      Type? entityType = modelType?.GetGenericInterfaceTypeParameter(typeof(IModel<,>), typeof(IEntity));
      Type? filterType = modelType?.GetGenericInterfaceTypeParameter(typeof(IModel<,>), typeof(IFilter));

      if (modelType == null || entityType == null || filterType == null) continue;

      if (entityType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>)))
      {
        IEnumerable<Type>? keyTypes = entityType.GetGenericInterfaceTypeParameters(typeof(IEntity<>));

        if (keyTypes?.Count() == 1)
        {
          Type? keyType = keyTypes.First();
          Type? genericServiceType = typeof(IService<,,>).MakeGenericType(keyType, modelType, filterType);
          Type? genericServiceImplementationType = typeof(Service<,,,>).MakeGenericType(keyType, entityType, modelType, filterType);
          Type? serviceImplementationType = genericServiceType.GetImplementations().FirstOrDefault();

          if (serviceImplementationType == null)
            services.AddScoped(genericServiceType, genericServiceImplementationType);

          else services.AddScoped(genericServiceType, serviceImplementationType);
        }

        else if (keyTypes?.Count() == 2)
        {
          Type? key1Type = keyTypes.First();
          Type? key2Type = keyTypes.Last();
          Type? genericServiceType = typeof(IService<,,,>).MakeGenericType(key1Type, key2Type, modelType, filterType);
          Type? genericServiceImplementationType = typeof(Service<,,,,>).MakeGenericType(key1Type, key2Type, entityType, modelType, filterType);
          Type? serviceImplementationType = genericServiceType.GetImplementations().FirstOrDefault();

          if (serviceImplementationType == null)
            services.AddScoped(genericServiceType, genericServiceImplementationType);

          else services.AddScoped(genericServiceType, serviceImplementationType);
        }

        else if (keyTypes?.Count() == 3)
        {
          Type? key1Type = keyTypes.ElementAt(0);
          Type? key2Type = keyTypes.ElementAt(1);
          Type? key3Type = keyTypes.ElementAt(2);
          Type? genericServiceType = typeof(IService<,,,,>).MakeGenericType(key1Type, key2Type, key3Type, modelType, filterType);
          Type? genericServiceImplementationType = typeof(Service<,,,,,>).MakeGenericType(key1Type, key2Type, key3Type, entityType, modelType, filterType);
          Type? serviceImplementationType = genericServiceType.GetImplementations().FirstOrDefault();

          if (serviceImplementationType == null)
            services.AddScoped(genericServiceType, genericServiceImplementationType);

          else services.AddScoped(genericServiceType, serviceImplementationType);
        }
      }
    }
  }
}