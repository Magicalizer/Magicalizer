// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Domain.Models.Abstractions;

namespace Magicalizer.Api.Dto.Abstractions;

/// <summary>
/// Base interface for a Data Transfer Object (DTO).
/// </summary>
public interface IDto
{
}

/// <summary>
/// Base interface for a DTO that can be mapped to a model.
/// </summary>
/// <typeparam name="TModel">The model type that this DTO maps to.</typeparam>
public interface IDto<TModel> : IDto where TModel : class, IModel
{
  /// <summary>
  /// Converts the DTO to its corresponding model.
  /// </summary>
  /// <returns>The corresponding model.</returns>
  TModel ToModel();
}
