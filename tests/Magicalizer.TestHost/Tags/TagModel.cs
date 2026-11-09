// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Models.Abstractions;

namespace Magicalizer.TestHost.Tags;

/// <summary>
/// Tag of products.
/// </summary>
public class TagModel : IModel<TagEntity, TagFilter>
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
  /// Creates an empty model.
  /// </summary>
  public TagModel() { }

  /// <summary>
  /// Creates a model from the entity.
  /// </summary>
  public TagModel(TagEntity tagEntity)
  {
    this.Id = tagEntity.Id;
    this.Name = tagEntity.Name;
  }

  /// <inheritdoc/>
  public TagEntity ToEntity()
  {
    return new TagEntity {
      Id = this.Id,
      Name = this.Name!
    };
  }
}
