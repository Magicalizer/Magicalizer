// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using FluentValidation;
using Magicalizer.Api.Dto.Abstractions;

namespace Magicalizer.TestHost.Stocks;

/// <summary>
/// Stock of a product batch in a warehouse, served at "/api/stocks/{warehouseCode}/{productId}/{batchNumber}" to the
/// authenticated users only. It's hidden from the API description.
/// </summary>
[Magicalized("api/stocks", IgnoreApi = true)]
[AuthenticatedOnly]
public class StockDto : IDto<StockModel>
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
  /// Creates an empty DTO.
  /// </summary>
  public StockDto() { }

  /// <summary>
  /// Creates a DTO from the model.
  /// </summary>
  public StockDto(StockModel stockModel)
  {
    this.WarehouseCode = stockModel.WarehouseCode;
    this.ProductId = stockModel.ProductId;
    this.BatchNumber = stockModel.BatchNumber;
    this.Quantity = stockModel.Quantity;
  }

  /// <inheritdoc/>
  public StockModel ToModel()
  {
    return new StockModel {
      WarehouseCode = this.WarehouseCode,
      ProductId = this.ProductId,
      BatchNumber = this.BatchNumber,
      Quantity = this.Quantity
    };
  }

  /// <summary>
  /// Validator of the stock.
  /// </summary>
  public class Validator : AbstractValidator<StockDto>
  {
    /// <summary>
    /// Creates the validator.
    /// </summary>
    public Validator()
    {
      this.RuleFor(s => s.WarehouseCode).NotEmpty().MaximumLength(16);
      this.RuleFor(s => s.ProductId).GreaterThan(0);
      this.RuleFor(s => s.Quantity).GreaterThanOrEqualTo(0);
    }
  }
}
