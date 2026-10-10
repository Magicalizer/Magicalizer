// Copyright © 2026 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Magicalizer.Diagnostics;

/// <summary>
/// Logs the events collected while Magicalizer was being configured and describes the resulting controllers
/// of the magicalized DTOs (their routes, HTTP methods, and authorization rules) when the application starts.
/// </summary>
internal sealed class MagicalizerStartupLogger : IHostedService
{
  private static readonly string[] httpMethodsOrder = ["GET", "POST", "PUT", "PATCH", "DELETE"];
  private readonly MagicalizerDiagnostics diagnostics;
  private readonly IActionDescriptorCollectionProvider actionDescriptorCollectionProvider;
  private readonly IServiceProvider serviceProvider;
  private readonly ILogger logger;

  /// <summary>
  /// Initializes a new instance of the <see cref="MagicalizerStartupLogger"/> class.
  /// </summary>
  /// <param name="diagnostics">The events collected while Magicalizer was being configured.</param>
  /// <param name="actionDescriptorCollectionProvider">The provider of the actions of all the controllers.</param>
  /// <param name="serviceProvider">The service provider to get the optional services (e.g. the authorization policy provider).</param>
  /// <param name="loggerFactory">The factory to create the logger.</param>
  public MagicalizerStartupLogger(MagicalizerDiagnostics diagnostics, IActionDescriptorCollectionProvider actionDescriptorCollectionProvider, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
  {
    this.diagnostics = diagnostics;
    this.actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
    this.serviceProvider = serviceProvider;
    this.logger = loggerFactory.CreateLogger("Magicalizer");
  }

  /// <summary>
  /// Logs the collected events and describes the controllers of the magicalized DTOs.
  /// </summary>
  /// <param name="cancellationToken">The token to cancel the operation.</param>
  public async Task StartAsync(CancellationToken cancellationToken)
  {
    // Getting the actions builds the application model, so the controllers are discovered and the conventions are applied
    // (and their events are collected) when the application starts rather than when the first request is processed.
    List<ControllerActionDescriptor> actions = this.actionDescriptorCollectionProvider.ActionDescriptors.Items
      .OfType<ControllerActionDescriptor>()
      .Where(a => MagicalizedControllerConventionHelper.GetDtoType(a.ControllerTypeInfo) != null)
      .ToList();

    foreach (MagicalizerDiagnostics.Entry entry in this.diagnostics.Entries)
      this.logger.Log(entry.Level, entry.Message, entry.Args);

    IAuthorizationPolicyProvider? authorizationPolicyProvider = this.serviceProvider.GetService<IAuthorizationPolicyProvider>();
    IEnumerable<IGrouping<Type, ControllerActionDescriptor>> actionsByControllers = actions.GroupBy(a => a.ControllerTypeInfo.AsType());

    foreach (IGrouping<Type, ControllerActionDescriptor> controllerActions in actionsByControllers)
    {
      Type dtoType = MagicalizedControllerConventionHelper.GetDtoType(controllerActions.Key)!;
      ControllerActionDescriptor firstAction = controllerActions.First();
      string controllerName = controllerActions.Key.IsGenericType ? firstAction.ControllerName : controllerActions.Key.Name;
      List<string> httpMethods = [];

      foreach (IGrouping<string, ControllerActionDescriptor> httpMethodActions in controllerActions.GroupBy(GetHttpMethod).OrderBy(g => GetHttpMethodPosition(g.Key)))
      {
        IEnumerable<string> rules = httpMethodActions.SelectMany(GetAuthorizationRules).Distinct();

        httpMethods.Add(rules.Any() ? $"{httpMethodActions.Key} ({string.Join(", ", rules)})" : httpMethodActions.Key);
        await this.ValidatePoliciesAsync(authorizationPolicyProvider, dtoType, httpMethodActions.Key, httpMethodActions);
      }

      this.logger.LogInformation(
        "Magicalized DTO {Dto} is served by {Controller} at {Route}: {HttpMethods}",
        dtoType.Name, controllerName, GetRoute(controllerActions), string.Join(", ", httpMethods)
      );
    }

    this.logger.LogInformation("{Count} controllers are created for the magicalized DTOs", actionsByControllers.Count());
  }

  /// <summary>
  /// Does nothing, as there is nothing to stop.
  /// </summary>
  /// <param name="cancellationToken">The token to cancel the operation.</param>
  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

  // Reports the authorization policies required by the actions that are not registered (the requests to such actions fail).
  private async Task ValidatePoliciesAsync(IAuthorizationPolicyProvider? authorizationPolicyProvider, Type dtoType, string httpMethod, IEnumerable<ControllerActionDescriptor> actions)
  {
    IEnumerable<string> policyNames = actions.SelectMany(a => a.EndpointMetadata.OfType<IAuthorizeData>()).Select(d => d.Policy).OfType<string>().Distinct();

    foreach (string policyName in policyNames)
    {
      if (authorizationPolicyProvider == null)
      {
        this.logger.LogWarning(
          "Magicalized DTO {Dto} requires the policy {Policy} for {HttpMethod}, but the authorization services are not registered, so the requests will fail",
          dtoType.Name, policyName, httpMethod
        );
      }

      else if (await authorizationPolicyProvider.GetPolicyAsync(policyName) == null)
      {
        this.logger.LogWarning(
          "Magicalized DTO {Dto} requires the policy {Policy} for {HttpMethod}, but it is not registered, so the requests will fail",
          dtoType.Name, policyName, httpMethod
        );
      }
    }
  }

  // Gets the HTTP method of an action (e.g. "GET").
  private static string GetHttpMethod(ControllerActionDescriptor action)
  {
    return action.ActionConstraints?.OfType<HttpMethodActionConstraint>().SelectMany(c => c.HttpMethods).FirstOrDefault() ?? "ANY";
  }

  // Gets the position of an HTTP method, so the methods are always listed in the same order.
  private static int GetHttpMethodPosition(string httpMethod)
  {
    int index = Array.IndexOf(httpMethodsOrder, httpMethod);

    return index == -1 ? httpMethodsOrder.Length : index;
  }

  // Gets the authorization rules of an action (e.g. "policy User.Get" or "authenticated").
  private static IEnumerable<string> GetAuthorizationRules(ControllerActionDescriptor action)
  {
    if (action.EndpointMetadata.OfType<IAllowAnonymous>().Any())
      return ["anonymous"];

    return action.EndpointMetadata.OfType<IAuthorizeData>().Select(d => d.Policy == null ? "authenticated" : $"policy {d.Policy}");
  }

  // Gets the base route of a controller (the shortest route template of its actions, e.g. "/v1/products").
  private static string GetRoute(IEnumerable<ControllerActionDescriptor> actions)
  {
    string? template = actions.Select(a => a.AttributeRouteInfo?.Template).OfType<string>().OrderBy(t => t.Length).FirstOrDefault();

    return template == null ? "(no route)" : "/" + template.TrimStart('/');
  }
}
