// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using FluentValidation;
using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.TestHost.Products;
using Magicalizer.Validators.Abstractions;
using HttpMethod = Magicalizer.Api.Dto.Abstractions.HttpMethod;

namespace Magicalizer.TestHost.Photos;

/// <summary>
/// Photo of a product. It's served by the custom controller (see <see cref="PhotosController"/>), and only the
/// authenticated users can delete the photos. A new photo must have an extension (a rule of the creation only).
/// </summary>
[Magicalized("api/photos")]
[AuthenticatedOnly(HttpMethod.Delete)]
public class PhotoDto : IDto<PhotoModel>
{
  /// <summary>
  /// Photo ID.
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// Product of the photo (only with the ID if it isn't included).
  /// </summary>
  public ProductDto? Product { get; set; }

  /// <summary>
  /// Filename of the photo (e.g. "margherita.jpg").
  /// </summary>
  public string? Filename { get; set; }

  /// <summary>
  /// Creates an empty DTO.
  /// </summary>
  public PhotoDto() { }

  /// <summary>
  /// Creates a DTO from the model.
  /// </summary>
  public PhotoDto(PhotoModel photoModel) : this(photoModel, ignoreProduct: false) { }

  /// <summary>
  /// Creates a DTO from the model.
  /// </summary>
  /// <param name="photoModel">Model to create the DTO from.</param>
  /// <param name="ignoreProduct">Whether the product is left out (to avoid cyclic references).</param>
  public PhotoDto(PhotoModel photoModel, bool ignoreProduct)
  {
    this.Id = photoModel.Id;

    if (!ignoreProduct)
      this.Product = photoModel.Product == null ? null : new ProductDto(photoModel.Product);

    this.Filename = photoModel.Filename;
  }

  /// <inheritdoc/>
  public PhotoModel ToModel()
  {
    return new PhotoModel {
      Id = this.Id,
      Product = this.Product?.ToModel(),
      Filename = this.Filename
    };
  }

  /// <summary>
  /// Validator of the photo.
  /// </summary>
  public class Validator : AbstractValidator<PhotoDto>
  {
    /// <summary>
    /// Creates the validator.
    /// </summary>
    public Validator()
    {
      this.RuleFor(p => p.Product).NotNull().DependentRules(() => {
        this.RuleFor(p => p.Product!.Id).GreaterThan(0);
      });

      this.RuleFor(p => p.Filename).NotEmpty().MaximumLength(64);
      this.RuleSet(RuleSetName.Create, () => {
        this.RuleFor(p => p.Filename).Must(Path.HasExtension).WithMessage("A new photo must have an extension.");
      });
    }
  }
}
