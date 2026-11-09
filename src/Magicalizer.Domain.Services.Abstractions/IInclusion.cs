// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Models.Abstractions;

namespace Magicalizer.Domain.Services.Abstractions;

/// <summary>
/// Inclusion of related models into a query.
/// </summary>
/// <typeparam name="TModel">Model type.</typeparam>
public interface IInclusion<TModel> where TModel : class, IModel
{
  /// <summary>
  /// Property path of related models to include (e.g. "Category.Products").
  /// </summary>
  string PropertyPath { get; }
}
