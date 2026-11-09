// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// Base of the filters of value properties that are compared for equality only (e.g. <see cref="GuidFilter"/>): the
/// null, equality and list conditions. Only the conditions that are set are applied.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public abstract class EquatableFilterBase<T> : ValueFilterBase<T> where T : struct
{
  /// <summary>
  /// Matches values not equal to this one.
  /// </summary>
  public T? NotEquals { get; set; }

  /// <summary>
  /// Matches values equal to any of these (the query parameter is repeated, e.g. "id.in=1&amp;id.in=2").
  /// </summary>
  public IEnumerable<T>? In { get; set; }

  /// <summary>
  /// Matches values not equal to any of these (the query parameter is repeated, e.g. "id.notin=1&amp;id.notin=2").
  /// </summary>
  public IEnumerable<T>? NotIn { get; set; }

  /// <summary>
  /// Creates a filter with the given conditions.
  /// </summary>
  /// <param name="isNull">Matches null values if <c>true</c>, non-null ones if <c>false</c>.</param>
  /// <param name="equals">Matches values equal to this one.</param>
  /// <param name="notEquals">Matches values not equal to this one.</param>
  /// <param name="in">
  /// Matches values equal to any of these (the query parameter is repeated, e.g. "id.in=1&amp;id.in=2").
  /// </param>
  /// <param name="notIn">
  /// Matches values not equal to any of these (the query parameter is repeated, e.g. "id.notin=1&amp;id.notin=2").
  /// </param>
  protected EquatableFilterBase(bool? isNull = null, T? equals = null, T? notEquals = null, IEnumerable<T>? @in = null, IEnumerable<T>? notIn = null)
    : base(isNull, equals)
  {
    this.NotEquals = notEquals;
    this.In = @in;
    this.NotIn = notIn;
  }
}
