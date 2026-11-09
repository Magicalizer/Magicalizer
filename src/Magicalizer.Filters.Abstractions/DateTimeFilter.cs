// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// Filter of <see cref="DateTime"/> properties. Only the conditions that are set are applied.
/// </summary>
public class DateTimeFilter : ComparableFilterBase<DateTime>
{
  /// <summary>
  /// Creates an empty filter.
  /// </summary>
  public DateTimeFilter() { }

  /// <summary>
  /// Creates a filter with the given conditions.
  /// </summary>
  /// <param name="isNull">Matches null values if <c>true</c>, non-null ones if <c>false</c>.</param>
  /// <param name="equals">Matches values equal to this one.</param>
  /// <param name="notEquals">Matches values not equal to this one.</param>
  /// <param name="from">Matches values greater than or equal to this one.</param>
  /// <param name="to">Matches values less than or equal to this one.</param>
  /// <param name="in">
  /// Matches values equal to any of these (the query parameter is repeated, e.g. "id.in=1&amp;id.in=2").
  /// </param>
  /// <param name="notIn">
  /// Matches values not equal to any of these (the query parameter is repeated, e.g. "id.notin=1&amp;id.notin=2").
  /// </param>
  public DateTimeFilter(bool? isNull = null, DateTime? equals = null, DateTime? notEquals = null, DateTime? from = null, DateTime? to = null, IEnumerable<DateTime>? @in = null, IEnumerable<DateTime>? notIn = null) : base(isNull, equals, notEquals, from, to, @in, notIn) { }
}
