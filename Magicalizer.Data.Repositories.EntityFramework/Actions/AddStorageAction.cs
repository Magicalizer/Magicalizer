// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using ExtCore.Infrastructure.Actions;
using Magicalizer.Data.Repositories.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer.Data.Repositories.EntityFramework.Actions
{
  public class AddStorageAction : IConfigureServicesAction
  {
    public int Priority => 1000;

    public void Execute(IServiceCollection services, IServiceProvider serviceProvider)
    {
      services.AddScoped<IStorage, Storage>();
    }
  }
}