// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// Base of the filters of value properties (e.g. <see cref="BooleanFilter"/>): the null and equality conditions. Only
/// the conditions that are set are applied.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public abstract class ValueFilterBase<T> : IFilter where T : struct
{
  /// <summary>
  /// Matches null values if <c>true</c>, non-null ones if <c>false</c>.
  /// </summary>
  public bool? IsNull { get; set; }

  /// <summary>
  /// Matches values equal to this one.
  /// </summary>
  public new T? Equals { get; set; }

  /// <summary>
  /// Creates a filter with the given conditions.
  /// </summary>
  /// <param name="isNull">Matches null values if <c>true</c>, non-null ones if <c>false</c>.</param>
  /// <param name="equals">Matches values equal to this one.</param>
  protected ValueFilterBase(bool? isNull = null, T? equals = null)
  {
    this.IsNull = isNull;
    this.Equals = equals;
  }
}
