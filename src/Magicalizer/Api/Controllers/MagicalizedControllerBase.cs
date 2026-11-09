// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Filters.Abstractions;
using Magicalizer.Validators.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using RangeAttribute = System.ComponentModel.DataAnnotations.RangeAttribute;

namespace Magicalizer.Api.Controllers;

/// <summary>
/// Base of the magicalized controllers. Contains the actions that don't depend on the primary key (getting a list,
/// creating and updating) and shared logic of the ones that do. Actions by the primary key are declared in the derived
/// controllers, which differ only in the number of primary key properties. When the app starts, the conventions of
/// Magicalizer set the route, the supported HTTP methods and the settings of the API description (by
/// <see cref="MagicalizedAttribute"/> of the DTO), the authorization (by <see cref="AuthenticatedOnlyAttribute"/> and
/// <see cref="AuthorizedOnlyAttribute"/>) and the maximum limit (<see cref="MagicalizerOptions.MaxLimit"/>) of these
/// controllers and of the custom ones derived from them.
/// </summary>
/// <typeparam name="TModel">Model type.</typeparam>
/// <typeparam name="TDto">DTO type.</typeparam>
/// <typeparam name="TFilter">Filter type.</typeparam>
[Route(MagicalizedControllerRouteConvention.RouteTemplate)]
public abstract class MagicalizedControllerBase<TModel, TDto, TFilter> : ControllerBase<TModel, TDto>
  where TModel : class, IModel, new()
  where TDto : class, IDto<TModel>, new()
  where TFilter : class, IFilter, new()
{
  private readonly IService<TModel, TFilter> service;

  // Only the controllers of this assembly derive from the class directly, the custom ones derive from the controllers
  // with the keys.
  private protected MagicalizedControllerBase(IService<TModel, TFilter> service)
  {
    this.service = service;
  }

  /// <summary>
  /// Gets the DTOs that match the filter, sorted and paged, with related models included.
  /// </summary>
  /// <param name="filter">Filter to apply.</param>
  /// <param name="sort">
  /// Sortings, passed as comma-separated property paths, prefixed with "-" for descending order (e.g. "name,-created").
  /// </param>
  /// <param name="offset">Number of models to skip.</param>
  /// <param name="limit">
  /// Maximum number of models to return, up to <see cref="MagicalizerOptions.MaxLimit"/> (it's also the default one).
  /// </param>
  /// <param name="include">
  /// Inclusions, passed as comma-separated property paths of related models (e.g. "category,photos"), each of up to
  /// <see cref="MagicalizerOptions.MaxInclusionDepth"/> properties.
  /// </param>
  /// <param name="cancellationToken">Token to cancel the request.</param>
  /// <returns>
  /// DTOs that match the filter. The number of all of them is returned in the <c>Paging-Total-Count</c> header if
  /// <see cref="MagicalizerOptions.CountTotalByDefault"/> is set or the request asks for it with the
  /// <c>Prefer: count=exact</c> header (<c>Prefer: count=none</c> skips it).
  /// </returns>
  [HttpGet]
  public virtual async Task<ActionResult<IEnumerable<TDto>>> GetAsync([FromQuery] TFilter? filter = null, [FromQuery] IEnumerable<ISorting<TModel>>? sort = null, [Range(0, int.MaxValue)] int? offset = null, [Range(0, int.MaxValue)] int? limit = null, [FromQuery] IEnumerable<IInclusion<TModel>>? include = null, CancellationToken cancellationToken = default)
  {
    IEnumerable<TModel> models = await this.service.GetAllAsync(filter, sort, offset, limit, include, cancellationToken);
    string? countPreference = this.GetCountPreference();

    // The response depends on the preference, so the caches must keep the responses apart.
    this.Response.Headers.Append(HeaderNames.Vary, PreferHeader.Name);

    if (countPreference != null)
      this.Response.Headers.Append(PreferHeader.AppliedName, $"{PreferHeader.Count}={countPreference}");

    bool isTotalCounted = countPreference == null ?
      this.HttpContext.RequestServices.GetRequiredService<IOptions<MagicalizerOptions>>().Value.CountTotalByDefault :
      countPreference == PreferHeader.CountExact;

    if (isTotalCounted)
      this.Response.Headers["Paging-Total-Count"] = (await this.service.CountAsync(filter, cancellationToken)).ToString();

    if (offset != null)
      this.Response.Headers["Paging-Offset"] = offset.ToString();

    if (limit != null)
      this.Response.Headers["Paging-Limit"] = limit.ToString();

    return models.Select(m => this.ModelToDto(m)!).ToList();
  }

  /// <summary>
  /// Creates a model from the DTO.
  /// </summary>
  /// <param name="dto">DTO of the model to create.</param>
  /// <param name="cancellationToken">Token to cancel the request.</param>
  /// <returns>Created DTO.</returns>
  [HttpPost]
  public virtual async Task<ActionResult<TDto>> PostAsync([FromBody][ValidationRuleSet(RuleSetName.DefaultCreate)] TDto dto, CancellationToken cancellationToken = default)
  {
    TModel createdModel = await this.service.CreateAsync(dto.ToModel(), cancellationToken);
    TDto? createdDto = this.ModelToDto(createdModel);
    object?[] keyValues = this.service.GetKeyValues(createdModel);

    // The location is omitted if it can't be built (the GET method isn't supported or the entity has no primary key).
    string? location = keyValues.Length == 0 ? null : this.Url.Action("Get", this.GetRouteValues(keyValues));

    return location == null ? this.StatusCode(StatusCodes.Status201Created, createdDto) : this.Created(location, createdDto);
  }

  /// <summary>
  /// Updates the model from the DTO.
  /// </summary>
  /// <param name="dto">DTO of the model to update.</param>
  /// <param name="cancellationToken">Token to cancel the request.</param>
  /// <returns><c>NoContent</c> if the model is updated.</returns>
  [HttpPut]
  public virtual async Task<IActionResult> PutAsync([FromBody][ValidationRuleSet(RuleSetName.DefaultEdit)] TDto dto, CancellationToken cancellationToken = default)
  {
    await this.service.EditAsync(dto.ToModel(), cancellationToken);
    return this.NoContent();
  }

  // Shared logic of the "get by primary key" actions. The service is called through the delegate, so each derived
  // controller just passes its own key values.
  private protected async Task<ActionResult<TDto>> GetAsync(Func<Task<TModel?>> getById)
  {
    TDto? dto = this.ModelToDto(await getById());

    return dto == null ? this.NotFound() : dto;
  }

  // Shared logic of the "patch by primary key" actions.
  private protected async Task<IActionResult> PatchAsync(Func<Task<TModel?>> getById, JsonPatchDocument<TDto> dtoPatch, CancellationToken cancellationToken)
  {
    TModel? model = await getById();
    TDto? dto = this.ModelToDto(model);

    if (model == null || dto == null)
      return this.NotFound();

    object?[] keyValues = this.service.GetKeyValues(model);

    // An operation that can't be applied (e.g. with an unknown path) is a client error.
    dtoPatch.ApplyTo(dto, error => this.ModelState.AddModelError(error.Operation?.path ?? string.Empty, error.ErrorMessage));

    // The patch can't change the primary key, as the other model with this key would be edited instead.
    if (this.ModelState.IsValid && !keyValues.SequenceEqual(this.service.GetKeyValues(dto.ToModel())))
      this.ModelState.AddModelError(string.Empty, "The primary key can't be changed.");

    if (!this.ModelState.IsValid)
      return this.ValidationProblem(this.ModelState);

    // The patched DTO is validated as the updated one (see PutAsync), so the patch can't bypass the validation.
    await FluentValidationFilterAttribute.ValidateAsync(this.HttpContext.RequestServices, typeof(TDto), dto, [RuleSetName.Default, RuleSetName.Edit], this.ModelState, cancellationToken);

    if (!this.ModelState.IsValid)
      return this.ValidationProblem(this.ModelState);

    await this.service.EditAsync(dto.ToModel(), cancellationToken);
    return this.NoContent();
  }

  // Shared logic of the "delete by primary key" actions.
  private protected async Task<IActionResult> DeleteAsync(Func<Task> delete)
  {
    await delete();
    return this.NoContent();
  }

  // Gets route values of the "get by primary key" action for the primary key values (to build the URL of a created model).
  private protected abstract object GetRouteValues(object?[] keyValues);

  // Gets the count preference of the request (see PreferHeader) in lower case if it's a supported one ("exact" or
  // "none"), otherwise null (the server ignores the preferences it doesn't support).
  private string? GetCountPreference()
  {
    string? countPreference = PreferHeader.GetValue(this.Request, PreferHeader.Count)?.ToLowerInvariant();

    return countPreference is PreferHeader.CountExact or PreferHeader.CountNone ? countPreference : null;
  }
}

