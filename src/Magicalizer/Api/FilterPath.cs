// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Magicalizer.Extensions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.Api;

/// <summary>
/// Path of a query parameter in a filter (e.g. "category.name.contains" or "products.any[0].photos.isempty").
/// </summary>
internal static partial class FilterPath
{
  /// <summary>
  /// Parses the path in the filter. It must lead to a condition (or to a property that isn't a filter, e.g. a search
  /// text the service applies). An index is allowed only on the <see cref="IEnumerableFilter.Any"/> and
  /// <see cref="IEnumerableFilter.None"/> conditions, and the <see cref="IEnumerableFilter.IsEmpty"/> one is the last one.
  /// </summary>
  /// <param name="filterType">Type of the filter.</param>
  /// <param name="path">Path to parse.</param>
  /// <param name="collectionConditions">Collection conditions on the way.</param>
  /// <returns><c>true</c> if the path is valid.</returns>
  public static bool TryParse(Type filterType, string path, out List<CollectionCondition> collectionConditions)
  {
    string[] segments = path.Split('.');
    Type type = filterType;

    collectionConditions = [];

    for (int i = 0; i < segments.Length; i++)
    {
      Match match = Segment().Match(segments[i]);

      if (!match.Success || match.Groups[2].Success || FindBindableProperty(type, match.Groups[1].Value) is not PropertyInfo property)
        return false;

      bool isLast = i == segments.Length - 1;

      if (typeof(IEnumerableFilter).IsAssignableFrom(property.PropertyType))
      {
        if (isLast) return false;

        Match condition = Segment().Match(segments[++i]);

        if (!condition.Success) return false;

        string conditionName = condition.Groups[1].Value;

        if (conditionName.Equals(nameof(IEnumerableFilter.IsEmpty), StringComparison.OrdinalIgnoreCase))
          return !condition.Groups[2].Success && i == segments.Length - 1;

        if (!conditionName.Equals(nameof(IEnumerableFilter.Any), StringComparison.OrdinalIgnoreCase) && !conditionName.Equals(nameof(IEnumerableFilter.None), StringComparison.OrdinalIgnoreCase))
          return false;

        if (i == segments.Length - 1) return false;

        int? index = null;

        // The pattern already limits the index, but a path that can't be parsed is invalid rather than an error.
        if (condition.Groups[2].Success)
        {
          if (!int.TryParse(condition.Groups[2].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int parsedIndex))
            return false;

          index = parsedIndex;
        }

        collectionConditions.Add(new CollectionCondition(string.Join(".", segments.Take(i)) + "." + conditionName, index));
        type = property.PropertyType.GetElementFilterType();
      }

      else if (typeof(IFilter).IsAssignableFrom(property.PropertyType))
      {
        if (isLast) return false;

        if (!property.PropertyType.IsValueFilter())
        {
          type = property.PropertyType;
          continue;
        }

        // The condition of a value filter (e.g. "contains") is the last segment.
        Match condition = Segment().Match(segments[i + 1]);

        return i + 1 == segments.Length - 1 && condition.Success && !condition.Groups[2].Success && FindBindableProperty(property.PropertyType, condition.Groups[1].Value) != null;
      }

      else return isLast;
    }

    return false;
  }

  // A segment of a path: a property name with an optional index (e.g. "any[1]"). The index has ASCII digits only (\d
  // matches the other Unicode digits too) and no more than 4 of them, so it always fits into an int (a larger one is
  // useless anyway, as the conditions must go without gaps, see CollectionConditionValidator). The regex is generated
  // at compile time, as it's matched for each segment of each query parameter.
  [GeneratedRegex(@"^([A-Za-z_][A-Za-z0-9_]*)(?:\[([0-9]{1,4})\])?$", RegexOptions.CultureInvariant)]
  private static partial Regex Segment();

  // Finds the property of the filter type that can be bound (a writable one) by its name, case-insensitively.
  private static PropertyInfo? FindBindableProperty(Type type, string name)
  {
    PropertyInfo? property = type.GetPublicProperty(name);

    return property?.CanWrite == true ? property : null;
  }
}
