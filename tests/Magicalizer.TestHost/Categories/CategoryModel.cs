// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.TestHost.Products;

namespace Magicalizer.TestHost.Categories;

/// <summary>
/// Category of products.
/// </summary>
public class CategoryModel : IModel<CategoryEntity, CategoryFilter>
{
  /// <summary>
  /// Category ID.
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// Category name (e.g. "Pizza").
  /// </summary>
  public string? Name { get; set; }

  /// <summary>
  /// Description, or <c>null</c> if there is none.
  /// </summary>
  public string? Description { get; set; }

  /// <summary>
  /// Products of the category (if they are included).
  /// </summary>
  public IEnumerable<ProductModel>? Products { get; set; }

  /// <summary>
  /// Creates an empty model.
  /// </summary>
  public CategoryModel() { }

  /// <summary>
  /// Creates a model from the entity.
  /// </summary>
  public CategoryModel(CategoryEntity categoryEntity) : this(categoryEntity, ignoreProducts: false) { }

  /// <summary>
  /// Creates a model from the entity.
  /// </summary>
  /// <param name="categoryEntity">Entity to create the model from.</param>
  /// <param name="ignoreProducts">Whether the products are left out (to avoid cyclic references).</param>
  public CategoryModel(CategoryEntity categoryEntity, bool ignoreProducts)
  {
    this.Id = categoryEntity.Id;
    this.Name = categoryEntity.Name;
    this.Description = categoryEntity.Description;

    if (!ignoreProducts)
      this.Products = categoryEntity.Products?.Select(p => new ProductModel(p, ignoreCategory: true)).ToList();
  }

  /// <inheritdoc/>
  public CategoryEntity ToEntity()
  {
    return new CategoryEntity {
      Id = this.Id,
      Name = this.Name!,
      Description = this.Description
    };
  }
}
