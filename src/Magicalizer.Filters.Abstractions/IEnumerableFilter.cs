// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// Non-generic interface of <see cref="EnumerableFilter{TFilter}"/>, to work with collection filters without knowing
/// their element filter type.
/// </summary>
public interface IEnumerableFilter
{
  /// <summary>
  /// Matches empty collections if <c>true</c>, non-empty ones if <c>false</c>.
  /// </summary>
  bool? IsEmpty { get; }

  /// <summary>
  /// Matches collections with at least one element matching each of these filters.
  /// </summary>
  IEnumerable<IFilter>? Any { get; }

  /// <summary>
  /// Matches collections without elements matching any of these filters.
  /// </summary>
  IEnumerable<IFilter>? None { get; }
}
