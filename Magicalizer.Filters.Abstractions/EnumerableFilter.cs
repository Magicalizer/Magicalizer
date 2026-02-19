// Copyright © 2021 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// A filter for enumerables that allows filtering based on whether any or none of the child elements meet specific criteria.
/// </summary>
/// <typeparam name="TFilter">The filter type applied to the elements in the collection.</typeparam>
public class EnumerableFilter<TFilter> : IEnumerableFilter, IFilter where TFilter : IFilter
{
  /// <summary>
  /// Specifies that the collection must be empty (contain no elements).
  /// </summary>
  public bool? IsEmpty { get; set; }

  /// <summary>
  /// Specifies that the collection must be non-empty (contain at least one element).
  /// </summary>
  public bool? IsNotEmpty { get; set; }

  /// <summary>
  ///  Specifies a condition that at least one element in the collection must satisfy.
  /// </summary>
  public IEnumerable<TFilter>? Any { get; set; }
  IEnumerable<IFilter>? IEnumerableFilter.Any => Any?.Cast<IFilter>();

  /// <summary>
  ///  Specifies a condition that none of the elements in the collection should satisfy.
  /// </summary>
  public IEnumerable<TFilter>? None { get; set; }
  IEnumerable<IFilter>? IEnumerableFilter.None => None?.Cast<IFilter>();

  /// <summary>
  /// Initializes a new instance of the <see cref="EnumerableFilter{TFilter}"/> class.
  /// </summary>
  public EnumerableFilter() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="EnumerableFilter{TFilter}"/> class with specific filter conditions.
  /// </summary>
  /// <param name="isEmpty">If true, the collection must be empty.</param>
  /// <param name="isNotEmpty">If true, the collection must be non-empty.</param>
  /// <param name="any">A condition that at least one element must satisfy.</param>
  /// <param name="none">A condition that no elements should satisfy.</param>
  public EnumerableFilter(bool? isEmpty = null, bool? isNotEmpty = null, IEnumerable<TFilter>? any = default, IEnumerable<TFilter>? none = default)
  {
    this.IsEmpty = isEmpty;
    this.IsNotEmpty = IsNotEmpty;
    this.Any = any;
    this.None = none;
  }
}