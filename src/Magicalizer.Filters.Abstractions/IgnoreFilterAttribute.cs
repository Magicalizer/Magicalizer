// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// Marks a filter property that isn't applied to queries (e.g. it's used by a query prefilter).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class IgnoreFilterAttribute : Attribute
{
}
