// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Services.Abstractions;

namespace Magicalizer.TestHost.Products;

/// <summary>
/// Custom service interface of products, so the service can be injected by it too.
/// </summary>
public interface IProductService : IService<int, ProductModel, ProductFilter>
{
}
