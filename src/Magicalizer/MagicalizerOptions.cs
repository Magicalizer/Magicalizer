// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer;

/// <summary>
/// Options of Magicalizer (see <c>AddMagicalizer</c>).
/// </summary>
public class MagicalizerOptions
{
  /// <summary>
  /// Query parameters that the magicalized controllers allow even if their actions don't support them, as something
  /// else processes them (e.g. "culture" for the request localization). The names are case-insensitive.
  /// </summary>
  public ISet<string> IgnoredQueryParameters { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

  /// <summary>
  /// Maximum number of models the API returns for a request, or <c>null</c> if it isn't limited (the default). The
  /// limit of a request can't be greater, and a request without the limit gets this number of models.
  /// </summary>
  public int? MaxLimit { get; set; }

  /// <summary>
  /// Maximum number of properties in a property path of an inclusion (e.g. 2 allows "category.products", but not
  /// "category.products.photos"), or <c>null</c> if it isn't limited (the default).
  /// </summary>
  public int? MaxInclusionDepth { get; set; }

  /// <summary>
  /// Whether a list is returned with the number of all the matching models (the <c>Paging-Total-Count</c> header), which
  /// takes a separate database query (the default is <c>true</c>). A request overrides it with the <c>Prefer</c> header:
  /// <c>Prefer: count=none</c> skips the count, and <c>Prefer: count=exact</c> gets it.
  /// </summary>
  public bool CountTotalByDefault { get; set; } = true;
}
