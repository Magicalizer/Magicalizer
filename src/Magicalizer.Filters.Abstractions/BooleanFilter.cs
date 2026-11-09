// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// Filter of <see cref="bool"/> properties. Only the conditions that are set are applied.
/// </summary>
public class BooleanFilter : ValueFilterBase<bool>
{
  /// <summary>
  /// Creates an empty filter.
  /// </summary>
  public BooleanFilter() { }

  /// <summary>
  /// Creates a filter with the given conditions.
  /// </summary>
  /// <param name="isNull">Matches null values if <c>true</c>, non-null ones if <c>false</c>.</param>
  /// <param name="equals">Matches values equal to this one.</param>
  public BooleanFilter(bool? isNull = null, bool? equals = null) : base(isNull, equals) { }
}
