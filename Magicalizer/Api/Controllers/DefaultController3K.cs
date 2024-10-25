﻿// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentValidation.AspNetCore;
using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Extensions;
using Magicalizer.Filters.Abstractions;
using Magicalizer.Validators.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Magicalizer.Api.Controllers;

/// <summary>
/// Default controller for managing models with a composite primary key consisting of three properties.
/// </summary>
/// <typeparam name="TKey1">The type of the first key.</typeparam>
/// <typeparam name="TKey2">The type of the second key.</typeparam>
/// <typeparam name="TKey3">The type of the third key.</typeparam>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TDto">The DTO type.</typeparam>
/// <typeparam name="TFilter">The filter type.</typeparam>
public class DefaultController<TKey1, TKey2, TKey3, TModel, TDto, TFilter> : ControllerBase<TModel, TDto, TFilter>
  where TModel : class, IModel, new()
  where TDto : class, IDto, new()
  where TFilter : class, IFilter, new()
{
  private readonly IService<TKey1, TKey2, TKey3, TModel, TFilter> service;

  /// <summary>
  /// Initializes a new instance of the <see cref="DefaultController{TKey1, TKey2, TKey3, TModel, TDto, TFilter}"/> class.
  /// </summary>
  /// <param name="authorizationService">The service for authorization checks.</param>
  /// <param name="service">The service for managing the model.</param>
  public DefaultController(IAuthorizationService authorizationService, IService<TKey1, TKey2, TKey3, TModel, TFilter> service)
    : base(authorizationService)
  {
    this.service = service;
  }

  /// <summary>
  /// Gets a DTO by its composite primary key.
  /// </summary>
  /// <param name="id1">The first key.</param>
  /// <param name="id2">The second key.</param>
  /// <param name="id3">The third key.</param>
  /// <param name="fields">Optional fields to include in the response.</param>
  /// <returns>The requested DTO or a <c>NotFound</c> result if it doesn't exist.</returns>
  [HttpGet("{id1}/{id2}/{id3}")]
  public virtual async Task<ActionResult<TDto>> GetAsync(TKey1 id1, TKey2 id2, TKey3 id3, string? fields = null)
  {
    if (!this.ValidateHttpMethodSupport(Dto.Abstractions.HttpMethod.Get))
      return this.StatusCode(StatusCodes.Status405MethodNotAllowed);

    if (!await this.ValidateAuthorizationRulesAsync(Dto.Abstractions.HttpMethod.Get))
      return this.Forbid();

    TDto? dto = this.ModelToDto(await this.service.GetByIdAsync(id1, id2, id3, fields.SplitByComma()));

    return dto == null ? this.NotFound() : dto;
  }

  /// <summary>
  /// Gets a list of DTOs based on filtering, sorting, pagination, and inclusion options.
  /// </summary>
  /// <param name="filter">The filter to apply to the query.</param>
  /// <param name="sorting">The sorting criteria.</param>
  /// <param name="offset">The number of models to skip.</param>
  /// <param name="limit">The maximum number of models to return.</param>
  /// <param name="fields">Optional fields to include in the response.</param>
  /// <returns>The list of matching DTOs.</returns>
  [HttpGet]
  public virtual async Task<ActionResult<IEnumerable<TDto>>> GetAsync([FromQuery] TFilter? filter = default, string? sorting = null, int? offset = null, int? limit = null, string? fields = null)
  {
    if (!this.ValidateHttpMethodSupport(Dto.Abstractions.HttpMethod.Get))
      return this.StatusCode(StatusCodes.Status405MethodNotAllowed);

    if (!await this.ValidateAuthorizationRulesAsync(Dto.Abstractions.HttpMethod.Get))
      return this.Forbid();

    if (!string.IsNullOrEmpty(sorting) && sorting.StartsWith(' '))
      sorting = "+" + sorting.Substring(1);

    this.Response.Headers["Paging-Total-Number"] = (await this.service.CountAsync(filter)).ToString();

    if (offset != null)
      this.Response.Headers["Paging-Offset"] = offset.ToString();

    if (limit != null)
      this.Response.Headers["Paging-Limit"] = limit.ToString();

    IEnumerable<TModel> models = filter == null
      ? await this.service.GetAllAsync(sorting.SplitByComma(), offset, limit, fields.SplitByComma())
      : await this.service.GetFilteredAsync(filter, sorting.SplitByComma(), offset, limit, fields.SplitByComma());

    return models.Select(m => this.ModelToDto(m)!).ToList();
  }

  /// <summary>
  /// Creates a new model from the provided DTO.
  /// </summary>
  /// <param name="dto">The DTO representing the model to create.</param>
  /// <returns>The created DTO.</returns>
  [HttpPost]
  public virtual async Task<ActionResult<TDto>> PostAsync([FromBody][CustomizeValidator(RuleSet = RuleSetName.DefaultCreate)] TDto dto)
  {
    if (!this.ValidateHttpMethodSupport(Dto.Abstractions.HttpMethod.Post))
      return this.StatusCode(StatusCodes.Status405MethodNotAllowed);

    if (!await this.ValidateAuthorizationRulesAsync(Dto.Abstractions.HttpMethod.Post))
      return this.Forbid();

    if (!this.ModelState.IsValid)
      return this.BadRequest(this.ModelState);

    if (dto is not IDto<TModel>)
      return this.BadRequest();

    return this.CreatedAtAction("Get", this.ModelToDto(await this.service.CreateAsync(((IDto<TModel>)dto).ToModel())));
  }

  /// <summary>
  /// Updates an existing model using the provided DTO.
  /// </summary>
  /// <param name="dto">The DTO representing the model to update.</param>
  /// <returns>A <c>NoContent</c> result if the update is successful.</returns>
  [HttpPut]
  public virtual async Task<IActionResult> PutAsync([FromBody][CustomizeValidator(RuleSet = RuleSetName.DefaultEdit)] TDto dto)
  {
    if (!this.ValidateHttpMethodSupport(Dto.Abstractions.HttpMethod.Put))
      return this.StatusCode(StatusCodes.Status405MethodNotAllowed);

    if (!await this.ValidateAuthorizationRulesAsync(Dto.Abstractions.HttpMethod.Put))
      return this.Forbid();

    if (!this.ModelState.IsValid)
      return this.BadRequest(this.ModelState);

    if (dto is not IDto<TModel>)
      return this.BadRequest();

    await this.service.EditAsync(((IDto<TModel>)dto).ToModel());
    return this.NoContent();
  }

  /// <summary>
  /// Partially updates a model using a JSON patch document.
  /// </summary>
  /// <param name="id1">The first key of the model to update.</param>
  /// <param name="id2">The second key of the model to update.</param>
  /// <param name="id3">The third key of the model to update.</param>
  /// <param name="dtoPatch">The JSON patch document containing the updates.</param>
  /// <returns>A <c>NoContent</c> result if the patch is successful.</returns>
  [HttpPatch("{id1}/{id2}/{id3}")]
  public virtual async Task<IActionResult> PatchAsync(TKey1 id1, TKey2 id2, TKey3 id3, [FromBody] JsonPatchDocument<TDto> dtoPatch)
  {
    if (!this.ValidateHttpMethodSupport(Dto.Abstractions.HttpMethod.Patch))
      return this.StatusCode(StatusCodes.Status405MethodNotAllowed);

    if (!await this.ValidateAuthorizationRulesAsync(Dto.Abstractions.HttpMethod.Patch))
      return this.Forbid();

    TDto? dto = this.ModelToDto(await this.service.GetByIdAsync(id1, id2, id3));

    if (dto == null)
      return this.NotFound();

    dtoPatch.ApplyTo(dto);

    await this.service.EditAsync(((IDto<TModel>)dto).ToModel());
    return this.NoContent();
  }

  /// <summary>
  /// Deletes a model by its composite primary key.
  /// </summary>
  /// <param name="id1">The first key of the model to delete.</param>
  /// <param name="id2">The second key of the model to delete.</param>
  /// <param name="id3">The third key of the model to delete.</param>
  /// <returns>A <c>NoContent</c> result if the deletion is successful.</returns>
  [HttpDelete("{id1}/{id2}/{id3}")]
  public virtual async Task<IActionResult> DeleteAsync(TKey1 id1, TKey2 id2, TKey3 id3)
  {
    if (!this.ValidateHttpMethodSupport(Dto.Abstractions.HttpMethod.Delete))
      return this.StatusCode(StatusCodes.Status405MethodNotAllowed);

    if (!(await this.ValidateAuthorizationRulesAsync(Dto.Abstractions.HttpMethod.Delete)))
      return this.Forbid();

    await this.service.DeleteAsync(id1, id2, id3);
    return this.NoContent();
  }
}