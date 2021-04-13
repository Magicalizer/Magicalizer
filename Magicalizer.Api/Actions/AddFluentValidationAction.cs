// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using ExtCore.Infrastructure;
using ExtCore.Mvc.Infrastructure.Actions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer.Api.Actions
{
  public class AddFluentValidationAction : IAddMvcAction
  {
    public int Priority => 3000;

    public void Execute(IMvcBuilder mvcBuilder, IServiceProvider serviceProvider)
    {
      ValidatorOptions.Global.LanguageManager = new CustomLanguageManager();
      mvcBuilder.AddFluentValidation(fv => {
        fv.RegisterValidatorsFromAssemblies(ExtensionManager.Assemblies);
        fv.LocalizationEnabled = false;
        fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
      });
    }
  }
}
