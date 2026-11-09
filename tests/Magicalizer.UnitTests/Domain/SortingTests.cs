// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain;

namespace Magicalizer.UnitTests.Domain;

/// <summary>
/// The sortings of the models: the parsing of the query values, the constructor and the builder.
/// </summary>
public class SortingTests
{
  [Theory]
  [InlineData("name", true, "name")]
  [InlineData("-name", false, "name")]
  [InlineData(" -parent.name ", false, "parent.name")]
  [InlineData("", true, "")]
  public void Parse_Value_ReturnsSorting(string value, bool isAscending, string propertyPath)
  {
    Sorting<ItemModel> sorting = Sorting<ItemModel>.Parse(value);

    Assert.Equal(isAscending, sorting.IsAscending);
    Assert.Equal(propertyPath, sorting.PropertyPath);
  }

  [Fact]
  public void Constructor_Expression_IsConvertedToPropertyPath()
  {
    Assert.Equal("Parent.Name", new Sorting<ItemModel>(true, m => m.Parent!.Name!).PropertyPath);
    Assert.Equal("Id", new Sorting<ItemModel>(true, m => m.Id).PropertyPath);
  }

  [Fact]
  public void Builder_NestedProperties_BuildPath()
  {
    Assert.Equal("Parent.Parent.Name", new SortingBuilder<ItemModel>(true).Add(m => m.Parent).ThenAdd(p => p.Parent).ThenAdd(p => p.Name).Build().PropertyPath);
    Assert.False(new SortingBuilder<ItemModel>(false).Add(m => m.Parent).Build().IsAscending);
  }
}
