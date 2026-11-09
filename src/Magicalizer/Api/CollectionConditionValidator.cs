// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Api;

/// <summary>
/// Validates the forms of the collection conditions of a query: the list form (e.g. "products.any[0].name") must have
/// the indexes from 0 without gaps and can't be combined with the single form (e.g. "products.any.name").
/// </summary>
internal sealed class CollectionConditionValidator
{
  private readonly Dictionary<string, Usage> usages = new Dictionary<string, Usage>(StringComparer.OrdinalIgnoreCase);

  /// <summary>
  /// Adds the collection conditions of a query parameter.
  /// </summary>
  /// <param name="collectionConditions">Collection conditions of the query parameter.</param>
  public void Add(IEnumerable<CollectionCondition> collectionConditions)
  {
    foreach ((string path, int? index) in collectionConditions)
    {
      if (!this.usages.TryGetValue(path, out Usage? usage))
        this.usages[path] = usage = new Usage();

      if (index == null)
        usage.HasSingleForm = true;

      else usage.Indexes.Add(index.Value);
    }
  }

  /// <summary>
  /// Validates the added collection conditions.
  /// </summary>
  /// <returns>Errors by the paths of the invalid collection conditions.</returns>
  public IEnumerable<KeyValuePair<string, string>> Validate()
  {
    foreach (KeyValuePair<string, Usage> usage in this.usages)
    {
      if (usage.Value.HasSingleForm && usage.Value.Indexes.Count != 0)
        yield return new KeyValuePair<string, string>(usage.Key, "The single and list forms of the collection condition can't be combined.");

      else if (usage.Value.Indexes.Count != 0 && usage.Value.Indexes.Max() != usage.Value.Indexes.Count - 1)
        yield return new KeyValuePair<string, string>(usage.Key, "The indexes of the collection condition must start from 0 and go without gaps.");
    }
  }

  // Forms of the conditions of a collection in the query.
  private sealed class Usage
  {
    public bool HasSingleForm { get; set; }
    public HashSet<int> Indexes { get; } = [];
  }
}
