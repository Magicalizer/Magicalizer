// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Linq.Expressions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Extensions;

namespace Magicalizer.Domain;

/// <summary>
/// Sorting of a model query by a property.
/// </summary>
/// <typeparam name="TModel">Model type.</typeparam>
public class Sorting<TModel> : ISorting<TModel> where TModel : class, IModel
{
  /// <inheritdoc/>
  public bool IsAscending { get; }

  /// <inheritdoc/>
  public string PropertyPath { get; }

  /// <summary>
  /// Creates a sorting from the expression.
  /// </summary>
  /// <param name="isAscending">Whether the sorting is ascending (otherwise descending).</param>
  /// <param name="property">Expression of the property path.</param>
  public Sorting(bool isAscending, Expression<Func<TModel, object?>> property)
  {
    this.IsAscending = isAscending;
    this.PropertyPath = property.GetPropertyPath();
  }

  /// <summary>
  /// Creates a sorting from the property path.
  /// </summary>
  /// <param name="isAscending">Whether the sorting is ascending (otherwise descending).</param>
  /// <param name="propertyPath">Property path (property names separated by dots).</param>
  public Sorting(bool isAscending, string propertyPath)
  {
    this.IsAscending = isAscending;
    this.PropertyPath = propertyPath;
  }

  /// <summary>
  /// Parses the sorting from a string.
  /// </summary>
  /// <param name="value">
  /// Property path to sort by, prefixed with "-" for descending order (e.g. "name" or "-created").
  /// </param>
  /// <returns>Sorting.</returns>
  public static Sorting<TModel> Parse(string value)
  {
    // The spaces around the value are ignored (e.g. in "name, -created").
    value = value.Trim();

    if (value.Length == 0)
      return new Sorting<TModel>(true, value);

    return value[0] == '-' ? new Sorting<TModel>(false, value[1..]) : new Sorting<TModel>(true, value);
  }
}
