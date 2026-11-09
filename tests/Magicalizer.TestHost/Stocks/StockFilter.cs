// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Filters.Abstractions;

namespace Magicalizer.TestHost.Stocks;

/// <summary>
/// Filter for stocks. Only the conditions that are set are applied.
/// </summary>
public class StockFilter : IFilter
{
  /// <summary>
  /// Filters by warehouse code.
  /// </summary>
  public StringFilter? WarehouseCode { get; set; }

  /// <summary>
  /// Filters by product ID.
  /// </summary>
  public IntegerFilter? ProductId { get; set; }

  /// <summary>
  /// Filters by batch number.
  /// </summary>
  public IntegerFilter? BatchNumber { get; set; }

  /// <summary>
  /// Filters by quantity.
  /// </summary>
  public IntegerFilter? Quantity { get; set; }
}
