// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;

namespace Magicalizer.IntegrationTests.Catalog;

/// <summary>
/// The data the catalog tests work with (see <see cref="CatalogTestsFactory"/>): the products by their numbers (1-6)
/// and the other objects by their names (e.g. "Pizza" for a category, "Hot" for a tag, "borscht-1" for a photo, without
/// the extension).
/// </summary>
public record CatalogDataSet(int[] ProductIds, Dictionary<string, int> OtherIds, Dictionary<string, Guid> Codes)
{
  /// <summary>
  /// Replaces "{1}"-"{6}" in the query with the product IDs, "{Pizza}", "{Hot}" etc. with the IDs of the other
  /// objects and "{code-1}"-"{code-6}" with the product codes.
  /// </summary>
  public string Resolve(string query)
  {
    for (int i = 0; i < this.ProductIds.Length; i++)
      query = query.Replace($"{{{i + 1}}}", this.ProductIds[i].ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);

    foreach (KeyValuePair<string, int> otherId in this.OtherIds)
      query = query.Replace($"{{{otherId.Key}}}", otherId.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);

    foreach (KeyValuePair<string, Guid> code in this.Codes)
      query = query.Replace($"{{{code.Key}}}", code.Value.ToString(), StringComparison.Ordinal);

    return query;
  }

  /// <summary>
  /// Returns the IDs of the products by their numbers (e.g. "1,3").
  /// </summary>
  public List<int> Products(string numbers)
  {
    return numbers.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(n => this.ProductIds[int.Parse(n, CultureInfo.InvariantCulture) - 1]).ToList();
  }

  /// <summary>
  /// Returns the IDs of the other objects by their keys (e.g. "Pizza,Soups").
  /// </summary>
  public List<int> Others(string keys)
  {
    return keys.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(k => this.OtherIds[k]).ToList();
  }
}
