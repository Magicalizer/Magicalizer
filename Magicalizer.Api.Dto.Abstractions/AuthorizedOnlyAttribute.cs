// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Api.Dto.Abstractions;

/// <summary>
/// Specifies authorization rules for a DTO's controller, validating against a specific policy and HTTP method.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AuthorizedOnlyAttribute : Attribute
{
  /// <summary>
  /// The authorization policy to validate against.
  /// </summary>
  public string PolicyName { get; }

  /// <summary>
  /// The HTTP method to which the policy applies.
  /// </summary>
  public HttpMethod HttpMethod { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="AuthorizedOnlyAttribute"/> class with the specified policy and HTTP method.
  /// </summary>
  /// <param name="policyName">The authorization policy to validate against.</param>
  /// <param name="httpMethod">The HTTP method to which the policy applies. Defaults to <see cref="HttpMethod.Any"/>.</param>
  public AuthorizedOnlyAttribute(string policyName, HttpMethod httpMethod = HttpMethod.Any)
  {
    this.PolicyName = policyName;
    this.HttpMethod = httpMethod;
  }
}
