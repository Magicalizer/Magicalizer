// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Domain.Models.Abstractions;

namespace Magicalizer.Api.Dto.Abstractions
{
  /// <summary>
  /// Describes a DTO.
  /// </summary>
  public interface IDto
  {
  }

  /// <summary>
  /// Describes a DTO.
  /// </summary>
  /// <typeparam name="TModel">A model type this DTO is used for.</typeparam>
  public interface IDto<TModel> : IDto where TModel : class, IModel
  {
    /// <summary>
    /// Creates a model and maps current DTO on it.
    /// </summary>
    /// <returns></returns>
    TModel ToModel();
  }
}