// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Magicalizer.Api.Dto.Abstractions
{
  /// <summary>
  /// Indicates to the Magicalizer that the standard REST API methods should be supported for the DTO.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public class MagicalizedAttribute : Attribute
  {
    /// <summary>
    /// The REST API base relative URL segment (example: "v1/products").
    /// </summary>
    public string Route { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MagicalizedAttribute"/> class.
    /// </summary>
    /// <param name="route">The REST API base relative URL segment (example: "v1/products").</param>
    public MagicalizedAttribute(string route)
    {
      this.Route = route;
    }
  }
}