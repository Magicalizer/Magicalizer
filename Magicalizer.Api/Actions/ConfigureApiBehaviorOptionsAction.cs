// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using ExtCore.Infrastructure.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer.Api.Actions
{
  public class ConfigureApiBehaviorOptionsAction : IConfigureServicesAction
  {
    public int Priority => 20000;

    public void Execute(IServiceCollection services, IServiceProvider serviceProvider)
    {
      services.Configure<ApiBehaviorOptions>(apiBehaviorOptions =>
        apiBehaviorOptions.InvalidModelStateResponseFactory = actionContext =>
        {
          return new BadRequestObjectResult(
            new
            {
              Message = "Validation failed.",
              Details = actionContext.ModelState.Select(
                mse => new
                {
                  Field = mse.Key.ToCamelCase(),
                  Errors = mse.Value.Errors.Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? "Value cannot be parsed." : e.ErrorMessage)
                }
              )
            }
          );
        }
      );
    }
  }
}
