// Copyright © 2021 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// A filter for enumerables that allows filtering based on whether any or none of the elements meet specific criteria.
/// </summary>
/// <typeparam name="TFilter">The filter type applied to the elements in the collection.</typeparam>
public class EnumerableFilter<TFilter> : IFilter where TFilter : IFilter
{
  /// <summary>
  ///  Specifies a condition that at least one element in the collection must satisfy.
  /// </summary>
  public TFilter? Any { get; set; }

  /// <summary>
  ///  Specifies a condition that none of the elements in the collection should satisfy.
  /// </summary>
  public TFilter? None { get; set; }

  /// <summary>
  /// Initializes a new instance of the <see cref="EnumerableFilter{TFilter}"/> class.
  /// </summary>
  public EnumerableFilter() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="EnumerableFilter{TFilter}"/> class with specific filter conditions.
  /// </summary>
  /// <param name="any">A condition that at least one element must satisfy.</param>
  /// <param name="none">A condition that no elements should satisfy.</param>
  public EnumerableFilter(TFilter? any = default, TFilter? none = default)
  {
    this.Any = any;
    this.None = none;
  }
}