// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// Filter of <see cref="byte"/> properties. Only the conditions that are set are applied.
/// </summary>
public class ByteFilter : ComparableFilterBase<byte>
{
  /// <summary>
  /// Creates an empty filter.
  /// </summary>
  public ByteFilter() { }

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
  public ByteFilter(bool? isNull = null, byte? equals = null, byte? notEquals = null, byte? from = null, byte? to = null, IEnumerable<byte>? @in = null, IEnumerable<byte>? notIn = null) : base(isNull, equals, notEquals, from, to, @in, notIn) { }
}
