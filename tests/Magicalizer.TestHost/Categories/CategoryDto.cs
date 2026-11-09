// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using FluentValidation;
using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.TestHost.Products;
using Magicalizer.Validators.Abstractions;

namespace Magicalizer.TestHost.Categories;

/// <summary>
/// Category of products, served at "/api/categories" with the standard API (open for everyone).
/// </summary>
[Magicalized("api/categories")]
public class CategoryDto : IDto<CategoryModel>
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
  /// Description. It's non-nullable and has no rules, so it shows that the implicit [Required] of ASP.NET Core isn't
  /// applied to the DTOs.
  /// </summary>
  public string Description { get; set; } = string.Empty;

  /// <summary>
  /// Products of the category (if they are included).
  /// </summary>
  public IEnumerable<ProductDto>? Products { get; set; }

  /// <summary>
  /// Creates an empty DTO.
  /// </summary>
  public CategoryDto() { }

  /// <summary>
  /// Creates a DTO from the model.
  /// </summary>
  public CategoryDto(CategoryModel categoryModel) : this(categoryModel, ignoreProducts: false) { }

  /// <summary>
  /// Creates a DTO from the model.
  /// </summary>
  /// <param name="categoryModel">Model to create the DTO from.</param>
  /// <param name="ignoreProducts">Whether the products are left out (to avoid cyclic references).</param>
  public CategoryDto(CategoryModel categoryModel, bool ignoreProducts)
  {
    this.Id = categoryModel.Id;
    this.Name = categoryModel.Name;
    this.Description = categoryModel.Description ?? string.Empty;

    if (!ignoreProducts)
      this.Products = categoryModel.Products?.Select(p => new ProductDto(p, ignoreCategory: true)).ToList();
  }

  /// <inheritdoc/>
  public CategoryModel ToModel()
  {
    return new CategoryModel {
      Id = this.Id,
      Name = this.Name,
      Description = string.IsNullOrEmpty(this.Description) ? null : this.Description
    };
  }

  /// <summary>
  /// Validator of the category.
  /// </summary>
  public class Validator : AbstractValidator<CategoryDto>
  {
    /// <summary>
    /// Creates the validator.
    /// </summary>
    public Validator()
    {
      this.RuleFor(c => c.Name).NotEmpty().MaximumLength(64);
      this.RuleSet(RuleSetName.Edit, () => {
        this.RuleFor(c => c.Id).GreaterThan(0);
      });
    }
  }
}
