// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Api.Dto.Abstractions;

namespace Magicalizer.TestHost.Diagnostics;

/// <summary>
/// Magicalized DTO without a model (it doesn't implement <see cref="IDto{TModel}"/>), so Magicalizer skips it and
/// reports it on the start.
/// </summary>
[Magicalized("api/broken")]
public class BrokenDto : IDto
{
}
