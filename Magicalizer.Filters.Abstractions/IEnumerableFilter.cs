// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// A non-generic interface of the <see cref="EnumerableFilter{TFilter}"/>,
/// which allows working with the enumerable filters without knowing the filter type of the collection elements.
/// </summary>
public interface IEnumerableFilter
{
  /// <summary>
  /// Specifies that the collection must be empty (contain no elements).
  /// </summary>
  bool? IsEmpty { get; }

  /// <summary>
  /// Specifies that the collection must be non-empty (contain at least one element).
  /// </summary>
  bool? IsNotEmpty { get; }

  /// <summary>
  /// Specifies the conditions that at least one element in the collection must satisfy.
  /// </summary>
  IEnumerable<IFilter>? Any { get; }

  /// <summary>
  /// Specifies the conditions that none of the elements in the collection should satisfy.
  /// </summary>
  IEnumerable<IFilter>? None { get; }
}
