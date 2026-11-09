// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Filters.Abstractions;
using Magicalizer.TestHost.Categories;
using Magicalizer.TestHost.Photos;
using Magicalizer.TestHost.Tags;

namespace Magicalizer.TestHost.Products;

/// <summary>
/// Filter for products. Only the conditions that are set are applied.
/// </summary>
public class ProductFilter : IFilter
{
  /// <summary>
  /// Filters by ID.
  /// </summary>
  public IntegerFilter? Id { get; set; }

  /// <summary>
  /// Filters by category.
  /// </summary>
  public CategoryFilter? Category { get; set; }

  /// <summary>
  /// Filters by name.
  /// </summary>
  public StringFilter? Name { get; set; }

  /// <summary>
  /// Filters by price.
  /// </summary>
  public DecimalFilter? Price { get; set; }

  /// <summary>
  /// Filters by quantity.
  /// </summary>
  public IntegerFilter? Quantity { get; set; }

  /// <summary>
  /// Filters by availability.
  /// </summary>
  public BooleanFilter? IsAvailable { get; set; }

  /// <summary>
  /// Filters by creation time.
  /// </summary>
  public DateTimeFilter? Created { get; set; }

  /// <summary>
  /// Filters by code.
  /// </summary>
  public GuidFilter? Code { get; set; }

  /// <summary>
  /// Filters by description.
  /// </summary>
  public StringFilter? Description { get; set; }

  /// <summary>
  /// Filters by whether the product is archived (the archived ones are hidden anyway if the user can't see them).
  /// </summary>
  public BooleanFilter? IsArchived { get; set; }

  /// <summary>
  /// Filters by diet.
  /// </summary>
  public EnumFilter<Diet>? Diet { get; set; }

  /// <summary>
  /// Filters by update time.
  /// </summary>
  public DateTimeOffsetFilter? Updated { get; set; }

  /// <summary>
  /// Filters by photos (any, none, empty or not empty).
  /// </summary>
  public EnumerableFilter<PhotoFilter>? Photos { get; set; }

  /// <summary>
  /// Filters by tags (any, none, empty or not empty).
  /// </summary>
  public EnumerableFilter<ProductTagFilter>? Tags { get; set; }

  /// <summary>
  /// Text to search in the names. It isn't applied by the filter itself, the service converts it to the name condition
  /// (see <see cref="ProductService"/>).
  /// </summary>
  [IgnoreFilter]
  public string? Search { get; set; }
}
