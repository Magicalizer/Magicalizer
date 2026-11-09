// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Microsoft.AspNetCore.Http;

namespace Magicalizer.Api;

/// <summary>
/// Preferences of the client from the <c>Prefer</c> header (RFC 7240), e.g. <c>Prefer: count=none</c>. The server may
/// ignore them, and it reports the ones it has applied in the <c>Preference-Applied</c> header.
/// </summary>
internal static class PreferHeader
{
  /// <summary>
  /// Name of the request header with the preferences.
  /// </summary>
  public const string Name = "Prefer";

  /// <summary>
  /// Name of the response header with the applied preferences.
  /// </summary>
  public const string AppliedName = "Preference-Applied";

  /// <summary>
  /// Preference whether to count all the matching models (the <c>Paging-Total-Count</c> header of a list).
  /// </summary>
  public const string Count = "count";

  /// <summary>
  /// Value of the <see cref="Count"/> preference to count the models.
  /// </summary>
  public const string CountExact = "exact";

  /// <summary>
  /// Value of the <see cref="Count"/> preference not to count the models.
  /// </summary>
  public const string CountNone = "none";

  /// <summary>
  /// Gets the value of the preference. The names are case-insensitive, the preferences are separated by commas (in one
  /// or more headers), and only the first one with the name counts. The parameters of a preference (after ";") aren't
  /// read.
  /// </summary>
  /// <param name="request">Request with the preferences.</param>
  /// <param name="name">Name of the preference.</param>
  /// <returns>
  /// Value of the preference without quotes (an empty one if it has no value), or <c>null</c> if there is no such one.
  /// </returns>
  public static string? GetValue(HttpRequest request, string name)
  {
    foreach (string? header in request.Headers[Name])
    {
      if (header == null) continue;

      foreach (string preference in header.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
      {
        string[] nameAndValue = preference.Split(';', 2)[0].Split('=', 2, StringSplitOptions.TrimEntries);

        if (nameAndValue[0].Equals(name, StringComparison.OrdinalIgnoreCase))
          return nameAndValue.Length == 2 ? nameAndValue[1].Trim('"') : string.Empty;
      }
    }

    return null;
  }
}
