// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Magicalizer.Diagnostics;

/// <summary>
/// Logs events collected while Magicalizer was being configured and describes the resulting controllers of the
/// magicalized DTOs (their routes, HTTP methods and authorization rules) when the app starts.
/// </summary>
internal sealed class MagicalizerStartupLogger : IHostedService
{
  private static readonly string[] HttpMethodsOrder = ["GET", "POST", "PUT", "PATCH", "DELETE"];
  private readonly MagicalizerDiagnostics diagnostics;
  private readonly IActionDescriptorCollectionProvider actionDescriptorCollectionProvider;
  private readonly IAuthorizationPolicyProvider authorizationPolicyProvider;
  private readonly ILogger logger;

  /// <summary>
  /// Creates a logger.
  /// </summary>
  /// <param name="diagnostics">Events collected while Magicalizer was being configured.</param>
  /// <param name="actionDescriptorCollectionProvider">Provider of actions of all controllers.</param>
  /// <param name="authorizationPolicyProvider">
  /// Provider of the authorization policies (the controllers register it), to check the required ones.
  /// </param>
  /// <param name="loggerFactory">Factory of the logger.</param>
  public MagicalizerStartupLogger(MagicalizerDiagnostics diagnostics, IActionDescriptorCollectionProvider actionDescriptorCollectionProvider, IAuthorizationPolicyProvider authorizationPolicyProvider, ILoggerFactory loggerFactory)
  {
    this.diagnostics = diagnostics;
    this.actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
    this.authorizationPolicyProvider = authorizationPolicyProvider;
    this.logger = loggerFactory.CreateLogger("Magicalizer");
  }

  /// <inheritdoc/>
  /// <remarks>
  /// It logs the collected events and describes controllers of the magicalized DTOs.
  /// </remarks>
  public async Task StartAsync(CancellationToken cancellationToken)
  {
    // Getting the actions builds the application model, so controllers are discovered and conventions are applied (and
    // their events are collected) when the app starts rather than when the first request is processed.
    List<IGrouping<Type, ControllerActionDescriptor>> actionsByControllers = this.actionDescriptorCollectionProvider.ActionDescriptors.Items
      .OfType<ControllerActionDescriptor>()
      .Where(a => MagicalizedControllerConventionHelper.GetDtoType(a.ControllerTypeInfo) != null)
      .GroupBy(a => a.ControllerTypeInfo.AsType())
      .ToList();

    this.diagnostics.StartLogging(this.logger);

    foreach (IGrouping<Type, ControllerActionDescriptor> controllerActions in actionsByControllers)
    {
      Type dtoType = MagicalizedControllerConventionHelper.GetDtoType(controllerActions.Key)!;
      ControllerActionDescriptor firstAction = controllerActions.First();
      string controllerName = controllerActions.Key.IsGenericType ? firstAction.ControllerName : controllerActions.Key.Name;
      List<string> httpMethods = [];

      foreach (IGrouping<string, ControllerActionDescriptor> httpMethodActions in controllerActions.GroupBy(GetHttpMethod).OrderBy(g => GetHttpMethodPosition(g.Key)))
      {
        List<string> rules = httpMethodActions.SelectMany(GetAuthorizationRules).Distinct().ToList();

        httpMethods.Add(rules.Count != 0 ? $"{httpMethodActions.Key} ({string.Join(", ", rules)})" : httpMethodActions.Key);
        await this.LogUnregisteredPoliciesAsync(dtoType, httpMethodActions.Key, httpMethodActions);
      }

      this.logger.LogInformation(
        "The magicalized DTO {Dto} is served by {Controller} at {Route}: {HttpMethods}.",
        dtoType.Name, controllerName, GetRoute(controllerActions), string.Join(", ", httpMethods)
      );
    }

    this.logger.LogInformation("{Count} controllers are created for the magicalized DTOs.", actionsByControllers.Count);
  }

  /// <inheritdoc/>
  /// <remarks>
  /// It does nothing, as there is nothing to stop.
  /// </remarks>
  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

  // Gets the HTTP method of an action (e.g. "GET").
  private static string GetHttpMethod(ControllerActionDescriptor action)
  {
    return action.ActionConstraints?.OfType<HttpMethodActionConstraint>().SelectMany(c => c.HttpMethods).FirstOrDefault() ?? "ANY";
  }

  // Gets the position of an HTTP method, so methods are always listed in the same order.
  private static int GetHttpMethodPosition(string httpMethod)
  {
    int index = Array.IndexOf(HttpMethodsOrder, httpMethod);

    return index == -1 ? HttpMethodsOrder.Length : index;
  }

  // Gets the authorization rules of an action (e.g. "policy User.Get" or "authenticated").
  private static IEnumerable<string> GetAuthorizationRules(ControllerActionDescriptor action)
  {
    if (action.EndpointMetadata.OfType<IAllowAnonymous>().Any())
      return ["anonymous"];

    return action.EndpointMetadata.OfType<IAuthorizeData>().Select(d => d.Policy == null ? "authenticated" : $"policy {d.Policy}");
  }

  // Logs the authorization policies that are required by the actions but aren't registered (requests to such actions fail).
  private async Task LogUnregisteredPoliciesAsync(Type dtoType, string httpMethod, IEnumerable<ControllerActionDescriptor> actions)
  {
    IEnumerable<string> policyNames = actions.SelectMany(a => a.EndpointMetadata.OfType<IAuthorizeData>()).Select(d => d.Policy).OfType<string>().Distinct();

    foreach (string policyName in policyNames)
    {
      if (await this.authorizationPolicyProvider.GetPolicyAsync(policyName) == null)
      {
        this.logger.LogWarning(
          "The magicalized DTO {Dto} requires the policy {Policy} for {HttpMethod}, but it isn't registered, so the requests will fail.",
          dtoType.Name, policyName, httpMethod
        );
      }
    }
  }

  // Gets the base route of a controller (the shortest route template of its actions, e.g. "/v1/products").
  private static string GetRoute(IEnumerable<ControllerActionDescriptor> actions)
  {
    string? template = actions.Select(a => a.AttributeRouteInfo?.Template).OfType<string>().OrderBy(t => t.Length).FirstOrDefault();

    return template == null ? "(no route)" : "/" + template.TrimStart('/');
  }
}
