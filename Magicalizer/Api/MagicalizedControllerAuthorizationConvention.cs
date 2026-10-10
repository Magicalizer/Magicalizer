// Copyright © 2026 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Magicalizer.Api.Dto.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using HttpMethod = Magicalizer.Api.Dto.Abstractions.HttpMethod;

namespace Magicalizer.Api;

/// <summary>
/// Converts the <see cref="AuthenticatedOnlyAttribute"/> and <see cref="AuthorizedOnlyAttribute"/> attributes of a DTO
/// into the standard authorization metadata (<see cref="AuthorizeAttribute"/>) of the corresponding actions of the magicalized controllers,
/// so the requests are authenticated and authorized by the standard authorization middleware before the actions are executed.
/// The actions without the attributes are left as is (open, unless the application specifies otherwise).
/// </summary>
public class MagicalizedControllerAuthorizationConvention : IControllerModelConvention
{
  /// <summary>
  /// Adds the authorization metadata to the actions of the controller.
  /// </summary>
  /// <param name="controller">The controller model to which the convention is applied.</param>
  public void Apply(ControllerModel controller)
  {
    Type? dtoType = MagicalizedControllerConventionHelper.GetDtoType(controller.ControllerType);

    if (dtoType == null) return;

    IEnumerable<AuthenticatedOnlyAttribute> authenticatedOnlyAttributes = dtoType.GetCustomAttributes<AuthenticatedOnlyAttribute>();
    IEnumerable<AuthorizedOnlyAttribute> authorizedOnlyAttributes = dtoType.GetCustomAttributes<AuthorizedOnlyAttribute>();

    foreach (ActionModel action in controller.Actions)
    {
      HttpMethod? httpMethod = MagicalizedControllerConventionHelper.GetHttpMethod(action.ActionMethod);

      if (httpMethod == null) continue;

      List<AuthorizeAttribute> authorizeAttributes = [];

      // The default policy (requires an authenticated user unless the application specifies otherwise).
      if (authenticatedOnlyAttributes.Any(a => MagicalizedControllerConventionHelper.Matches(a.HttpMethod, httpMethod.Value)))
        authorizeAttributes.Add(new AuthorizeAttribute());

      // All the policies must be satisfied.
      foreach (AuthorizedOnlyAttribute authorizedOnlyAttribute in authorizedOnlyAttributes.Where(a => MagicalizedControllerConventionHelper.Matches(a.HttpMethod, httpMethod.Value)))
        authorizeAttributes.Add(new AuthorizeAttribute(authorizedOnlyAttribute.PolicyName));

      foreach (SelectorModel selector in action.Selectors)
        foreach (AuthorizeAttribute authorizeAttribute in authorizeAttributes)
          selector.EndpointMetadata.Add(authorizeAttribute);
    }
  }
}
