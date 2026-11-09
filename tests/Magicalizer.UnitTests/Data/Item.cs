// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Entities.Abstractions;

namespace Magicalizer.UnitTests.Data;

/// <summary>
/// Test entity with the properties of all the types the filters support.
/// </summary>
public class Item : IEntity<int>
{
  public int Id { get; set; }
  public byte Byte { get; set; }
  public short Short { get; set; }
  public int Integer { get; set; }
  public long Long { get; set; }
  public float Float { get; set; }
  public double Double { get; set; }
  public decimal Decimal { get; set; }
  public int? NullableInteger { get; set; }
  public string? Name { get; set; }
  public bool Flag { get; set; }
  public DateTime Date { get; set; }
  public DateTimeOffset Timestamp { get; set; }
  public Guid Code { get; set; }
  public Color Color { get; set; }
  public Group? Group { get; set; }
  public ICollection<Item>? Children { get; set; }
}
