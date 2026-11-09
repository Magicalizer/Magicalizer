// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data;
using Magicalizer.Data.Extensions;

namespace Magicalizer.UnitTests.Data;

/// <summary>
/// The sortings applied to a query in memory (see <see cref="Items"/>) and the invalid property paths.
/// </summary>
public class SortingTests
{
  [Theory]
  [InlineData("Integer", true, "1,2,3")]
  [InlineData("Integer", false, "3,2,1")]
  [InlineData("integer", false, "3,2,1")]
  [InlineData("Group.Name", false, "3,1,2")]
  public void ApplySorting_OneSorting_SortsByIt(string propertyPath, bool isAscending, string expected)
  {
    Assert.Equal(expected, Items.Ids(Items.Create().ApplySorting([new Sorting<Item>(isAscending, propertyPath)])));
  }

  [Fact]
  public void ApplySorting_SeveralSortings_AreAppliedInOrder()
  {
    Sorting<Item>[] sortings = [new Sorting<Item>(false, "group.name"), new Sorting<Item>(false, "id")];

    Assert.Equal("3,2,1", Items.Ids(Items.Create().ApplySorting(sortings)));
    Assert.Equal("2,1,3", Items.Ids(Items.Create().ApplySorting([new Sorting<Item>(true, "Group.Name"), new Sorting<Item>(false, "Id")])));
  }

  [Fact]
  public void ApplySorting_Builder_BuildsSamePath()
  {
    Sorting<Item> sorting = new SortingBuilder<Item>(false).Add(i => i.Group).Build();

    Assert.Equal("Group", sorting.PropertyPath);
    Assert.False(sorting.IsAscending);
    Assert.Equal("Group.Name", new Sorting<Item>(true, i => i.Group!.Name!).PropertyPath);
  }

  [Fact]
  public void ApplySorting_Null_ReturnsQuery()
  {
    Assert.Equal("1,2,3", Items.Ids(Items.Create().ApplySorting(null)));
  }

  [Theory]
  [InlineData("Nope", "Unable to sort by 'Nope': the type 'Item' doesn't have the property 'Nope'.")]
  [InlineData("Children", "Unable to sort by 'Children': the property 'Children' is a collection.")]
  [InlineData("Group", "Unable to sort by 'Group': the property 'Group' isn't a scalar value.")]
  [InlineData("Group.Nope", "Unable to sort by 'Group.Nope': the type 'Group' doesn't have the property 'Nope'.")]
  [InlineData("Children.Name", "Unable to sort by 'Children.Name': the property 'Children' is a collection.")]
  [InlineData("Group..Name", "Unable to sort by 'Group..Name': the property path is empty or malformed.")]
  public void ApplySorting_InvalidPath_Throws(string propertyPath, string message)
  {
    InvalidSortingException exception = Assert.Throws<InvalidSortingException>(() => Items.Create().ApplySorting([new Sorting<Item>(true, propertyPath)]));

    Assert.Equal(propertyPath, exception.PropertyPath);
    Assert.Equal(message, exception.Message);
  }
}
