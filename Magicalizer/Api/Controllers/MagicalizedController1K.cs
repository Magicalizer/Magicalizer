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
/// Magicalized controller for managing models with a single primary key.
/// </summary>
/// <typeparam name="TKey">The type of the primary key.</typeparam>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TDto">The DTO type.</typeparam>
/// <typeparam name="TFilter">The filter type.</typeparam>
public class MagicalizedController<TKey, TModel, TDto, TFilter> : MagicalizedControllerBase<TModel, TDto, TFilter>
  where TModel : class, IModel, new()
  where TDto : class, IDto, new()
  where TFilter : class, IFilter, new()
{
  protected readonly IService<TKey, TModel, TFilter> service;

  /// <summary>
  /// Initializes a new instance of the <see cref="MagicalizedController{TKey, TModel, TDto, TFilter}"/> class.
  /// </summary>
  /// <param name="service">The service for managing the model.</param>
  public MagicalizedController(IService<TKey, TModel, TFilter> service)
    : base(service)
  {
    this.service = service;
  }

  /// <summary>
  /// Gets a DTO by its primary key.
  /// </summary>
  /// <param name="id">The primary key of the model.</param>
  /// <param name="include">The comma-separated property paths of the related models to include (e.g. "category,photos").</param>
  /// <param name="cancellationToken">The token to cancel the request.</param>
  /// <returns>The requested DTO or a <c>NotFound</c> result if it doesn't exist.</returns>
  [HttpGet("{id}")]
  public virtual Task<ActionResult<TDto>> GetAsync(TKey id, string? include = null, CancellationToken cancellationToken = default) =>
    this.GetAsync(inclusions => this.service.GetByIdAsync(id, inclusions, cancellationToken), include);

  /// <summary>
  /// Partially updates a model using a JSON patch document.
  /// </summary>
  /// <param name="id">The primary key of the model to update.</param>
  /// <param name="dtoPatch">The JSON patch document containing the updates.</param>
  /// <param name="cancellationToken">The token to cancel the request.</param>
  /// <returns>A <c>NoContent</c> result if the patch is successful.</returns>
  [HttpPatch("{id}")]
  public virtual Task<IActionResult> PatchAsync(TKey id, [FromBody] JsonPatchDocument<TDto> dtoPatch, CancellationToken cancellationToken = default) =>
    this.PatchAsync(() => this.service.GetByIdAsync(id, cancellationToken: cancellationToken), dtoPatch, cancellationToken);

  /// <summary>
  /// Deletes a model by its primary key.
  /// </summary>
  /// <param name="id">The primary key of the model to delete.</param>
  /// <param name="cancellationToken">The token to cancel the request.</param>
  /// <returns>A <c>NoContent</c> result if the deletion is successful.</returns>
  [HttpDelete("{id}")]
  public virtual Task<IActionResult> DeleteAsync(TKey id, CancellationToken cancellationToken = default) =>
    this.DeleteAsync(() => this.service.DeleteAsync(id, cancellationToken));

  private protected override object GetRouteValues(object?[] keyValues) =>
    new { id = keyValues[0] };
}