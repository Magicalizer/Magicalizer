// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Api.Dto.Abstractions;

/// <summary>
/// Authorization rule of a DTO controller: the user has to satisfy the policy to call the API methods.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AuthorizedOnlyAttribute : Attribute
{
  /// <summary>
  /// Name of the authorization policy.
  /// </summary>
  public string PolicyName { get; }

  /// <summary>
  /// HTTP method the rule applies to.
  /// </summary>
  public HttpMethod HttpMethod { get; }

  /// <summary>
  /// Creates an authorization rule for the policy and HTTP method.
  /// </summary>
  /// <param name="policyName">Name of the authorization policy.</param>
  /// <param name="httpMethod">HTTP method the rule applies to (any by default).</param>
  public AuthorizedOnlyAttribute(string policyName, HttpMethod httpMethod = HttpMethod.Any)
  {
    this.PolicyName = policyName;
    this.HttpMethod = httpMethod;
  }
}
