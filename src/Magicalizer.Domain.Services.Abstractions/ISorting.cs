// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Models.Abstractions;

namespace Magicalizer.Domain.Services.Abstractions;

/// <summary>
/// Sorting of a query by a property.
/// </summary>
/// <typeparam name="TModel">Model type.</typeparam>
public interface ISorting<TModel> where TModel : class, IModel
{
  /// <summary>
  /// Whether the sorting is ascending (otherwise descending).
  /// </summary>
  bool IsAscending { get; }

  /// <summary>
  /// Property path to sort by (e.g. "Category.Name").
  /// </summary>
  string PropertyPath { get; }
}
