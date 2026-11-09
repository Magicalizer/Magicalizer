// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using FluentValidation;
using FluentValidation.Results;
using Magicalizer.Domain.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer.Api;

/// <summary>
/// Converts the exceptions of the model services to the responses:
/// <list type="bullet">
///   <item><see cref="ValidationException"/> to 400 Bad Request with the validation errors.</item>
///   <item><see cref="ModelNotFoundException"/> to 404 Not Found.</item>
///   <item><see cref="ModelAlreadyExistsException"/> and <see cref="ModelInUseException"/> to 409 Conflict.</item>
/// </list>
/// Other exceptions aren't handled.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ModelExceptionFilterAttribute : ExceptionFilterAttribute
{
  /// <inheritdoc/>
  public override void OnException(ExceptionContext context)
  {
    ProblemDetailsFactory problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

    if (context.Exception is ValidationException validationException)
    {
      foreach (ValidationFailure failure in validationException.Errors)
        context.ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);

      // The exception may be thrown without failures (e.g. with a message only).
      if (context.ModelState.IsValid)
        context.ModelState.AddModelError(string.Empty, validationException.Message);

      ValidationProblemDetails validationProblemDetails = problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);

      context.Result = new ObjectResult(validationProblemDetails) { StatusCode = validationProblemDetails.Status };
      context.ExceptionHandled = true;
      return;
    }

    int? statusCode = context.Exception switch {
      ModelNotFoundException => StatusCodes.Status404NotFound,
      ModelAlreadyExistsException or ModelInUseException => StatusCodes.Status409Conflict,
      _ => null
    };

    if (statusCode == null) return;

    ProblemDetails problemDetails = problemDetailsFactory.CreateProblemDetails(context.HttpContext, statusCode, detail: context.Exception.Message);

    context.Result = new ObjectResult(problemDetails) { StatusCode = statusCode };
    context.ExceptionHandled = true;
  }
}
