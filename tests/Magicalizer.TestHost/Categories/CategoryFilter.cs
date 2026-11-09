// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Filters.Abstractions;
using Magicalizer.TestHost.Products;

namespace Magicalizer.TestHost.Categories;

/// <summary>
/// Filter for categories. Only the conditions that are set are applied.
/// </summary>
public class CategoryFilter : IFilter
{
  /// <summary>
  /// Filters by ID.
  /// </summary>
  public IntegerFilter? Id { get; set; }

  /// <summary>
  /// Filters by name.
  /// </summary>
  public StringFilter? Name { get; set; }

  /// <summary>
  /// Filters by description.
  /// </summary>
  public StringFilter? Description { get; set; }

  /// <summary>
  /// Filters by products (any, none, empty or not empty).
  /// </summary>
  public EnumerableFilter<ProductFilter>? Products { get; set; }
}
