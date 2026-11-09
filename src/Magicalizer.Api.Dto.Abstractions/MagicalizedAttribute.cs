// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Api.Dto.Abstractions;

/// <summary>
/// Marks a DTO that has the standard REST API.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MagicalizedAttribute : Attribute
{
  /// <summary>
  /// Base route of the API (e.g. "v1/products").
  /// </summary>
  public string Route { get; }

  /// <summary>
  /// Supported HTTP methods (all if empty).
  /// </summary>
  public HttpMethod[] Methods { get; }

  /// <summary>
  /// Whether the API is hidden from API description tools (e.g. OpenAPI), as
  /// <c>ApiExplorerSettingsAttribute.IgnoreApi</c> of ASP.NET Core does for a controller (the attribute can't be set on
  /// a DTO, as DTOs don't depend on ASP.NET Core). The settings of a custom controller derived from the magicalized
  /// ones win.
  /// </summary>
  public bool IgnoreApi { get; set; }

  /// <summary>
  /// Group of the API in API description tools (e.g. the name of the OpenAPI document), as
  /// <c>ApiExplorerSettingsAttribute.GroupName</c> of ASP.NET Core. The settings of a custom controller derived from
  /// the magicalized ones win.
  /// </summary>
  public string? GroupName { get; set; }

  /// <summary>
  /// Marks a DTO that has the standard REST API with the route and HTTP methods.
  /// </summary>
  /// <param name="route">Base route of the API (e.g. "v1/products").</param>
  /// <param name="methods">Supported HTTP methods (all if empty).</param>
  public MagicalizedAttribute(string route, params HttpMethod[] methods)
  {
    this.Route = route;
    this.Methods = methods;
  }
}
