// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Magicalizer.Data.Extensions;
using Magicalizer.Filters.Abstractions;

namespace Magicalizer.UnitTests.Data;

/// <summary>
/// The filters applied to a query in memory (see <see cref="Items"/>): the conditions of all the value filters, the
/// nested objects and the collections.
/// </summary>
public class FilteringTests
{
  [Theory]
  [InlineData("equals", "2")]
  [InlineData("notEquals", "1,3")]
  [InlineData("from", "2,3")]
  [InlineData("to", "1,2")]
  [InlineData("in", "1,3")]
  [InlineData("notIn", "2")]
  public void Filter_ByteConditions_ReturnMatching(string condition, string expected)
  {
    ByteFilter filter = condition switch {
      "equals" => new ByteFilter(equals: (byte)2),
      "notEquals" => new ByteFilter(notEquals: (byte)2),
      "from" => new ByteFilter(from: (byte)2),
      "to" => new ByteFilter(to: (byte)2),
      "in" => new ByteFilter(@in: [(byte)1, (byte)3]),
      _ => new ByteFilter(notIn: [(byte)1, (byte)3])
    };

    Assert.Equal(expected, Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Byte = filter })));
  }

  [Theory]
  [InlineData("equals", "2")]
  [InlineData("notEquals", "1,3")]
  [InlineData("from", "2,3")]
  [InlineData("to", "1,2")]
  [InlineData("in", "1,3")]
  [InlineData("notIn", "2")]
  public void Filter_ShortConditions_ReturnMatching(string condition, string expected)
  {
    ShortFilter filter = condition switch {
      "equals" => new ShortFilter(equals: (short)2),
      "notEquals" => new ShortFilter(notEquals: (short)2),
      "from" => new ShortFilter(from: (short)2),
      "to" => new ShortFilter(to: (short)2),
      "in" => new ShortFilter(@in: [(short)1, (short)3]),
      _ => new ShortFilter(notIn: [(short)1, (short)3])
    };

    Assert.Equal(expected, Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Short = filter })));
  }

  [Theory]
  [InlineData("equals", "2")]
  [InlineData("notEquals", "1,3")]
  [InlineData("from", "2,3")]
  [InlineData("to", "1,2")]
  [InlineData("in", "1,3")]
  [InlineData("notIn", "2")]
  public void Filter_IntegerConditions_ReturnMatching(string condition, string expected)
  {
    IntegerFilter filter = condition switch {
      "equals" => new IntegerFilter(equals: 2),
      "notEquals" => new IntegerFilter(notEquals: 2),
      "from" => new IntegerFilter(from: 2),
      "to" => new IntegerFilter(to: 2),
      "in" => new IntegerFilter(@in: [1, 3]),
      _ => new IntegerFilter(notIn: [1, 3])
    };

    Assert.Equal(expected, Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Integer = filter })));
  }

  [Theory]
  [InlineData("equals", "2")]
  [InlineData("notEquals", "1,3")]
  [InlineData("from", "2,3")]
  [InlineData("to", "1,2")]
  [InlineData("in", "1,3")]
  [InlineData("notIn", "2")]
  public void Filter_LongConditions_ReturnMatching(string condition, string expected)
  {
    LongFilter filter = condition switch {
      "equals" => new LongFilter(equals: 2L),
      "notEquals" => new LongFilter(notEquals: 2L),
      "from" => new LongFilter(from: 2L),
      "to" => new LongFilter(to: 2L),
      "in" => new LongFilter(@in: [1L, 3L]),
      _ => new LongFilter(notIn: [1L, 3L])
    };

    Assert.Equal(expected, Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Long = filter })));
  }

  [Theory]
  [InlineData("equals", "2")]
  [InlineData("notEquals", "1,3")]
  [InlineData("from", "2,3")]
  [InlineData("to", "1,2")]
  [InlineData("in", "1,3")]
  [InlineData("notIn", "2")]
  public void Filter_FloatConditions_ReturnMatching(string condition, string expected)
  {
    FloatFilter filter = condition switch {
      "equals" => new FloatFilter(equals: 2f),
      "notEquals" => new FloatFilter(notEquals: 2f),
      "from" => new FloatFilter(from: 2f),
      "to" => new FloatFilter(to: 2f),
      "in" => new FloatFilter(@in: [1f, 3f]),
      _ => new FloatFilter(notIn: [1f, 3f])
    };

    Assert.Equal(expected, Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Float = filter })));
  }

  [Theory]
  [InlineData("equals", "2")]
  [InlineData("notEquals", "1,3")]
  [InlineData("from", "2,3")]
  [InlineData("to", "1,2")]
  [InlineData("in", "1,3")]
  [InlineData("notIn", "2")]
  public void Filter_DoubleConditions_ReturnMatching(string condition, string expected)
  {
    DoubleFilter filter = condition switch {
      "equals" => new DoubleFilter(equals: 2d),
      "notEquals" => new DoubleFilter(notEquals: 2d),
      "from" => new DoubleFilter(from: 2d),
      "to" => new DoubleFilter(to: 2d),
      "in" => new DoubleFilter(@in: [(double)1, (double)3]),
      _ => new DoubleFilter(notIn: [(double)1, (double)3])
    };

    Assert.Equal(expected, Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Double = filter })));
  }

  [Theory]
  [InlineData("equals", "2")]
  [InlineData("notEquals", "1,3")]
  [InlineData("from", "2,3")]
  [InlineData("to", "1,2")]
  [InlineData("in", "1,3")]
  [InlineData("notIn", "2")]
  public void Filter_DecimalConditions_ReturnMatching(string condition, string expected)
  {
    DecimalFilter filter = condition switch {
      "equals" => new DecimalFilter(equals: 2m),
      "notEquals" => new DecimalFilter(notEquals: 2m),
      "from" => new DecimalFilter(from: 2m),
      "to" => new DecimalFilter(to: 2m),
      "in" => new DecimalFilter(@in: [1m, 3m]),
      _ => new DecimalFilter(notIn: [1m, 3m])
    };

    Assert.Equal(expected, Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Decimal = filter })));
  }

  [Theory]
  [InlineData(true, "1,3")]
  [InlineData(false, "2")]
  public void Filter_BooleanCondition_ReturnsMatching(bool equals, string expected)
  {
    Assert.Equal(expected, Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Flag = new BooleanFilter(equals: equals) })));
  }

  [Theory]
  [InlineData("equals", "2")]
  [InlineData("notEquals", "1,3")]
  [InlineData("from", "2,3")]
  [InlineData("to", "1,2")]
  [InlineData("in", "2")]
  [InlineData("notIn", "1,3")]
  public void Filter_DateTimeConditions_ReturnMatching(string condition, string expected)
  {
    DateTime date = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
    DateTimeFilter filter = condition switch {
      "equals" => new DateTimeFilter(equals: date),
      "notEquals" => new DateTimeFilter(notEquals: date),
      "from" => new DateTimeFilter(from: date),
      "to" => new DateTimeFilter(to: date),
      "in" => new DateTimeFilter(@in: [date]),
      _ => new DateTimeFilter(notIn: [date])
    };

    Assert.Equal(expected, Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Date = filter })));
  }

  [Theory]
  [InlineData("equals", "1")]
  [InlineData("notEquals", "2,3")]
  [InlineData("from", "1,2")]
  [InlineData("to", "1,3")]
  [InlineData("in", "1")]
  [InlineData("notIn", "2,3")]
  public void Filter_DateTimeOffsetConditions_ReturnMatching(string condition, string expected)
  {
    // The moment of the 1st item with another offset (see Items.Timestamp).
    DateTimeOffset timestamp = Items.Timestamp(1).ToUniversalTime();
    DateTimeOffsetFilter filter = condition switch {
      "equals" => new DateTimeOffsetFilter(equals: timestamp),
      "notEquals" => new DateTimeOffsetFilter(notEquals: timestamp),
      "from" => new DateTimeOffsetFilter(from: timestamp),
      "to" => new DateTimeOffsetFilter(to: timestamp),
      "in" => new DateTimeOffsetFilter(@in: [timestamp]),
      _ => new DateTimeOffsetFilter(notIn: [timestamp])
    };

    Assert.Equal(expected, Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Timestamp = filter })));
  }

  [Theory]
  [InlineData("equals", "2")]
  [InlineData("notEquals", "1,3")]
  [InlineData("in", "1,3")]
  [InlineData("notIn", "2")]
  public void Filter_GuidConditions_ReturnMatching(string condition, string expected)
  {
    GuidFilter filter = condition switch {
      "equals" => new GuidFilter(equals: Items.Code(2)),
      "notEquals" => new GuidFilter(notEquals: Items.Code(2)),
      "in" => new GuidFilter(@in: [Items.Code(1), Items.Code(3)]),
      _ => new GuidFilter(notIn: [Items.Code(1), Items.Code(3)])
    };

    Assert.Equal(expected, Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Code = filter })));
  }

  [Theory]
  [InlineData("equals", "1,3")]
  [InlineData("notEquals", "2")]
  [InlineData("in", "2")]
  [InlineData("notIn", "1,3")]
  public void Filter_EnumConditions_ReturnMatching(string condition, string expected)
  {
    EnumFilter<Color> filter = condition switch {
      "equals" => new EnumFilter<Color>(equals: Color.Red),
      "notEquals" => new EnumFilter<Color>(notEquals: Color.Red),
      "in" => new EnumFilter<Color>(@in: [Color.Yellow, Color.Green]),
      _ => new EnumFilter<Color>(notIn: [Color.Yellow, Color.Green])
    };

    Assert.Equal(expected, Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Color = filter })));
  }

  [Fact]
  public void Filter_Null_ReturnsAll()
  {
    Assert.Equal("1,2,3", Items.Ids(Items.Create().ApplyFiltering<Item, ItemFilter>(null)));
    Assert.Equal("1,2,3", Items.Ids(Items.Create().ApplyFiltering(new ItemFilter())));
  }

  [Theory]
  [InlineData(true, "2")]
  [InlineData(false, "1,3")]
  public void Filter_NullConditionOfNullableValue_ReturnsMatching(bool isNull, string expected)
  {
    ItemFilter filter = new ItemFilter { NullableInteger = new IntegerFilter(isNull: isNull) };

    Assert.Equal(expected, Items.Ids(Items.Create().ApplyFiltering(filter)));
  }

  [Theory]
  [InlineData(true, "")]
  [InlineData(false, "1,2,3")]
  public void Filter_NullConditionOfNonNullableValue_TreatsItAsNotNull(bool isNull, string expected)
  {
    ItemFilter filter = new ItemFilter { Integer = new IntegerFilter(isNull: isNull) };

    Assert.Equal(expected, Items.Ids(Items.Create().ApplyFiltering(filter)));
  }

  [Fact]
  public void Filter_NotEqualsAndNotInOfNullableValue_MatchNulls()
  {
    // As in C#, null isn't equal to any value (EF Core translates the conditions with the same semantics).
    Assert.Equal("2,3", Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { NullableInteger = new IntegerFilter(notEquals: 1) })));
    Assert.Equal("2,3", Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { NullableInteger = new IntegerFilter(notIn: [1]) })));
  }

  [Theory]
  [InlineData("equals", "1")]
  [InlineData("notEquals", "2,3")]
  [InlineData("contains", "2")]
  [InlineData("startsWith", "3")]
  [InlineData("endsWith", "1")]
  [InlineData("in", "1,3")]
  [InlineData("notIn", "2")]
  public void Filter_StringConditions_ReturnMatching(string condition, string expected)
  {
    StringFilter filter = condition switch {
      "equals" => new StringFilter(equals: "Apple"),
      "notEquals" => new StringFilter(notEquals: "Apple"),
      "contains" => new StringFilter(contains: "an"),
      "startsWith" => new StringFilter(startsWith: "Ch"),
      "endsWith" => new StringFilter(endsWith: "le"),
      "in" => new StringFilter(@in: ["Apple", "Cherry"]),
      _ => new StringFilter(notIn: ["Apple", "Cherry"])
    };

    Assert.Equal(expected, Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Name = filter })));
  }

  [Theory]
  [InlineData("equals")]
  [InlineData("contains")]
  [InlineData("startsWith")]
  [InlineData("endsWith")]
  public void Filter_StringConditions_DontConvertCase(string condition)
  {
    // The strings are compared as they are, so the database compares them by its collation (in memory they are
    // compared ordinally).
    StringFilter filter = condition switch {
      "equals" => new StringFilter(equals: "apple"),
      "contains" => new StringFilter(contains: "AN"),
      "startsWith" => new StringFilter(startsWith: "ch"),
      _ => new StringFilter(endsWith: "LE")
    };

    Assert.Equal("", Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Name = filter })));
  }

  [Fact]
  public void Filter_SeveralConditions_AreCombinedWithAnd()
  {
    ItemFilter filter = new ItemFilter { Integer = new IntegerFilter(from: 2), Name = new StringFilter(contains: "e"), Id = new IntegerFilter(notEquals: 1) };

    Assert.Equal("3", Items.Ids(Items.Create().ApplyFiltering(filter)));
  }

  [Fact]
  public void Filter_NestedObjectCondition_ReturnsMatching()
  {
    Assert.Equal("1,2", Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Group = new GroupFilter { Name = new StringFilter(equals: "A") } })));
  }

  [Theory]
  [InlineData(true, "2")]
  [InlineData(false, "1,3")]
  public void Filter_EmptinessCondition_ReturnsMatching(bool isEmpty, string expected)
  {
    ItemFilter filter = new ItemFilter { Children = new EnumerableFilter<ItemFilter>(isEmpty: isEmpty) };

    Assert.Equal(expected, Items.Ids(Items.Create().ApplyFiltering(filter)));
  }

  [Fact]
  public void Filter_CollectionConditions_ReturnMatching()
  {
    Assert.Equal("3", Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Children = new EnumerableFilter<ItemFilter>(any: [new ItemFilter { Name = new StringFilter(equals: "Cherry") }]) })));
    Assert.Equal("2,3", Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Children = new EnumerableFilter<ItemFilter>(none: [new ItemFilter { Id = new IntegerFilter(equals: 2) }]) })));
  }

  [Fact]
  public void Filter_CollectionWithSeveralFilters_EachIsMatchedByAnyElement()
  {
    ItemFilter apple = new ItemFilter { Name = new StringFilter(equals: "Apple") };
    ItemFilter cherry = new ItemFilter { Name = new StringFilter(equals: "Cherry") };
    ItemFilter appleAndCherry = new ItemFilter { Name = new StringFilter(equals: "Apple"), Id = new IntegerFilter(equals: 3) };

    Assert.Equal("3", Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Children = new EnumerableFilter<ItemFilter>(any: [apple, cherry]) })));
    Assert.Equal("", Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Children = new EnumerableFilter<ItemFilter>(any: [appleAndCherry]) })));
  }

  [Fact]
  public void Filter_IgnoredProperty_IsNotApplied()
  {
    Assert.Equal("1,2,3", Items.Ids(Items.Create().ApplyFiltering(new ItemFilter { Ignored = new IntegerFilter(equals: 100) })));
  }

  [Fact]
  public void Filter_Values_AreQueryParameters()
  {
    // The values are wrapped (as the captured variables are), so EF Core passes them as parameters, not as constants.
    ItemFilter filter = new ItemFilter {
      Integer = new IntegerFilter(equals: 2),
      NullableInteger = new IntegerFilter(@in: [1, 3]),
      Name = new StringFilter(contains: "e"),
      Children = new EnumerableFilter<ItemFilter>(any: [new ItemFilter { Id = new IntegerFilter(from: 2) }])
    };

    ConstantCollector constantCollector = new ConstantCollector();

    constantCollector.Visit(Items.Create().ApplyFiltering(filter).Expression);
    Assert.Equal(4, constantCollector.Constants.Count(IsQueryParameter));
    Assert.All(constantCollector.Constants, c => Assert.True(c.Value is IQueryable || IsQueryParameter(c)));
  }

  // Checks whether the constant is a wrapped value of a query parameter.
  private static bool IsQueryParameter(ConstantExpression constant)
  {
    return constant.Type.IsGenericType && constant.Type.GetGenericTypeDefinition() == typeof(StrongBox<>);
  }

  // Collects the constants of an expression.
  private sealed class ConstantCollector : ExpressionVisitor
  {
    /// <summary>
    /// Constants of the visited expression.
    /// </summary>
    public List<ConstantExpression> Constants { get; } = [];

    /// <inheritdoc/>
    protected override Expression VisitConstant(ConstantExpression node)
    {
      this.Constants.Add(node);
      return base.VisitConstant(node);
    }
  }
}
