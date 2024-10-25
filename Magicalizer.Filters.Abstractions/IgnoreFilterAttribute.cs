﻿// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// Marks a filter property to be ignored during filtering.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class IgnoreFilterAttribute : Attribute
{
}