// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Api.Dto.Abstractions;

/// <summary>
/// Authentication rule of a DTO controller: the user only has to be authenticated to call the API methods.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AuthenticatedOnlyAttribute : Attribute
{
  /// <summary>
  /// HTTP method the rule applies to.
  /// </summary>
  public HttpMethod HttpMethod { get; }

  /// <summary>
  /// Creates an authentication rule for the HTTP method.
  /// </summary>
  /// <param name="httpMethod">HTTP method the rule applies to (any by default).</param>
  public AuthenticatedOnlyAttribute(HttpMethod httpMethod = HttpMethod.Any)
  {
    this.HttpMethod = httpMethod;
  }
}
