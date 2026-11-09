// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Filters.Abstractions;

namespace Magicalizer.UnitTests.Data;

/// <summary>
/// Filter of the test groups.
/// </summary>
public class GroupFilter : IFilter
{
  public StringFilter? Name { get; set; }
}
