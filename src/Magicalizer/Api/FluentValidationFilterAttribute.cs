// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using System.Reflection;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Magicalizer.Api;

/// <summary>
/// Validates action arguments with the registered FluentValidation validators (<see cref="IValidator{T}"/>) before the
/// action is executed and adds validation errors to the model state. Asynchronous validation rules are supported. The
/// rule sets to validate are set by <see cref="ValidationRuleSetAttribute"/> on the parameter. It runs before
/// <see cref="ApiControllerAttribute"/> rejects the requests with the model state errors, so the client gets all the
/// errors of the request at once.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class FluentValidationFilterAttribute : ActionFilterAttribute
{
  // Parameters of the actions with their rule sets, as they don't change.
  private static readonly ConcurrentDictionary<string, ValidatedParameter[]> ParametersByActions = [];

  // Validator types (IValidator<T>) of the validated types.
  private static readonly ConcurrentDictionary<Type, Type> ValidatorTypesByTypes = [];

  /// <summary>
  /// Creates a filter. It runs before the filter of <see cref="ApiControllerAttribute"/> (-2000).
  /// </summary>
  public FluentValidationFilterAttribute()
  {
    this.Order = -2010;
  }

  /// <inheritdoc/>
  public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
  {
    foreach (ValidatedParameter parameter in GetValidatedParameters(context.ActionDescriptor))
    {
      if (!context.ActionArguments.TryGetValue(parameter.Name, out object? argument) || argument == null)
        continue;

      await ValidateAsync(context.HttpContext.RequestServices, parameter.Type, argument, parameter.RuleSets, context.ModelState, context.HttpContext.RequestAborted);
    }

    await next();
  }

  // Validates the object with its registered validator (if there is one) and adds the errors to the model state.
  // Without the rule sets, only the default rules are validated. It's used for the patched DTOs too.
  internal static async Task ValidateAsync(IServiceProvider services, Type type, object instance, string[]? ruleSets, ModelStateDictionary modelState, CancellationToken cancellationToken)
  {
    if (services.GetService(GetValidatorType(type)) is not IValidator validator) return;

    IValidationContext validationContext = ruleSets == null ?
      new ValidationContext<object>(instance) :
      new ValidationContext<object>(instance, new PropertyChain(), new RulesetValidatorSelector(ruleSets));

    ValidationResult result = await validator.ValidateAsync(validationContext, cancellationToken);

    foreach (ValidationFailure failure in result.Errors)
      modelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
  }

  // Gets the cached parameters of the action with their rule sets (the attributes are slow to read for each request).
  private static ValidatedParameter[] GetValidatedParameters(ActionDescriptor actionDescriptor)
  {
    return ParametersByActions.GetOrAdd(actionDescriptor.Id, _ => actionDescriptor.Parameters.OfType<ControllerParameterDescriptor>()
      .Select(p => new ValidatedParameter(
        p.Name,
        p.ParameterType,

        // Inherited, so the rule sets still apply when a derived controller overrides the action.
        p.ParameterInfo.GetCustomAttribute<ValidationRuleSetAttribute>(inherit: true)?.RuleSets
      ))
      .ToArray());
  }

  // Gets the cached validator type of the type (e.g. IValidator<ProductDto> of ProductDto).
  private static Type GetValidatorType(Type type)
  {
    return ValidatorTypesByTypes.GetOrAdd(type, static t => typeof(IValidator<>).MakeGenericType(t));
  }

  // Parameter of an action that is validated, with the rule sets (null if only the default rules are validated).
  private sealed record ValidatedParameter(string Name, Type Type, string[]? RuleSets);
}
