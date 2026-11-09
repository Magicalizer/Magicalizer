// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Filters.Abstractions;

namespace Magicalizer.UnitTests.Data;

/// <summary>
/// Filter of the test items.
/// </summary>
public class ItemFilter : IFilter
{
  public IntegerFilter? Id { get; set; }
  public ByteFilter? Byte { get; set; }
  public ShortFilter? Short { get; set; }
  public IntegerFilter? Integer { get; set; }
  public LongFilter? Long { get; set; }
  public FloatFilter? Float { get; set; }
  public DoubleFilter? Double { get; set; }
  public DecimalFilter? Decimal { get; set; }
  public IntegerFilter? NullableInteger { get; set; }
  public StringFilter? Name { get; set; }
  public BooleanFilter? Flag { get; set; }
  public DateTimeFilter? Date { get; set; }
  public DateTimeOffsetFilter? Timestamp { get; set; }
  public GuidFilter? Code { get; set; }
  public EnumFilter<Color>? Color { get; set; }
  public GroupFilter? Group { get; set; }
  public EnumerableFilter<ItemFilter>? Children { get; set; }

  [IgnoreFilter]
  public IntegerFilter? Ignored { get; set; }
}
