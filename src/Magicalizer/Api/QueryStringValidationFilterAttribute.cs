// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Magicalizer.Api;

/// <summary>
/// Adds the model state errors for the query parameters the action doesn't support (e.g. a misspelled filter
/// condition) and for the invalid collection conditions (the list form with gaps in the indexes or combined with the
/// single form), as the response would have the objects the client didn't ask for. The parameters of
/// <see cref="MagicalizerOptions.IgnoredQueryParameters"/> are allowed anyway. It runs before
/// <see cref="ApiControllerAttribute"/> rejects the requests with the model state errors, so the client gets all the
/// errors of the request at once.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class QueryStringValidationFilterAttribute : ActionFilterAttribute
{
  // Validators of the actions, as their parameters don't change.
  private static readonly ConcurrentDictionary<string, QueryStringValidator> ValidatorsByActions = [];

  /// <summary>
  /// Creates a filter. It runs before the filter of <see cref="ApiControllerAttribute"/> (-2000).
  /// </summary>
  public QueryStringValidationFilterAttribute()
  {
    this.Order = -2020;
  }

  /// <inheritdoc/>
  public override void OnActionExecuting(ActionExecutingContext context)
  {
    MagicalizerOptions options = context.HttpContext.RequestServices.GetRequiredService<IOptions<MagicalizerOptions>>().Value;
    QueryStringValidator validator = ValidatorsByActions.GetOrAdd(
      context.ActionDescriptor.Id,
      _ => new QueryStringValidator(context.ActionDescriptor.Parameters.OfType<ControllerParameterDescriptor>())
    );

    foreach (KeyValuePair<string, string> error in validator.Validate(context.HttpContext.Request.Query.Keys.Where(k => !options.IgnoredQueryParameters.Contains(k))))
      context.ModelState.AddModelError(error.Key, error.Value);
  }
}
