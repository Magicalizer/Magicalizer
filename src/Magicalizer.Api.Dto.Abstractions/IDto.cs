// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Models.Abstractions;

namespace Magicalizer.Api.Dto.Abstractions;

/// <summary>
/// Data transfer object (DTO).
/// </summary>
public interface IDto
{
}

/// <summary>
/// DTO that can be converted to a model.
/// </summary>
/// <typeparam name="TModel">Model type.</typeparam>
public interface IDto<TModel> : IDto where TModel : class, IModel
{
  /// <summary>
  /// Converts the DTO to the model.
  /// </summary>
  /// <returns>Model.</returns>
  TModel ToModel();
}
