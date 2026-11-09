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
/// Magicalized controller of models with a composite primary key of two properties.
/// </summary>
/// <typeparam name="TKey1">Type of the first primary key property.</typeparam>
/// <typeparam name="TKey2">Type of the second primary key property.</typeparam>
/// <typeparam name="TModel">Model type.</typeparam>
/// <typeparam name="TDto">DTO type.</typeparam>
/// <typeparam name="TFilter">Filter type.</typeparam>
public class MagicalizedController<TKey1, TKey2, TModel, TDto, TFilter> : MagicalizedControllerBase<TModel, TDto, TFilter>
  where TModel : class, IModel, new()
  where TDto : class, IDto<TModel>, new()
  where TFilter : class, IFilter, new()
{
  /// <summary>
  /// Service of the models.
  /// </summary>
  protected readonly IService<TKey1, TKey2, TModel, TFilter> service;

  /// <summary>
  /// Creates a controller.
  /// </summary>
  /// <param name="service">Service of the models.</param>
  public MagicalizedController(IService<TKey1, TKey2, TModel, TFilter> service)
    : base(service)
  {
    this.service = service;
  }

  /// <summary>
  /// Gets the DTO by the composite primary key.
  /// </summary>
  /// <param name="id1">Value of the first primary key property.</param>
  /// <param name="id2">Value of the second primary key property.</param>
  /// <param name="include">
  /// Inclusions, passed as comma-separated property paths of related models (e.g. "category,photos"), each of up to
  /// <see cref="MagicalizerOptions.MaxInclusionDepth"/> properties.
  /// </param>
  /// <param name="cancellationToken">Token to cancel the request.</param>
  /// <returns>DTO, or <c>NotFound</c> if it doesn't exist.</returns>
  [HttpGet("{id1}/{id2}")]
  public virtual Task<ActionResult<TDto>> GetAsync(TKey1 id1, TKey2 id2, [FromQuery] IEnumerable<IInclusion<TModel>>? include = null, CancellationToken cancellationToken = default) =>
    this.GetAsync(() => this.service.GetByIdAsync(id1, id2, include, cancellationToken));

  /// <summary>
  /// Partially updates the model by the composite primary key with a JSON patch.
  /// </summary>
  /// <param name="id1">Value of the first primary key property.</param>
  /// <param name="id2">Value of the second primary key property.</param>
  /// <param name="dtoPatch">JSON patch of the DTO.</param>
  /// <param name="cancellationToken">Token to cancel the request.</param>
  /// <returns><c>NoContent</c> if the model is updated.</returns>
  [HttpPatch("{id1}/{id2}")]
  public virtual Task<IActionResult> PatchAsync(TKey1 id1, TKey2 id2, [FromBody] JsonPatchDocument<TDto> dtoPatch, CancellationToken cancellationToken = default) =>
    this.PatchAsync(() => this.service.GetByIdAsync(id1, id2, cancellationToken: cancellationToken), dtoPatch, cancellationToken);

  /// <summary>
  /// Deletes the model by the composite primary key.
  /// </summary>
  /// <param name="id1">Value of the first primary key property.</param>
  /// <param name="id2">Value of the second primary key property.</param>
  /// <param name="cancellationToken">Token to cancel the request.</param>
  /// <returns><c>NoContent</c> if the model is deleted.</returns>
  [HttpDelete("{id1}/{id2}")]
  public virtual Task<IActionResult> DeleteAsync(TKey1 id1, TKey2 id2, CancellationToken cancellationToken = default) =>
    this.DeleteAsync(() => this.service.DeleteAsync(id1, id2, cancellationToken));

  private protected override object GetRouteValues(object?[] keyValues) =>
    new { id1 = keyValues[0], id2 = keyValues[1] };
}
