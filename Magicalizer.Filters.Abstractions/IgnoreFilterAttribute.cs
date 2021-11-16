// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Magicalizer.Filters.Abstractions
{
  /// <summary>
  /// Allows to ignore a filter's property.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class IgnoreFilterAttribute : Attribute
  {
  }
}