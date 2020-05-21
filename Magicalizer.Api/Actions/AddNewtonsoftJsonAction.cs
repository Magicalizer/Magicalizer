// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using ExtCore.Mvc.Infrastructure.Actions;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer.Api
{
  public class AddNewtonsoftJsonAction : IAddMvcAction
  {
    public int Priority => 4000;

    public void Execute(IMvcBuilder mvcBuilder, IServiceProvider serviceProvider)
    {
      mvcBuilder.AddNewtonsoftJson();
    }
  }
}