// Copyright © 2026 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer.Api;

/// <summary>
/// Converts the exceptions caused by invalid query parameters (sorting and inclusions)
/// into a 400 Bad Request response with the error message attached to the corresponding parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class InvalidQueryExceptionFilterAttribute : ExceptionFilterAttribute
{
  /// <summary>
  /// The name of the query string parameter that contains the sorting rules.
  /// </summary>
  public const string SortParameterName = "sort";

  /// <summary>
  /// The name of the query string parameter that contains the inclusion rules.
  /// </summary>
  public const string IncludeParameterName = "include";

  /// <inheritdoc />
  public override void OnException(ExceptionContext context)
  {
    string? parameterName = context.Exception switch
    {
      InvalidSortingException => SortParameterName,
      InvalidInclusionException => IncludeParameterName,
      _ => null
    };

    if (parameterName == null) return;

    context.ModelState.AddModelError(parameterName, context.Exception.Message);
    context.Result = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>().CreateValidationProblemResult(context.HttpContext, context.ModelState);
    context.ExceptionHandled = true;
  }
}