// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Filters.Abstractions;
using Magicalizer.TestHost.Products;

namespace Magicalizer.TestHost.Tags;

/// <summary>
/// Filter for tags of products. Only the conditions that are set are applied.
/// </summary>
public class ProductTagFilter : IFilter
{
  /// <summary>
  /// Filters by product.
  /// </summary>
  public ProductFilter? Product { get; set; }

  /// <summary>
  /// Filters by tag.
  /// </summary>
  public TagFilter? Tag { get; set; }

  /// <summary>
  /// Filters by position.
  /// </summary>
  public IntegerFilter? Position { get; set; }
}
