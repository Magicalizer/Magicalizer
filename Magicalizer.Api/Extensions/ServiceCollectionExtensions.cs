// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using ExtCore.WebApplication.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer.Api.Extensions
{
  public static class ServiceCollectionExtensions
  {
    /// <summary>
    /// Registers and configures all the services that are required to process the REST API requests.
    /// </summary>
    public static void AddMagicalizer(this IServiceCollection services)
    {
      services.AddExtCore();
    }
  }
}