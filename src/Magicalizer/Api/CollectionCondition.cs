// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.Api;

/// <summary>
/// Collection condition (<c>Any</c> or <c>None</c>) on the path of a query parameter (see <see cref="FilterPath"/>).
/// </summary>
/// <param name="Path">
/// Path of the condition with the indexes of the outer collections (e.g. "products.any[0].photos.any").
/// </param>
/// <param name="Index">Index of the condition in the list form, or <c>null</c> for the single form.</param>
internal readonly record struct CollectionCondition(string Path, int? Index);
