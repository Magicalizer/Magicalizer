// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using FluentValidation;
using Magicalizer.Api;
using Magicalizer.Api.ModelBinding;
using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Diagnostics;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Magicalizer.Extensions;

/// <summary>
/// Extensions of <see cref="IServiceCollection"/> to register Magicalizer services.
/// </summary>
public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Registers the services that process REST API requests.
  /// </summary>
  /// <param name="services">Services to add to.</param>
  /// <param name="configure">Configures the options of Magicalizer.</param>
  /// <returns>Services, so the calls can be chained.</returns>
  public static IServiceCollection AddMagicalizer(this IServiceCollection services, Action<MagicalizerOptions>? configure = null)
  {
    MagicalizerDiagnostics diagnostics = new MagicalizerDiagnostics();

    services.Configure(configure ?? (_ => { }));

    // The events that happen during the configuration are logged when the app starts.
    services.AddSingleton(diagnostics);
    services.AddHostedService<MagicalizerStartupLogger>();
    AddDomainServices(services, diagnostics);

    IMvcBuilder mvcBuilder = services.AddControllers();

    // Query parameters (e.g. filters) are described as they are passed (see MagicalizedApiDescriptionProvider).
    services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, MagicalizedApiDescriptionProvider>());
    mvcBuilder.AddMvcOptions(setupAction => {
      setupAction.ModelBinderProviders.Insert(0, new EnumerableFilterBinderProvider());
      setupAction.ModelBinderProviders.Insert(0, new SortingBinderProvider());
      setupAction.ModelBinderProviders.Insert(0, new InclusionBinderProvider());
      setupAction.Conventions.Add(new MagicalizedControllerRouteConvention());
      setupAction.Conventions.Add(new MagicalizedControllerHttpMethodConvention());
      setupAction.Conventions.Add(new MagicalizedControllerAuthorizationConvention());
      setupAction.Conventions.Add(new MagicalizedControllerApiExplorerConvention());
      setupAction.Conventions.Add(new MagicalizedControllerLimitConvention());
      setupAction.ModelMetadataDetailsProviders.Add(new DtoValidationMetadataProvider());
    });

    mvcBuilder.ConfigureApplicationPartManager(setupAction => {
      setupAction.FeatureProviders.Add(new MagicalizedControllerFeatureProvider(diagnostics));
    });

    services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
    return services;
  }

  // Registers domain services of all models that are mapped to entities and filters.
  private static void AddDomainServices(IServiceCollection services, MagicalizerDiagnostics diagnostics)
  {
    foreach (Type modelType in typeof(IModel).GetImplementations())
    {
      Type? entityType = modelType.GetGenericInterfaceTypeArgument(typeof(IModel<,>), typeof(IEntity));
      Type? filterType = modelType.GetGenericInterfaceTypeArgument(typeof(IModel<,>), typeof(IFilter));

      // Such models can still be used (e.g. as nested ones), they just don't have their own services.
      if (entityType == null || filterType == null)
      {
        diagnostics.Add(LogLevel.Debug, "The model {Model} doesn't have a service: it doesn't implement IModel<TEntity, TFilter>.", modelType.Name);
        continue;
      }

      Type[]? keyTypes = entityType.GetEntityKeyTypes();

      if (keyTypes == null)
      {
        diagnostics.Add(
          LogLevel.Warning,
          "The model {Model} doesn't have a service: its entity {Entity} doesn't implement IEntity<TKey>, IEntity<TKey1, TKey2> or IEntity<TKey1, TKey2, TKey3>.",
          modelType.Name, entityType.Name
        );

        continue;
      }

      RegisterDomainService(services, keyTypes, entityType, modelType, filterType, diagnostics);
    }
  }

  // Registers the default service of the model or, if there is one, its custom implementation (also for custom service
  // interfaces, so it can be injected by them too).
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
      diagnostics.Add(LogLevel.Debug, "The model {Model} is served by the default service.", modelType.Name);
      return;
    }

    diagnostics.Add(LogLevel.Debug, "The model {Model} is served by the custom service {Service}.", modelType.Name, customServiceImplementationType.Name);

    IEnumerable<Type> customServiceTypes = customServiceImplementationType.GetInterfaces().Where(i => i != genericServiceType && genericServiceType.IsAssignableFrom(i));

    foreach (Type customServiceType in customServiceTypes)
      services.AddScoped(customServiceType, customServiceImplementationType);
  }
}
