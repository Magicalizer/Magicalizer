// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// Base of the filters of value properties that are ordered (e.g. <see cref="IntegerFilter"/>): the null, equality,
/// range and list conditions. Only the conditions that are set are applied.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public abstract class ComparableFilterBase<T> : EquatableFilterBase<T> where T : struct, IComparable<T>
{
  /// <summary>
  /// Matches values greater than or equal to this one.
  /// </summary>
  public T? From { get; set; }

  /// <summary>
  /// Matches values less than or equal to this one.
  /// </summary>
  public T? To { get; set; }

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
  protected ComparableFilterBase(bool? isNull = null, T? equals = null, T? notEquals = null, T? from = null, T? to = null, IEnumerable<T>? @in = null, IEnumerable<T>? notIn = null)
    : base(isNull, equals, notEquals, @in, notIn)
  {
    this.From = from;
    this.To = to;
  }
}
