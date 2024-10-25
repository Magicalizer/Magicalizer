// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Domain.Models.Abstractions;

namespace Magicalizer.Domain.Services.Abstractions;

/// <summary>
/// Defines a sorting rule, specifying the property path and sort direction.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
public interface ISorting<TModel> where TModel : class, IModel
{
  /// <summary>
  /// Indicates if sorting is ascending (true) or descending (false).
  /// </summary>
  bool IsAscending { get; }

  /// <summary>
  /// The property path for sorting (e.g., "Category.Name").
  /// </summary>
  string PropertyPath { get; }
}