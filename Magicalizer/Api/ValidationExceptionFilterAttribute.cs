// Copyright © 2026 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer.Api;

/// <summary>
/// Converts the <see cref="ValidationException"/> thrown by the services (when a model is not valid)
/// into a 400 Bad Request response with the standard validation problem details, the same as the invalid DTOs are reported.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ValidationExceptionFilterAttribute : ExceptionFilterAttribute
{
  /// <inheritdoc />
  public override void OnException(ExceptionContext context)
  {
    if (context.Exception is not ValidationException validationException) return;

    foreach (ValidationFailure failure in validationException.Errors)
      context.ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);

    // The exception may be thrown without the failures (e.g. with a message only).
    if (context.ModelState.IsValid)
      context.ModelState.AddModelError(string.Empty, validationException.Message);

    context.Result = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>().CreateValidationProblemResult(context.HttpContext, context.ModelState);
    context.ExceptionHandled = true;
  }
}
