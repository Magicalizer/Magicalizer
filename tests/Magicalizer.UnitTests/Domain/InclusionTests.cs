// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain;

namespace Magicalizer.UnitTests.Domain;

/// <summary>
/// The inclusions of the models: the constructor and the builder.
/// </summary>
public class InclusionTests
{
  [Fact]
  public void Constructor_Expression_IsConvertedToPropertyPath()
  {
    Assert.Equal("Parent.Children", new Inclusion<ItemModel>(m => m.Parent!.Children).PropertyPath);
  }

  [Fact]
  public void Builder_NestedProperties_BuildPath()
  {
    Assert.Equal("Children.Parent", new InclusionBuilder<ItemModel>().Add(m => m.Children).ThenAdd(c => c.Parent).Build().PropertyPath);
  }

  [Fact]
  public void Builder_CollectionsOfAllTypes_AreFollowedIntoElements()
  {
    Assert.Equal("List.Collection.Enumerable.Children.Parent", new InclusionBuilder<ItemModel>().Add(m => m.List).ThenAdd(m => m.Collection).ThenAdd(m => m.Enumerable).ThenAdd(m => m.Children).ThenAdd(m => m.Parent).Build().PropertyPath);
    Assert.Equal("Collection", new InclusionBuilder<ItemModel>().Add(m => m.Collection).Build().PropertyPath);
    Assert.Equal("Enumerable", new InclusionBuilder<ItemModel>().Add(m => m.Enumerable).Build().PropertyPath);
    Assert.Equal("Children", new InclusionBuilder<ItemModel>().Add(m => m.Children).Build().PropertyPath);
  }
}
