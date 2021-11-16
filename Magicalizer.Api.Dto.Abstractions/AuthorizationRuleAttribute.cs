// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Magicalizer.Api.Dto.Abstractions
{
  /// <summary>
  /// Defines the HTTP methods.
  /// </summary>
  public enum HttpMethod
  {
    Any,
    Get,
    Post,
    Put,
    Patch,
    Delete
  }

  /// <summary>
  /// Indicates to the Magicalizer to validate authorization policy before executing requests with the given HTTP method.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public class AuthorizationRuleAttribute : Attribute
  {
    /// <summary>
    /// Name of the authorization policy to validate.
    /// </summary>
    public string PolicyName { get; }

    /// <summary>
    /// HTTP method the authorization policy validation should be applied to.
    /// </summary>
    public HttpMethod HttpMethod { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationRuleAttribute"/> class.
    /// </summary>
    /// <param name="policyName">Name of the authorization policy to validate.</param>
    /// <param name="httpMethod">HTTP method the authorization policy validation should be applied to.</param>
    public AuthorizationRuleAttribute(string policyName, HttpMethod httpMethod = HttpMethod.Any)
    {
      this.HttpMethod = httpMethod;
      this.PolicyName = policyName;
    }
  }
}