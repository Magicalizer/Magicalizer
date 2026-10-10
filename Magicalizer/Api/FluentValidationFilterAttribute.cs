// Copyright © 2026 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Magicalizer.Api;

/// <summary>
/// Validates the action arguments using the registered FluentValidation validators (<see cref="IValidator{T}"/>)
/// before the action is executed, and adds the validation errors to the model state.
/// Asynchronous validation rules are supported. The rule sets to execute can be specified
/// using the <see cref="ValidationRuleSetAttribute"/> on the parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class FluentValidationFilterAttribute : ActionFilterAttribute
{
  /// <inheritdoc />
  public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
  {
    foreach (ControllerParameterDescriptor parameter in context.ActionDescriptor.Parameters.OfType<ControllerParameterDescriptor>())
    {
      if (!context.ActionArguments.TryGetValue(parameter.Name, out object? argument) || argument == null)
        continue;

      Type validatorType = typeof(IValidator<>).MakeGenericType(parameter.ParameterType);

      if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
        continue;

      // Inherited, so the rule sets still apply when a derived controller overrides the action.
      string[]? ruleSets = parameter.ParameterInfo.GetCustomAttribute<ValidationRuleSetAttribute>(inherit: true)?.RuleSets;
      IValidationContext validationContext = ruleSets == null ?
        new ValidationContext<object>(argument) :
        new ValidationContext<object>(argument, new PropertyChain(), new RulesetValidatorSelector(ruleSets));

      ValidationResult result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

      foreach (ValidationFailure failure in result.Errors)
        context.ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
    }

    await next();
  }
}