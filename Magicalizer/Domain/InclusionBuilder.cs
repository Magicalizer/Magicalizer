// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Magicalizer.Domain.Models.Abstractions;

namespace Magicalizer.Domain;

/// <summary>
/// Builds an inclusion rule for related models.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
public class InclusionBuilder<TModel> : PropertyPathBuilder<TModel, Inclusion<TModel>> where TModel : class, IModel
{
  /// <summary>
  /// Initializes a new instance of the <see cref="InclusionBuilder{TModel}"/> class.
  /// </summary>
  public InclusionBuilder() : base(propertyPath => new Inclusion<TModel>(propertyPath))
  {
  }
}