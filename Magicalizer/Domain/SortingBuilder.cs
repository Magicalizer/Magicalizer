// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Domain.Models.Abstractions;

namespace Magicalizer.Domain;

/// <summary>
/// Builds a sorting rule for models.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
public class SortingBuilder<TModel> : PropertyPathBuilder<TModel, Sorting<TModel>> where TModel : class, IModel
{
  /// <summary>
  /// Initializes a new instance of the <see cref="SortingBuilder{TModel}"/> class.
  /// </summary>
  /// <param name="isAscending">Indicates if sorting is ascending.</param>
  public SortingBuilder(bool isAscending) : base(propertyPath => new Sorting<TModel>(isAscending, propertyPath))
  {
  }
}