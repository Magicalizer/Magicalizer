// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Entities.Abstractions;

namespace Magicalizer.TestHost.Stocks;

/// <summary>
/// Stock of a product batch in a warehouse (an entity with a composite primary key of three properties of different
/// types).
/// </summary>
public class StockEntity : IEntity<string, int, int>
{
  /// <summary>
  /// Code of the warehouse (the first primary key property).
  /// </summary>
  public string WarehouseCode { get; set; } = null!;

  /// <summary>
  /// ID of the product (the second primary key property).
  /// </summary>
  public int ProductId { get; set; }

  /// <summary>
  /// Number of the batch (the third primary key property).
  /// </summary>
  public int BatchNumber { get; set; }

  /// <summary>
  /// Quantity in the warehouse.
  /// </summary>
  public int Quantity { get; set; }
}
