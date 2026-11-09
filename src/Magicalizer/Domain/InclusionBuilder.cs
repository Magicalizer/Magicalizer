// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.PropertyPaths;

namespace Magicalizer.Domain;

/// <summary>
/// Builder of an inclusion of related models.
/// </summary>
/// <typeparam name="TModel">Model type.</typeparam>
public class InclusionBuilder<TModel> : PropertyPathBuilder<TModel, Inclusion<TModel>> where TModel : class, IModel
{
  /// <summary>
  /// Creates a builder.
  /// </summary>
  public InclusionBuilder() : base(propertyPath => new Inclusion<TModel>(propertyPath)) { }
}
