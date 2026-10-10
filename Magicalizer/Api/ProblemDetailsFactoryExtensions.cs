// Copyright © 2026 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Magicalizer.Api;

/// <summary>
/// Provides extension methods for the <see cref="ProblemDetailsFactory"/> class.
/// </summary>
internal static class ProblemDetailsFactoryExtensions
{
  /// <summary>
  /// Creates a 400 Bad Request result with the standard validation problem details (RFC 9457) built from the model state,
  /// the same as <see cref="ControllerBase.ValidationProblem(ModelStateDictionary)"/> does in the actions.
  /// </summary>
  /// <param name="problemDetailsFactory">The factory to create the problem details.</param>
  /// <param name="httpContext">The HTTP context of the request.</param>
  /// <param name="modelState">The model state that contains the errors.</param>
  /// <returns>The result to return to the client.</returns>
  public static ObjectResult CreateValidationProblemResult(this ProblemDetailsFactory problemDetailsFactory, HttpContext httpContext, ModelStateDictionary modelState)
  {
    ValidationProblemDetails validationProblemDetails = problemDetailsFactory.CreateValidationProblemDetails(httpContext, modelState);

    return new ObjectResult(validationProblemDetails) { StatusCode = validationProblemDetails.Status ?? StatusCodes.Status400BadRequest };
  }
}
