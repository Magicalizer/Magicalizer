// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Magicalizer.Api.Dto.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Filters.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Magicalizer.Api.Controllers;

/// <summary>
/// Base controller for managing models, providing methods for DTO conversion.
/// The supported HTTP methods, authentication, and authorization are handled by the conventions
/// (<see cref="MagicalizedControllerHttpMethodConvention"/> and <see cref="MagicalizedControllerAuthorizationConvention"/>)
/// and the standard middleware before the actions are executed.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TDto">The DTO type.</typeparam>
/// <typeparam name="TFilter">The filter type.</typeparam>
[InvalidQueryExceptionFilter]
[ValidationExceptionFilter]
[FluentValidationFilter]
public abstract class ControllerBase<TModel, TDto, TFilter> : ControllerBase
  where TModel : class, IModel, new()
  where TDto : class, IDto, new()
  where TFilter : class, IFilter, new()
{
  private static readonly Func<TModel, TDto> mapper = CreateMapper();

  /// <summary>
  /// Converts the given model to its corresponding DTO.
  /// </summary>
  /// <param name="model">The model to convert.</param>
  /// <returns>The converted DTO, or <c>null</c> if the model is <c>null</c>.</returns>
  protected virtual TDto? ModelToDto(TModel? model)
  {
    return model == null ? null : mapper(model);
  }

  // Creates a compiled function that calls the DTO constructor that accepts the model.
  private static Func<TModel, TDto> CreateMapper()
  {
    ConstructorInfo? constructor = typeof(TDto).GetConstructor([typeof(TModel)]);

    if (constructor == null)
      throw new InvalidOperationException($"Type {typeof(TDto).Name} must have a constructor that accepts {typeof(TModel).Name}");

    ParameterExpression parameter = Expression.Parameter(typeof(TModel), "model");
    NewExpression @new = Expression.New(constructor, parameter);

    return Expression.Lambda<Func<TModel, TDto>>(@new, parameter).Compile();
  }
}
