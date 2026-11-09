// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// Filter of enum properties. The values are passed by their names or numbers (e.g. "diet.equals=vegan"). Only the
/// conditions that are set are applied.
/// </summary>
/// <typeparam name="TEnum">Enum type.</typeparam>
public class EnumFilter<TEnum> : EquatableFilterBase<TEnum> where TEnum : struct, Enum
{
  /// <summary>
  /// Creates an empty filter.
  /// </summary>
  public EnumFilter() { }

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
  public EnumFilter(bool? isNull = null, TEnum? equals = null, TEnum? notEquals = null, IEnumerable<TEnum>? @in = null, IEnumerable<TEnum>? notIn = null) : base(isNull, equals, notEquals, @in, notIn) { }
}
