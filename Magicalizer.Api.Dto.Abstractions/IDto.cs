// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Domain.Models.Abstractions;

namespace Magicalizer.Api.Dto.Abstractions
{
  public interface IDto
  {
  }

  public interface IDto<TModel> : IDto where TModel : class, IModel
  {
    TModel ToModel();
  }
}