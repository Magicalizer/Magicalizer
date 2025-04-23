// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Api.Dto.Abstractions;

/// <summary>
/// Specifies authentication rules for a DTO's controller, validating only that the user is authenticated for a specific HTTP method.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AuthenticatedOnlyAttribute : Attribute
{
  /// <summary>
  /// The HTTP method to which the authentication check applies.
  /// </summary>
  public HttpMethod HttpMethod { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="AuthenticatedOnlyAttribute"/> class with the specified HTTP method.
  /// </summary>
  /// <param name="httpMethod">The HTTP method to which the authentication check applies. Defaults to <see cref="HttpMethod.Any"/>.</param>
  public AuthenticatedOnlyAttribute(HttpMethod httpMethod = HttpMethod.Any)
  {
    this.HttpMethod = httpMethod;
  }
}