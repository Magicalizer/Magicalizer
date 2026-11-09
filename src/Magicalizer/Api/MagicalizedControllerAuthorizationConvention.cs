// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;
using Magicalizer.Api.Dto.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using HttpMethod = Magicalizer.Api.Dto.Abstractions.HttpMethod;

namespace Magicalizer.Api;

/// <summary>
/// Converts <see cref="AuthenticatedOnlyAttribute"/> and <see cref="AuthorizedOnlyAttribute"/> of a DTO to standard
/// authorization metadata (<see cref="AuthorizeAttribute"/>) of the magicalized controller actions, so requests are
/// authenticated and authorized by the standard authorization middleware before the actions are executed. Actions
/// without the attributes are left as they are (open, unless the app sets otherwise).
/// </summary>
internal class MagicalizedControllerAuthorizationConvention : IControllerModelConvention
{
  /// <inheritdoc/>
  /// <remarks>
  /// It adds authorization metadata to the controller actions.
  /// </remarks>
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

      // The default policy (requires an authenticated user unless the app sets otherwise).
      if (authenticatedOnlyAttributes.Any(a => MagicalizedControllerConventionHelper.Matches(a.HttpMethod, httpMethod.Value)))
        authorizeAttributes.Add(new AuthorizeAttribute());

      // All policies must be satisfied.
      foreach (AuthorizedOnlyAttribute authorizedOnlyAttribute in authorizedOnlyAttributes.Where(a => MagicalizedControllerConventionHelper.Matches(a.HttpMethod, httpMethod.Value)))
        authorizeAttributes.Add(new AuthorizeAttribute(authorizedOnlyAttribute.PolicyName));

      foreach (SelectorModel selector in action.Selectors)
      {
        foreach (AuthorizeAttribute authorizeAttribute in authorizeAttributes)
          selector.EndpointMetadata.Add(authorizeAttribute);
      }
    }
  }
}
