// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Filters.Abstractions;

/// <summary>
/// Filter of <see cref="string"/> properties. The strings are compared as the database compares them (its collation
/// decides, e.g., whether the case is ignored). Only the conditions that are set are applied.
/// </summary>
public class StringFilter : IFilter
{
  /// <summary>
  /// Matches null values if <c>true</c>, non-null ones if <c>false</c>.
  /// </summary>
  public bool? IsNull { get; set; }

  /// <summary>
  /// Matches values equal to this one.
  /// </summary>
  public new string? Equals { get; set; }

  /// <summary>
  /// Matches values not equal to this one.
  /// </summary>
  public string? NotEquals { get; set; }

  /// <summary>
  /// Matches values that contain this one.
  /// </summary>
  public string? Contains { get; set; }

  /// <summary>
  /// Matches values that start with this one.
  /// </summary>
  public string? StartsWith { get; set; }

  /// <summary>
  /// Matches values that end with this one.
  /// </summary>
  public string? EndsWith { get; set; }

  /// <summary>
  /// Matches values equal to any of these (the query parameter is repeated, e.g. "id.in=1&amp;id.in=2").
  /// </summary>
  public IEnumerable<string>? In { get; set; }

  /// <summary>
  /// Matches values not equal to any of these (the query parameter is repeated, e.g. "id.notin=1&amp;id.notin=2").
  /// </summary>
  public IEnumerable<string>? NotIn { get; set; }

  /// <summary>
  /// Creates an empty filter.
  /// </summary>
  public StringFilter() { }

  /// <summary>
  /// Creates a filter with the given conditions.
  /// </summary>
  /// <param name="isNull">Matches null values if <c>true</c>, non-null ones if <c>false</c>.</param>
  /// <param name="equals">Matches values equal to this one.</param>
  /// <param name="notEquals">Matches values not equal to this one.</param>
  /// <param name="contains">Matches values that contain this one.</param>
  /// <param name="startsWith">Matches values that start with this one.</param>
  /// <param name="endsWith">Matches values that end with this one.</param>
  /// <param name="in">
  /// Matches values equal to any of these (the query parameter is repeated, e.g. "id.in=1&amp;id.in=2").
  /// </param>
  /// <param name="notIn">
  /// Matches values not equal to any of these (the query parameter is repeated, e.g. "id.notin=1&amp;id.notin=2").
  /// </param>
  public StringFilter(bool? isNull = null, string? equals = null, string? notEquals = null, string? contains = null, string? startsWith = null, string? endsWith = null, IEnumerable<string>? @in = null, IEnumerable<string>? notIn = null)
  {
    this.IsNull = isNull;
    this.Equals = equals;
    this.NotEquals = notEquals;
    this.Contains = contains;
    this.StartsWith = startsWith;
    this.EndsWith = endsWith;
    this.In = @in;
    this.NotIn = notIn;
  }
}
