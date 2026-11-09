// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Mapping;
using Microsoft.AspNetCore.Mvc;

namespace Magicalizer.Api.Controllers;

/// <summary>
/// Base of the controllers of models (the magicalized ones and the custom ones): converts models to DTOs and processes
/// the requests around the actions, so the actions only call the services:
/// <list type="bullet">
///   <item>
///     The requests are validated before the actions: the errors of the query parameter names
///     (<see cref="QueryStringValidationFilterAttribute"/>), their values (e.g. the property paths of the sortings and
///     inclusions are checked while they are bound) and the DTOs
///     (<see cref="FluentValidationFilterAttribute"/>) are collected, and <see cref="ApiControllerAttribute"/> returns
///     400 Bad Request with all of them.
///   </item>
///   <item>
///     The exceptions of the services are converted to 400, 404 or 409 responses
///     (<see cref="ModelExceptionFilterAttribute"/>).
///   </item>
/// </list>
/// The DTOs are created by their constructors that take the models, so a DTO only for reading (without
/// <see cref="IDto{TModel}"/>) is enough for a custom controller that doesn't create or update models.
/// </summary>
/// <typeparam name="TModel">Model type.</typeparam>
/// <typeparam name="TDto">DTO type.</typeparam>
[ApiController]
[QueryStringValidationFilter]
[FluentValidationFilter]
[ModelExceptionFilter]
public abstract class ControllerBase<TModel, TDto> : ControllerBase
  where TModel : class, IModel
  where TDto : class, IDto
{
  /// <summary>
  /// Converts the model to the DTO.
  /// </summary>
  /// <param name="model">Model to convert.</param>
  /// <returns>DTO, or <c>null</c> if the model is <c>null</c>.</returns>
  protected virtual TDto? ModelToDto(TModel? model)
  {
    return model == null ? null : ConstructorMapper<TModel, TDto>.Map(model);
  }
}
