// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magicalizer.Api.Controllers;

/// <summary>
/// Base controller for managing models, providing methods for DTO conversion and authorization validation.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TDto">The DTO type.</typeparam>
/// <typeparam name="TFilter">The filter type.</typeparam>
public abstract class ControllerBase<TModel, TDto, TFilter> : Controller
  where TModel : class, IModel, new()
  where TDto : class, IDto, new()
  where TFilter : class, IFilter, new()
{
  protected readonly IAuthorizationService authorizationService;

  /// <summary>
  /// Initializes a new instance of the <see cref="ControllerBase{TModel, TDto, TFilter}"/> class.
  /// </summary>
  /// <param name="authorizationService">The service for authorization checks.</param>
  public ControllerBase(IAuthorizationService authorizationService)
  {
    this.authorizationService = authorizationService;
  }

  /// <summary>
  /// Checks if the current HTTP method is supported for the given DTO.
  /// </summary>
  /// <param name="httpMethod">The HTTP method to validate.</param>
  /// <returns><c>true</c> if supported; otherwise, <c>false</c>.</returns>
  protected virtual bool ValidateHttpMethodSupport(Dto.Abstractions.HttpMethod httpMethod)
  {
    MagicalizedAttribute? magicalizedAttribute = typeof(TDto).GetCustomAttribute<MagicalizedAttribute>();

    if (magicalizedAttribute?.Methods == null || !magicalizedAttribute!.Methods!.Any()) return true;

    return magicalizedAttribute!.Methods!.Any(m => m == Dto.Abstractions.HttpMethod.Any || m == httpMethod);
  }

  /// <summary>
  /// Validates the authorization rules for the specified HTTP method.
  /// </summary>
  /// <param name="httpMethod">The HTTP method to validate.</param>
  /// <returns><c>true</c> if the user is authorized; otherwise, <c>false</c>.</returns>
  protected virtual async Task<bool> ValidateAuthorizationRulesAsync(Dto.Abstractions.HttpMethod httpMethod)
  {
    foreach (AuthorizationRuleAttribute authorizationRuleAttribute in typeof(TDto).GetCustomAttributes<AuthorizationRuleAttribute>())
    {
      if (authorizationRuleAttribute.HttpMethod == Dto.Abstractions.HttpMethod.Any || authorizationRuleAttribute.HttpMethod == httpMethod)
      {
        AuthorizationResult authorizationResult = await this.authorizationService.AuthorizeAsync(this.User, authorizationRuleAttribute.PolicyName);

        if (!authorizationResult.Succeeded)
          return false;
      }
    }

    return true;
  }

  /// <summary>
  /// Converts the given model to its corresponding DTO.
  /// </summary>
  /// <param name="model">The model to convert.</param>
  /// <returns>The converted DTO, or <c>null</c> if the model is <c>null</c>.</returns>
  protected virtual TDto? ModelToDto(TModel? model)
  {
    return model == null ? null : Activator.CreateInstance(typeof(TDto), model) as TDto;
  }
}