// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentValidation;
using Magicalizer.Api;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Diagnostics;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Filters.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Magicalizer.Extensions;

/// <summary>
/// Provides extension methods for registering the services that are required by Magicalizer.
/// </summary>
public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Registers and configures all the services that are required to process the REST API requests.
  /// </summary>
  /// <param name="services">The service collection to add the services to.</param>
  public static void AddMagicalizer(this IServiceCollection services)
  {
    MagicalizerDiagnostics diagnostics = new MagicalizerDiagnostics();

    // The events that happen during the configuration are logged when the application starts.
    services.AddSingleton(diagnostics);
    services.AddHostedService<MagicalizerStartupLogger>();
    AddDomainServices(services, diagnostics);

    IMvcBuilder mvcBuilder = services.AddControllers();

    mvcBuilder.AddMvcOptions(setupAction => {
      setupAction.ModelBinderProviders.Insert(0, new EnumerableFilterBinderProvider());
      setupAction.Conventions.Add(new MagicalizedControllerRouteConvention());
      setupAction.Conventions.Add(new MagicalizedControllerHttpMethodConvention());
      setupAction.Conventions.Add(new MagicalizedControllerAuthorizationConvention());
      setupAction.ModelMetadataDetailsProviders.Add(new DtoValidationMetadataProvider());
    });

    mvcBuilder.ConfigureApplicationPartManager(setupAction => {
      setupAction.FeatureProviders.Add(new MagicalizedControllerFeatureProvider(diagnostics));
    });

    services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
  }

  // Registers the domain services for all the models that are mapped to entities and filters.
  private static void AddDomainServices(IServiceCollection services, MagicalizerDiagnostics diagnostics)
  {
    foreach (Type modelType in typeof(IModel).GetImplementations())
    {
      Type? entityType = modelType.GetGenericInterfaceTypeParameter(typeof(IModel<,>), typeof(IEntity));
      Type? filterType = modelType.GetGenericInterfaceTypeParameter(typeof(IModel<,>), typeof(IFilter));

      // Such models can still be used (e.g. as nested ones), they just don't have their own services.
      if (entityType == null || filterType == null)
      {
        diagnostics.Add(LogLevel.Debug, "Model {Model} doesn't have a service: it doesn't implement IModel<TEntity, TFilter>", modelType.Name);
        continue;
      }

      Type[]? keyTypes = entityType.GetEntityKeyTypes();

      if (keyTypes == null)
      {
        diagnostics.Add(
          LogLevel.Warning,
          "Model {Model} doesn't have a service: its entity {Entity} doesn't implement IEntity<TKey>, IEntity<TKey1, TKey2>, or IEntity<TKey1, TKey2, TKey3>",
          modelType.Name, entityType.Name
        );

        continue;
      }

      RegisterDomainService(services, keyTypes, entityType, modelType, filterType, diagnostics);
    }
  }

  // Registers the default service for the model or, if there is one, its custom implementation
  // (also for the custom service interfaces, so it can be injected by them too).
  private static void RegisterDomainService(IServiceCollection services, Type[] keyTypes, Type entityType, Type modelType, Type filterType, MagicalizerDiagnostics diagnostics)
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

    Type? customServiceImplementationType = genericServiceType.GetImplementations().FirstOrDefault();

    services.AddScoped(genericServiceType, customServiceImplementationType ?? genericServiceImplementationType);

    if (customServiceImplementationType == null)
    {
      diagnostics.Add(LogLevel.Debug, "Model {Model} is served by the default service", modelType.Name);
      return;
    }

    diagnostics.Add(LogLevel.Debug, "Model {Model} is served by the custom service {Service}", modelType.Name, customServiceImplementationType.Name);

    IEnumerable<Type> customServiceTypes = customServiceImplementationType.GetInterfaces().Where(i => i != genericServiceType && genericServiceType.IsAssignableFrom(i));

    foreach (Type customServiceType in customServiceTypes)
      services.AddScoped(customServiceType, customServiceImplementationType);
  }
}
