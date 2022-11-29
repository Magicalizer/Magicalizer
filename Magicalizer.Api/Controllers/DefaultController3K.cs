// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.AspNetCore;
using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Filters.Abstractions;
using Magicalizer.Validators.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Magicalizer.Api.Controllers
{
  public class DefaultController<TKey1, TKey2, TKey3, TModel, TDto, TFilter> : ControllerBase<TModel, TDto, TFilter>
    where TModel : class, IModel, new()
    where TDto : class, IDto, new()
    where TFilter : class, IFilter, new()
  {
    public DefaultController(IAuthorizationService authorizationService, IServiceResolver serviceResolver)
      : base(authorizationService, serviceResolver)
    {
    }

    [HttpGet("{id1}/{id2}/{id3}")]
    public virtual async Task<ActionResult<TDto>> GetAsync(TKey1 id1, TKey2 id2, TKey3 id3, string fields = null)
    {
      if (!(await this.ValidateAuthorizationRules(HttpMethod.Get)))
        return this.Forbid();

      TModel model = await this.GetService().GetByIdAsync(id1, id2, id3, fields?.Split(','));

      if (model == null)
        return this.NotFound();

      return this.ModelToDto(model);
    }

    [HttpGet]
    public virtual async Task<ActionResult<IEnumerable<TDto>>> GetAsync([FromQuery] TFilter filter = default, string sorting = null, int? offset = null, int? limit = null, string fields = null)
    {
      if (!string.IsNullOrEmpty(sorting) && sorting.StartsWith(' '))
        sorting = "+" + sorting.Substring(1);

      if (!(await this.ValidateAuthorizationRules(HttpMethod.Get)))
        return this.Forbid();

      this.Response.Headers["Paging-Total-Number"] = (await this.GetService().CountAsync(filter)).ToString();

      if (offset != null)
        this.Response.Headers["Paging-Offset"] = offset.ToString();

      if (limit != null)
        this.Response.Headers["Paging-Limit"] = limit.ToString();

      return (await this.GetService().GetAllAsync(filter, sorting, offset, limit, fields?.Split(','))).Select(m => this.ModelToDto(m)).ToList();
    }

    [HttpPost]
    public virtual async Task<ActionResult<TDto>> PostAsync([FromBody][CustomizeValidator(RuleSet = RuleSetName.DefaultCreate)] TDto dto)
    {
      if (!(await this.ValidateAuthorizationRules(HttpMethod.Post)))
        return this.Forbid();

      if (!this.ModelState.IsValid)
        return this.BadRequest(this.ModelState);

      if (!(dto is IDto<TModel>))
        return this.BadRequest();

      return this.CreatedAtAction("Get", this.ModelToDto(await this.GetService().CreateAsync((dto as IDto<TModel>).ToModel())));
    }

    [HttpPut]
    public virtual async Task<IActionResult> PutAsync([FromBody][CustomizeValidator(RuleSet = RuleSetName.DefaultEdit)] TDto dto)
    {
      if (!(await this.ValidateAuthorizationRules(HttpMethod.Post)))
        return this.Forbid();

      if (!this.ModelState.IsValid)
        return this.BadRequest(this.ModelState);

      if (!(dto is IDto<TModel>))
        return this.BadRequest();

      await this.GetService().EditAsync((dto as IDto<TModel>).ToModel());
      return this.NoContent();
    }

    [HttpPatch("{id1}/{id2}/{id3}")]
    public virtual async Task<IActionResult> PatchAsync(TKey1 id1, TKey2 id2, TKey3 id3, [FromBody] JsonPatchDocument<TDto> dtoPatch)
    {
      if (!(await this.ValidateAuthorizationRules(HttpMethod.Patch)))
        return this.Forbid();

      TModel model = await this.GetService().GetByIdAsync(id1, id2, id3);

      if (model == null)
        return this.NotFound();

      TDto dto = this.ModelToDto(model);

      dtoPatch.ApplyTo(dto);

      await this.GetService().EditAsync((dto as IDto<TModel>).ToModel());
      return this.NoContent();
    }

    [HttpDelete("{id1}/{id2}/{id3}")]
    public virtual async Task<IActionResult> DeleteAsync(TKey1 id1, TKey2 id2, TKey3 id3)
    {
      if (!(await this.ValidateAuthorizationRules(HttpMethod.Delete)))
        return this.Forbid();

      await this.GetService().DeleteAsync(id1, id2, id3);
      return this.NoContent();
    }

    protected virtual IService<TKey1, TKey2, TKey3, TModel, TFilter> GetService()
    {
      return this.serviceResolver.GetService<TKey1, TKey2, TKey3, TModel, TFilter>();
    }
  }
}