// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Models.Abstractions;

namespace Magicalizer.TestHost.Stocks;

/// <summary>
/// Stock of a product batch in a warehouse.
/// </summary>
public class StockModel : IModel<StockEntity, StockFilter>
{
  /// <summary>
  /// Code of the warehouse.
  /// </summary>
  public string? WarehouseCode { get; set; }

  /// <summary>
  /// ID of the product.
  /// </summary>
  public int ProductId { get; set; }

  /// <summary>
  /// Number of the batch.
  /// </summary>
  public int BatchNumber { get; set; }

  /// <summary>
  /// Quantity in the warehouse.
  /// </summary>
  public int Quantity { get; set; }

  /// <summary>
  /// Creates an empty model.
  /// </summary>
  public StockModel() { }

  /// <summary>
  /// Creates a model from the entity.
  /// </summary>
  public StockModel(StockEntity stockEntity)
  {
    this.WarehouseCode = stockEntity.WarehouseCode;
    this.ProductId = stockEntity.ProductId;
    this.BatchNumber = stockEntity.BatchNumber;
    this.Quantity = stockEntity.Quantity;
  }

  /// <inheritdoc/>
  public StockEntity ToEntity()
  {
    return new StockEntity {
      WarehouseCode = this.WarehouseCode!,
      ProductId = this.ProductId,
      BatchNumber = this.BatchNumber,
      Quantity = this.Quantity
    };
  }
}
