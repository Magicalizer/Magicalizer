// Copyright © 2021 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magicalizer.Api.Controllers
{
  public abstract class ControllerBase< TModel, TDto, TFilter> : Controller
    where TModel : class, IModel, new()
    where TDto : class, IDto, new()
    where TFilter : class, IFilter, new()
  {
    protected readonly IAuthorizationService authorizationService;
    protected readonly IServiceResolver serviceResolver;

    public ControllerBase(IAuthorizationService authorizationService, IServiceResolver serviceResolver)
    {
      this.authorizationService = authorizationService;
      this.serviceResolver = serviceResolver;
    }

    protected virtual async Task<bool> ValidateAuthorizationRules(HttpMethod httpMethod)
    {
      foreach (AuthorizationRuleAttribute authorizationRuleAttribute in typeof(TDto).GetCustomAttributes<AuthorizationRuleAttribute>())
      {
        if (authorizationRuleAttribute.HttpMethod == HttpMethod.Any || authorizationRuleAttribute.HttpMethod == httpMethod)
        {
          AuthorizationResult authorizationResult = await this.authorizationService.AuthorizeAsync(this.User, authorizationRuleAttribute.PolicyName);

          if (!authorizationResult.Succeeded)
            return false;
        }
      }

      return true;
    }

    protected virtual TDto ModelToDto(TModel model)
    {
      return Activator.CreateInstance(typeof(TDto), model) as TDto;
    }
  }
}