// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using ExtCore.Infrastructure.Actions;
using Magicalizer.Domain.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer.Domain.Services.Defaults.Actions
{
  public class AddServiceResolverAction : IConfigureServicesAction
  {
    public int Priority => 1000;

    public void Execute(IServiceCollection services, IServiceProvider serviceProvider)
    {
      services.AddScoped<IServiceResolver, ServiceResolver>();
    }
  }
}