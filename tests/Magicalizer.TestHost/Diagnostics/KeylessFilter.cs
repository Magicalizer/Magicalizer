// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Filters.Abstractions;

namespace Magicalizer.TestHost.Diagnostics;

/// <summary>
/// Filter of <see cref="KeylessModel"/>.
/// </summary>
public class KeylessFilter : IFilter
{
  /// <summary>
  /// Filters by value.
  /// </summary>
  public StringFilter? Value { get; set; }
}
