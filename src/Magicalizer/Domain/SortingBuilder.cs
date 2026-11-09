// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.PropertyPaths;

namespace Magicalizer.Domain;

/// <summary>
/// Builder of a model sorting.
/// </summary>
/// <typeparam name="TModel">Model type.</typeparam>
public class SortingBuilder<TModel> : PropertyPathBuilder<TModel, Sorting<TModel>> where TModel : class, IModel
{
  /// <summary>
  /// Creates a builder.
  /// </summary>
  /// <param name="isAscending">Whether the sorting is ascending (otherwise descending).</param>
  public SortingBuilder(bool isAscending) : base(propertyPath => new Sorting<TModel>(isAscending, propertyPath)) { }
}
