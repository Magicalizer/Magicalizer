﻿// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using ExtCore.WebApplication.Extensions;
using Microsoft.AspNetCore.Builder;

namespace Magicalizer.Api.Extensions
{
  public static class ApplicationBuilderExtensions
  {
    public static void UseMagicalizer(this IApplicationBuilder applicationBuilder)
    {
      applicationBuilder.UseExtCore();
    }
  }
}