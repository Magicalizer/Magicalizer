// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Models.Abstractions;

namespace Magicalizer.TestHost.Diagnostics;

/// <summary>
/// Model without an entity and a filter, so it doesn't have a service (Magicalizer reports it on the start).
/// </summary>
public class UnservedModel : IModel
{
  /// <summary>
  /// Model ID.
  /// </summary>
  public int Id { get; set; }
}
