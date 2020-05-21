// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Magicalizer.Api.Dto.Abstractions
{
  public enum HttpMethod
  {
    Any,
    Get,
    Post,
    Put,
    Patch,
    Delete
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class AuthorizationRuleAttribute : Attribute
  {
    public string PolicyName { get; }
    public HttpMethod HttpMethod { get; }
    
    public AuthorizationRuleAttribute(string policyName, HttpMethod httpMethod = HttpMethod.Any)
    {
      this.HttpMethod = httpMethod;
      this.PolicyName = policyName;
    }
  }
}