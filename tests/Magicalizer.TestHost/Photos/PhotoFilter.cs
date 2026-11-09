// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Filters.Abstractions;
using Magicalizer.TestHost.Products;

namespace Magicalizer.TestHost.Photos;

/// <summary>
/// Filter for photos. Only the conditions that are set are applied.
/// </summary>
public class PhotoFilter : IFilter
{
  /// <summary>
  /// Filters by ID.
  /// </summary>
  public IntegerFilter? Id { get; set; }

  /// <summary>
  /// Filters by product.
  /// </summary>
  public ProductFilter? Product { get; set; }

  /// <summary>
  /// Filters by filename.
  /// </summary>
  public StringFilter? Filename { get; set; }
}
