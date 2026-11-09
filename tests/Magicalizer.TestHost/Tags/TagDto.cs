// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using FluentValidation;
using Magicalizer.Api.Dto.Abstractions;
using HttpMethod = Magicalizer.Api.Dto.Abstractions.HttpMethod;

namespace Magicalizer.TestHost.Tags;

/// <summary>
/// Tag of products, served at "/api/tags" with the GET and POST methods only.
/// </summary>
[Magicalized("api/tags", HttpMethod.Get, HttpMethod.Post)]
public class TagDto : IDto<TagModel>
{
  /// <summary>
  /// Tag ID.
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// Tag name (e.g. "Hot").
  /// </summary>
  public string? Name { get; set; }

  /// <summary>
  /// Creates an empty DTO.
  /// </summary>
  public TagDto() { }

  /// <summary>
  /// Creates a DTO from the model.
  /// </summary>
  public TagDto(TagModel tagModel)
  {
    this.Id = tagModel.Id;
    this.Name = tagModel.Name;
  }

  /// <inheritdoc/>
  public TagModel ToModel()
  {
    return new TagModel {
      Id = this.Id,
      Name = this.Name
    };
  }

  /// <summary>
  /// Validator of the tag.
  /// </summary>
  public class Validator : AbstractValidator<TagDto>
  {
    /// <summary>
    /// Creates the validator.
    /// </summary>
    public Validator()
    {
      this.RuleFor(t => t.Name).NotEmpty().MaximumLength(64);
    }
  }
}
