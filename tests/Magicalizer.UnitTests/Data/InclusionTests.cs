// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data;
using Magicalizer.Data.Extensions;

namespace Magicalizer.UnitTests.Data;

/// <summary>
/// The inclusions applied to a query in memory: the property paths are checked before they are passed to EF Core.
/// </summary>
public class InclusionTests
{
  [Theory]
  [InlineData("Group")]
  [InlineData("children.group")]
  [InlineData("Children.Children.Group")]
  public void ApplyInclusions_ValidPath_ReturnsQuery(string propertyPath)
  {
    Assert.Equal("1,2,3", Items.Ids(Items.Create().ApplyInclusions([new Inclusion<Item>(propertyPath)])));
  }

  [Fact]
  public void ApplyInclusions_Builder_BuildsNestedPath()
  {
    Assert.Equal("Children.Group", new InclusionBuilder<Item>().Add(i => i.Children).ThenAdd(c => c.Group).Build().PropertyPath);
    Assert.Equal("Group", new Inclusion<Item>(i => i.Group).PropertyPath);
  }

  [Theory]
  [InlineData("Nope", "Unable to include 'Nope': the type 'Item' doesn't have the property 'Nope'.")]
  [InlineData("Name", "Unable to include 'Name': the property 'Name' isn't a navigation property.")]
  [InlineData("Group.Name", "Unable to include 'Group.Name': the property 'Name' isn't a navigation property.")]
  [InlineData("Children.Nope", "Unable to include 'Children.Nope': the type 'Item' doesn't have the property 'Nope'.")]
  [InlineData("Group.", "Unable to include 'Group.': the property path is empty or malformed.")]
  public void ApplyInclusions_InvalidPath_Throws(string propertyPath, string message)
  {
    InvalidInclusionException exception = Assert.Throws<InvalidInclusionException>(() => Items.Create().ApplyInclusions([new Inclusion<Item>(propertyPath)]));

    Assert.Equal(propertyPath, exception.PropertyPath);
    Assert.Equal(message, exception.Message);
  }
}
