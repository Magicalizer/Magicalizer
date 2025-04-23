// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Domain.Models.Abstractions;

namespace Magicalizer.Domain.Services.Abstractions;

/// <summary>
/// Defines an inclusion rule, specifying the property path of related models to include in a query.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
public interface IInclusion<TModel> where TModel : class, IModel
{
  /// <summary>
  /// The property path for including related models (e.g., "Category.Products").
  /// </summary>
  string PropertyPath { get; }
}