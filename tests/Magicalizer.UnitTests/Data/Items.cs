// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

namespace Magicalizer.UnitTests.Data;

/// <summary>
/// Test items: 1-3 with the numbers equal to their IDs, the groups and the children.
/// </summary>
public static class Items
{
  /// <summary>
  /// Creates the items as a query in memory:
  ///   #  numbers  nullable  name     flag   date        code  group  children
  ///   1  1        1         Apple    true   2026-01-01  G1    A      2
  ///   2  2        -         banana   false  2026-01-02  G2    A      -
  ///   3  3        3         Cherry   true   2026-01-03  G3    B      1, 3
  /// The numbers are the IDs, and G1-G3 are the codes made of them (see <see cref="Code"/>).
  /// </summary>
  public static IQueryable<Item> Create()
  {
    Group a = new Group { Id = 1, Name = "A" };
    Group b = new Group { Id = 2, Name = "B" };
    Item[] items = [
      CreateItem(1, 1, "Apple", a),
      CreateItem(2, null, "banana", a),
      CreateItem(3, 3, "Cherry", b)
    ];

    items[0].Children = [items[1]];
    items[1].Children = [];
    items[2].Children = [items[0], items[2]];
    return items.AsQueryable();
  }

  /// <summary>
  /// Returns the code of the item.
  /// </summary>
  public static Guid Code(int id)
  {
    return new Guid(id, 0, 0, new byte[8]);
  }

  /// <summary>
  /// Returns the IDs of the items in the order of the query.
  /// </summary>
  public static DateTimeOffset Timestamp(int id)
  {
    // The offsets differ, so the filters compare the moments, not the clock times (the 2nd item is the latest one).
    return new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.FromHours(id == 2 ? -id : id));
  }

  public static string Ids(IQueryable<Item> items)
  {
    return string.Join(",", items.Select(i => i.Id));
  }

  private static Item CreateItem(int id, int? nullableInteger, string name, Group group)
  {
    return new Item {
      Id = id,
      Byte = (byte)id,
      Short = (short)id,
      Integer = id,
      Long = id,
      Float = id,
      Double = id,
      Decimal = id,
      NullableInteger = nullableInteger,
      Name = name,
      Flag = id != 2,
      Date = new DateTime(2026, 1, id, 0, 0, 0, DateTimeKind.Utc),
      Timestamp = Timestamp(id),
      Code = Code(id),
      Color = id == 2 ? Color.Yellow : Color.Red,
      Group = group
    };
  }
}
