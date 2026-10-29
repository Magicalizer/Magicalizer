// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Api.Dto.Abstractions;

/// <summary>
/// Indicates that the standard REST API methods are supported for a DTO.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MagicalizedAttribute : Attribute
{
  /// <summary>
  /// The base route for the REST API (e.g., "v1/products").
  /// </summary>
  public string Route { get; }

  /// <summary>
  /// The supported HTTP methods.
  /// </summary>
  public HttpMethod[] Methods { get; }

  /// <summary>
  /// Whether the API is hidden from the API description tools (e.g. OpenAPI), as <c>ApiExplorerSettingsAttribute.IgnoreApi</c>
  /// of ASP.NET Core does for a controller (the attribute can't be set on a DTO, as the DTOs don't depend on ASP.NET Core).
  /// The settings of a custom controller derived from the magicalized ones win.
  /// </summary>
  public bool IgnoreApi { get; set; }

  /// <summary>
  /// The group of the API in the API description tools (e.g. the name of the OpenAPI document), as
  /// <c>ApiExplorerSettingsAttribute.GroupName</c> of ASP.NET Core. The settings of a custom controller derived from the
  /// magicalized ones win.
  /// </summary>
  public string? GroupName { get; set; }

  /// <summary>
  /// Initializes a new instance of the <see cref="MagicalizedAttribute"/> class with the specified API route and supported HTTP methods.
  /// </summary>
  /// <param name="route">The base route for the REST API (e.g., "v1/products").</param>
  /// <param name="methods">The supported HTTP methods.</param>
  public MagicalizedAttribute(string route, params HttpMethod[] methods)
  {
    this.Route = route;
    this.Methods = methods;
  }
}
