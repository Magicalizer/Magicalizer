// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
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
