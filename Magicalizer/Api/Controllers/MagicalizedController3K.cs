// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;

namespace Magicalizer.Api.Controllers;

/// <summary>
/// Magicalized controller for managing models with a composite primary key consisting of three properties.
/// </summary>
/// <typeparam name="TKey1">The type of the first key.</typeparam>
/// <typeparam name="TKey2">The type of the second key.</typeparam>
/// <typeparam name="TKey3">The type of the third key.</typeparam>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TDto">The DTO type.</typeparam>
/// <typeparam name="TFilter">The filter type.</typeparam>
public class MagicalizedController<TKey1, TKey2, TKey3, TModel, TDto, TFilter> : MagicalizedControllerBase<TModel, TDto, TFilter>
  where TModel : class, IModel, new()
  where TDto : class, IDto, new()
  where TFilter : class, IFilter, new()
{
  protected readonly IService<TKey1, TKey2, TKey3, TModel, TFilter> service;

  /// <summary>
  /// Initializes a new instance of the <see cref="MagicalizedController{TKey1, TKey2, TKey3, TModel, TDto, TFilter}"/> class.
  /// </summary>
  /// <param name="service">The service for managing the model.</param>
  public MagicalizedController(IService<TKey1, TKey2, TKey3, TModel, TFilter> service)
    : base(service)
  {
    this.service = service;
  }

  /// <summary>
  /// Gets a DTO by its composite primary key.
  /// </summary>
  /// <param name="id1">The first key.</param>
  /// <param name="id2">The second key.</param>
  /// <param name="id3">The third key.</param>
  /// <param name="include">The comma-separated property paths of the related models to include (e.g. "category,photos").</param>
  /// <param name="cancellationToken">The token to cancel the request.</param>
  /// <returns>The requested DTO or a <c>NotFound</c> result if it doesn't exist.</returns>
  [HttpGet("{id1}/{id2}/{id3}")]
  public virtual Task<ActionResult<TDto>> GetAsync(TKey1 id1, TKey2 id2, TKey3 id3, string? include = null, CancellationToken cancellationToken = default) =>
    this.GetAsync(inclusions => this.service.GetByIdAsync(id1, id2, id3, inclusions, cancellationToken), include);

  /// <summary>
  /// Partially updates a model using a JSON patch document.
  /// </summary>
  /// <param name="id1">The first key of the model to update.</param>
  /// <param name="id2">The second key of the model to update.</param>
  /// <param name="id3">The third key of the model to update.</param>
  /// <param name="dtoPatch">The JSON patch document containing the updates.</param>
  /// <param name="cancellationToken">The token to cancel the request.</param>
  /// <returns>A <c>NoContent</c> result if the patch is successful.</returns>
  [HttpPatch("{id1}/{id2}/{id3}")]
  public virtual Task<IActionResult> PatchAsync(TKey1 id1, TKey2 id2, TKey3 id3, [FromBody] JsonPatchDocument<TDto> dtoPatch, CancellationToken cancellationToken = default) =>
    this.PatchAsync(() => this.service.GetByIdAsync(id1, id2, id3, cancellationToken: cancellationToken), dtoPatch, cancellationToken);

  /// <summary>
  /// Deletes a model by its composite primary key.
  /// </summary>
  /// <param name="id1">The first key of the model to delete.</param>
  /// <param name="id2">The second key of the model to delete.</param>
  /// <param name="id3">The third key of the model to delete.</param>
  /// <param name="cancellationToken">The token to cancel the request.</param>
  /// <returns>A <c>NoContent</c> result if the deletion is successful.</returns>
  [HttpDelete("{id1}/{id2}/{id3}")]
  public virtual Task<IActionResult> DeleteAsync(TKey1 id1, TKey2 id2, TKey3 id3, CancellationToken cancellationToken = default) =>
    this.DeleteAsync(() => this.service.DeleteAsync(id1, id2, id3, cancellationToken));

  private protected override object GetRouteValues(object?[] keyValues) =>
    new { id1 = keyValues[0], id2 = keyValues[1], id3 = keyValues[2] };
}