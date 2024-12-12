// Copyright © 2024 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentValidation.AspNetCore;
using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.Domain;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Extensions;
using Magicalizer.Filters.Abstractions;
using Magicalizer.Validators.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Magicalizer.Api.Controllers;

/// <summary>
/// Default controller for managing models with a single primary key.
/// </summary>
/// <typeparam name="TKey">The type of the primary key.</typeparam>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TDto">The DTO type.</typeparam>
/// <typeparam name="TFilter">The filter type.</typeparam>
public class DefaultController<TKey, TModel, TDto, TFilter> : ControllerBase<TModel, TDto, TFilter>
  where TModel : class, IModel, new()
  where TDto : class, IDto, new()
  where TFilter : class, IFilter, new()
{
  private readonly IService<TKey, TModel, TFilter> service;

  /// <summary>
  /// Initializes a new instance of the <see cref="DefaultController{TKey, TModel, TDto, TFilter}"/> class.
  /// </summary>
  /// <param name="authorizationService">The service for authorization checks.</param>
  /// <param name="service">The service for managing the model.</param>
  public DefaultController(IAuthorizationService authorizationService, IService<TKey, TModel, TFilter> service)
    : base(authorizationService)
  {
    this.service = service;
  }

  /// <summary>
  /// Gets a DTO by its primary key.
  /// </summary>
  /// <param name="id">The primary key of the model.</param>
  /// <param name="fields">Optional fields to include in the response.</param>
  /// <returns>The requested DTO or a <c>NotFound</c> result if it doesn't exist.</returns>
  [HttpGet("{id}")]
  public virtual async Task<ActionResult<TDto>> GetAsync(TKey id, string? fields = null)
  {
    ActionResult? validationResult = await this.ValidateRequestAsync(Dto.Abstractions.HttpMethod.Get);

    if (validationResult != null)
      return validationResult;

    TDto? dto = this.ModelToDto(await this.service.GetByIdAsync(id, fields.SplitByComma().Select(f => new Inclusion<TModel>(f)).ToArray()));

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
    ActionResult? validationResult = await this.ValidateRequestAsync(Dto.Abstractions.HttpMethod.Get);

    if (validationResult != null)
      return validationResult;

    if (!string.IsNullOrEmpty(sorting) && sorting.StartsWith(' '))
      sorting = "+" + sorting.Substring(1);

    this.Response.Headers["Paging-Total-Count"] = (await this.service.CountAsync(filter)).ToString();

    if (offset != null)
      this.Response.Headers["Paging-Offset"] = offset.ToString();

    if (limit != null)
      this.Response.Headers["Paging-Limit"] = limit.ToString();

    IEnumerable<TModel> models = await this.service.GetAllAsync(
      filter,
      sorting.SplitByComma().Select(Sorting<TModel>.Parse).ToList(),
      offset,
      limit,
      fields.SplitByComma().Select(f => new Inclusion<TModel>(f)).ToArray()
    );

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
    ActionResult? validationResult = await this.ValidateRequestAsync(Dto.Abstractions.HttpMethod.Post);

    if (validationResult != null)
      return validationResult;

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
    ActionResult? validationResult = await this.ValidateRequestAsync(Dto.Abstractions.HttpMethod.Put);

    if (validationResult != null)
      return validationResult;

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
  /// <param name="id">The primary key of the model to update.</param>
  /// <param name="dtoPatch">The JSON patch document containing the updates.</param>
  /// <returns>A <c>NoContent</c> result if the patch is successful.</returns>
  [HttpPatch("{id}")]
  public virtual async Task<IActionResult> PatchAsync(TKey id, [FromBody] JsonPatchDocument<TDto> dtoPatch)
  {
    ActionResult? validationResult = await this.ValidateRequestAsync(Dto.Abstractions.HttpMethod.Patch);

    if (validationResult != null)
      return validationResult;

    TDto? dto = this.ModelToDto(await this.service.GetByIdAsync(id));

    if (dto == null)
      return this.NotFound();

    dtoPatch.ApplyTo(dto);

    await this.service.EditAsync(((IDto<TModel>)dto).ToModel());
    return this.NoContent();
  }

  /// <summary>
  /// Deletes a model by its primary key.
  /// </summary>
  /// <param name="id">The primary key of the model to delete.</param>
  /// <returns>A <c>NoContent</c> result if the deletion is successful.</returns>
  [HttpDelete("{id}")]
  public virtual async Task<IActionResult> DeleteAsync(TKey id)
  {
    ActionResult? validationResult = await this.ValidateRequestAsync(Dto.Abstractions.HttpMethod.Delete);

    if (validationResult != null)
      return validationResult;

    await this.service.DeleteAsync(id);
    return this.NoContent();
  }
}