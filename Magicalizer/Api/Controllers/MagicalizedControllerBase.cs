// Copyright © 2026 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.Domain;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Extensions;
using Magicalizer.Filters.Abstractions;
using Magicalizer.Validators.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;

namespace Magicalizer.Api.Controllers;

/// <summary>
/// Base class for the magicalized controllers. Contains the actions that don't depend on the primary key
/// (getting a list, creating, and updating), and the shared logic of the actions that do.
/// The key-specific actions are declared in the derived controllers, which differ only in the number of key properties.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TDto">The DTO type.</typeparam>
/// <typeparam name="TFilter">The filter type.</typeparam>
public abstract class MagicalizedControllerBase<TModel, TDto, TFilter> : ControllerBase<TModel, TDto, TFilter>
  where TModel : class, IModel, new()
  where TDto : class, IDto, new()
  where TFilter : class, IFilter, new()
{
  private readonly IService<TModel, TFilter> service;

  private protected MagicalizedControllerBase(IService<TModel, TFilter> modelService)
  {
    this.service = modelService;
  }

  /// <summary>
  /// Gets a list of DTOs based on filtering, sorting, pagination, and inclusion options.
  /// </summary>
  /// <param name="filter">The filter to apply to the query.</param>
  /// <param name="sort">The comma-separated sorting rules: property paths, prefixed with "-" for descending order (e.g. "name,-created").</param>
  /// <param name="offset">The number of models to skip.</param>
  /// <param name="limit">The maximum number of models to return.</param>
  /// <param name="include">The comma-separated property paths of the related models to include (e.g. "category,photos").</param>
  /// <param name="cancellationToken">The token to cancel the request.</param>
  /// <returns>The list of matching DTOs.</returns>
  [HttpGet]
  public virtual async Task<ActionResult<IEnumerable<TDto>>> GetAsync([FromQuery] TFilter? filter = default, string? sort = null, int? offset = null, int? limit = null, string? include = null, CancellationToken cancellationToken = default)
  {
    IEnumerable<TModel> models = await this.service.GetAllAsync(
      filter,
      sort.SplitByComma().Select(Sorting<TModel>.Parse).ToList(),
      offset,
      limit,
      include.SplitByComma().Select(i => new Inclusion<TModel>(i)).ToArray(),
      cancellationToken
    );

    this.Response.Headers["Paging-Total-Count"] = (await this.service.CountAsync(filter, cancellationToken)).ToString();

    if (offset != null)
      this.Response.Headers["Paging-Offset"] = offset.ToString();

    if (limit != null)
      this.Response.Headers["Paging-Limit"] = limit.ToString();

    return models.Select(m => this.ModelToDto(m)!).ToList();
  }

  /// <summary>
  /// Creates a new model from the provided DTO.
  /// </summary>
  /// <param name="dto">The DTO representing the model to create.</param>
  /// <param name="cancellationToken">The token to cancel the request.</param>
  /// <returns>The created DTO.</returns>
  [HttpPost]
  public virtual async Task<ActionResult<TDto>> PostAsync([FromBody][ValidationRuleSet(RuleSetName.DefaultCreate)] TDto dto, CancellationToken cancellationToken = default)
  {
    if (!this.ModelState.IsValid)
      return this.ValidationProblem(this.ModelState);

    if (dto is not IDto<TModel>)
      return this.BadRequest();

    TModel createdModel = await this.service.CreateAsync(((IDto<TModel>)dto).ToModel(), cancellationToken);
    TDto? createdDto = this.ModelToDto(createdModel);
    object?[] keyValues = this.service.GetKeyValues(createdModel);

    // The location is omitted if it can't be built (the GET method is not supported or the entity has no primary key).
    string? location = keyValues.Length == 0 ? null : this.Url.Action("Get", this.GetRouteValues(keyValues));

    return location == null ? this.StatusCode(StatusCodes.Status201Created, createdDto) : this.Created(location, createdDto);
  }

  /// <summary>
  /// Updates an existing model using the provided DTO.
  /// </summary>
  /// <param name="dto">The DTO representing the model to update.</param>
  /// <param name="cancellationToken">The token to cancel the request.</param>
  /// <returns>A <c>NoContent</c> result if the update is successful.</returns>
  [HttpPut]
  public virtual async Task<IActionResult> PutAsync([FromBody][ValidationRuleSet(RuleSetName.DefaultEdit)] TDto dto, CancellationToken cancellationToken = default)
  {
    if (!this.ModelState.IsValid)
      return this.ValidationProblem(this.ModelState);

    if (dto is not IDto<TModel>)
      return this.BadRequest();

    await this.service.EditAsync(((IDto<TModel>)dto).ToModel(), cancellationToken);
    return this.NoContent();
  }

  // Shared logic of the "get by primary key" actions. The service is called through the delegate,
  // so each derived controller just passes its own key values.
  private protected async Task<ActionResult<TDto>> GetAsync(Func<IInclusion<TModel>[], Task<TModel?>> getById, string? include)
  {
    TDto? dto = this.ModelToDto(await getById(include.SplitByComma().Select(i => new Inclusion<TModel>(i)).ToArray()));

    return dto == null ? this.NotFound() : dto;
  }

  // Shared logic of the "patch by primary key" actions.
  private protected async Task<IActionResult> PatchAsync(Func<Task<TModel?>> getById, JsonPatchDocument<TDto> dtoPatch, CancellationToken cancellationToken)
  {
    TDto? dto = this.ModelToDto(await getById());

    if (dto == null)
      return this.NotFound();

    dtoPatch.ApplyTo(dto);

    await this.service.EditAsync(((IDto<TModel>)dto).ToModel(), cancellationToken);
    return this.NoContent();
  }

  // Shared logic of the "delete by primary key" actions.
  private protected async Task<IActionResult> DeleteAsync(Func<Task<bool>> delete)
  {
    if (await delete())
      return this.NoContent();

    return this.Conflict();
  }

  // Gets the route values of the "get by primary key" action for the given primary key values
  // (to build the URL of a created model).
  private protected abstract object GetRouteValues(object?[] keyValues);
}