// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// Filter of collection properties: by emptiness and by filters that any or none of the elements match. Only the
/// conditions that are set are applied.
/// </summary>
/// <typeparam name="TFilter">Element filter type.</typeparam>
public class EnumerableFilter<TFilter> : IEnumerableFilter, IFilter where TFilter : IFilter
{
  /// <summary>
  /// Matches empty collections if <c>true</c>, non-empty ones if <c>false</c>.
  /// </summary>
  public bool? IsEmpty { get; set; }

  /// <summary>
  /// Matches collections with at least one element matching each of these filters.
  /// </summary>
  public IEnumerable<TFilter>? Any { get; set; }
  IEnumerable<IFilter>? IEnumerableFilter.Any => this.Any?.Cast<IFilter>();

  /// <summary>
  /// Matches collections without elements matching any of these filters.
  /// </summary>
  public IEnumerable<TFilter>? None { get; set; }
  IEnumerable<IFilter>? IEnumerableFilter.None => this.None?.Cast<IFilter>();

  /// <summary>
  /// Creates an empty filter.
  /// </summary>
  public EnumerableFilter() { }

  /// <summary>
  /// Creates a filter with the given conditions.
  /// </summary>
  /// <param name="isEmpty">Matches empty collections if <c>true</c>, non-empty ones if <c>false</c>.</param>
  /// <param name="any">Matches collections with at least one element matching each of these filters.</param>
  /// <param name="none">Matches collections without elements matching any of these filters.</param>
  public EnumerableFilter(bool? isEmpty = null, IEnumerable<TFilter>? any = null, IEnumerable<TFilter>? none = null)
  {
    this.IsEmpty = isEmpty;
    this.Any = any;
    this.None = none;
  }
}
