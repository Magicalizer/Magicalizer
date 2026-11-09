// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;

namespace Magicalizer.Api.Controllers;

/// <summary>
/// Magicalized controller of models with a primary key of one property.
/// </summary>
/// <typeparam name="TKey">Primary key type.</typeparam>
/// <typeparam name="TModel">Model type.</typeparam>
/// <typeparam name="TDto">DTO type.</typeparam>
/// <typeparam name="TFilter">Filter type.</typeparam>
public class MagicalizedController<TKey, TModel, TDto, TFilter> : MagicalizedControllerBase<TModel, TDto, TFilter>
  where TModel : class, IModel, new()
  where TDto : class, IDto<TModel>, new()
  where TFilter : class, IFilter, new()
{
  /// <summary>
  /// Service of the models.
  /// </summary>
  protected readonly IService<TKey, TModel, TFilter> service;

  /// <summary>
  /// Creates a controller.
  /// </summary>
  /// <param name="service">Service of the models.</param>
  public MagicalizedController(IService<TKey, TModel, TFilter> service)
    : base(service)
  {
    this.service = service;
  }

  /// <summary>
  /// Gets the DTO by the primary key.
  /// </summary>
  /// <param name="id">Primary key value.</param>
  /// <param name="include">
  /// Inclusions, passed as comma-separated property paths of related models (e.g. "category,photos"), each of up to
  /// <see cref="MagicalizerOptions.MaxInclusionDepth"/> properties.
  /// </param>
  /// <param name="cancellationToken">Token to cancel the request.</param>
  /// <returns>DTO, or <c>NotFound</c> if it doesn't exist.</returns>
  [HttpGet("{id}")]
  public virtual Task<ActionResult<TDto>> GetAsync(TKey id, [FromQuery] IEnumerable<IInclusion<TModel>>? include = null, CancellationToken cancellationToken = default) =>
    this.GetAsync(() => this.service.GetByIdAsync(id, include, cancellationToken));

  /// <summary>
  /// Partially updates the model by the primary key with a JSON patch.
  /// </summary>
  /// <param name="id">Primary key value.</param>
  /// <param name="dtoPatch">JSON patch of the DTO.</param>
  /// <param name="cancellationToken">Token to cancel the request.</param>
  /// <returns><c>NoContent</c> if the model is updated.</returns>
  [HttpPatch("{id}")]
  public virtual Task<IActionResult> PatchAsync(TKey id, [FromBody] JsonPatchDocument<TDto> dtoPatch, CancellationToken cancellationToken = default) =>
    this.PatchAsync(() => this.service.GetByIdAsync(id, cancellationToken: cancellationToken), dtoPatch, cancellationToken);

  /// <summary>
  /// Deletes the model by the primary key.
  /// </summary>
  /// <param name="id">Primary key value.</param>
  /// <param name="cancellationToken">Token to cancel the request.</param>
  /// <returns><c>NoContent</c> if the model is deleted.</returns>
  [HttpDelete("{id}")]
  public virtual Task<IActionResult> DeleteAsync(TKey id, CancellationToken cancellationToken = default) =>
    this.DeleteAsync(() => this.service.DeleteAsync(id, cancellationToken));

  private protected override object GetRouteValues(object?[] keyValues) =>
    new { id = keyValues[0] };
}
