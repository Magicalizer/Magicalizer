// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Magicalizer.Api.Dto.Abstractions
{
  [AttributeUsage(AttributeTargets.Class)]
  public class MagicalizedAttribute : Attribute
  {
    public string Route { get; }

    public MagicalizedAttribute(string route)
    {
      this.Route = route;
    }
  }
}