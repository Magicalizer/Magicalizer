// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Data.Entities.Abstractions;
using Magicalizer.Domain.Models.Abstractions;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Extensions;
using Magicalizer.Filters.Abstractions;
using Magicalizer.UnitTests.Data;
using Magicalizer.UnitTests.Domain;

namespace Magicalizer.UnitTests.Extensions;

/// <summary>
/// The search of the implementations and the type arguments of the generic interfaces (the DTOs, the models and the
/// entities are found this way), and the reflection of the properties, collections and filters.
/// </summary>
public class TypeExtensionsTests
{
  [Fact]
  public void GetImplementations_Interface_ReturnsNonAbstractClasses()
  {
    IEnumerable<Type> implementations = typeof(IModel).GetImplementations();

    Assert.Contains(typeof(ItemModel), implementations);
    Assert.DoesNotContain(typeof(IModel), implementations);
  }

  [Fact]
  public void GetGenericInterfaceTypeArguments_ImplementedInterface_ReturnsTypeArguments()
  {
    Assert.Equal([typeof(int)], typeof(Item).GetGenericInterfaceTypeArguments(typeof(IEntity<>)));
    Assert.Null(typeof(ItemModel).GetGenericInterfaceTypeArguments(typeof(IModel<>)));
  }

  [Fact]
  public void GetGenericInterfaceTypeArgument_BaseType_ReturnsMatchingArgument()
  {
    Assert.Equal(typeof(int), typeof(Item).GetGenericInterfaceTypeArgument(typeof(IEntity<>), typeof(int)));
    Assert.Null(typeof(Item).GetGenericInterfaceTypeArgument(typeof(IEntity<>), typeof(string)));
  }

  [Fact]
  public void GetPublicProperties_DerivedFilter_ReturnsInheritedOnes()
  {
    Assert.Equal(
      ["From", "To", "NotEquals", "In", "NotIn", "IsNull", "Equals"],
      typeof(IntegerFilter).GetPublicProperties().Select(p => p.Name)
    );
  }

  [Fact]
  public void GetPublicProperty_NameInAnyCase_ReturnsProperty()
  {
    Assert.Equal(typeof(Item).GetProperty(nameof(Item.Name)), typeof(Item).GetPublicProperty("nAME"));
    Assert.Null(typeof(Item).GetPublicProperty("Nope"));
  }

  [Fact]
  public void GetValueGetter_InheritedProperty_ReturnsValue()
  {
    IntegerFilter filter = new IntegerFilter(equals: 5);

    // The property is declared on the base class (ValueFilterBase) and returns a value type.
    Assert.Equal(5, typeof(IntegerFilter).GetPublicProperty(nameof(IntegerFilter.Equals))!.GetValueGetter()(filter));
    Assert.Null(typeof(IntegerFilter).GetPublicProperty(nameof(IntegerFilter.From))!.GetValueGetter()(filter));
  }

  [Theory]
  [InlineData(typeof(ICollection<Item>), typeof(Item))]
  [InlineData(typeof(List<int>), typeof(int))]
  [InlineData(typeof(Item[]), typeof(Item))]
  [InlineData(typeof(IEnumerable<ItemFilter>), typeof(ItemFilter))]
  [InlineData(typeof(string), null)]
  [InlineData(typeof(int), null)]
  public void GetCollectionElementType_Type_ReturnsElementTypeOfCollections(Type type, Type? expected)
  {
    Assert.Equal(expected, type.GetCollectionElementType());
  }

  [Theory]
  [InlineData(typeof(IEnumerable<ISorting<ItemModel>>), typeof(ItemModel))]
  [InlineData(typeof(IEnumerable<IInclusion<ItemModel>>), null)]
  [InlineData(typeof(List<ISorting<ItemModel>>), null)]
  [InlineData(typeof(ISorting<ItemModel>), null)]
  public void GetEnumerableElementTypeArgument_SortingDefinition_ReturnsModelOfSortingsOnly(Type type, Type? expected)
  {
    Assert.Equal(expected, type.GetEnumerableElementTypeArgument(typeof(ISorting<>)));
  }

  [Fact]
  public void GetFilterProperties_IgnoredProperty_IsSkipped()
  {
    IEnumerable<string> names = typeof(ItemFilter).GetFilterProperties().Select(p => p.Name);

    Assert.Contains(nameof(ItemFilter.Name), names);
    Assert.DoesNotContain(nameof(ItemFilter.Ignored), names);
  }

  [Theory]
  [InlineData(typeof(IntegerFilter), true)]
  [InlineData(typeof(StringFilter), true)]
  [InlineData(typeof(EnumFilter<Color>), true)]
  [InlineData(typeof(GroupFilter), false)]
  [InlineData(typeof(EnumerableFilter<ItemFilter>), false)]
  [InlineData(typeof(Item), false)]
  public void IsValueFilter_Type_IsTrueForFiltersOfConditionsOnly(Type type, bool expected)
  {
    Assert.Equal(expected, type.IsValueFilter());
  }

  [Fact]
  public void GetElementFilterType_EnumerableFilter_ReturnsElementFilterType()
  {
    Assert.Equal(typeof(ItemFilter), typeof(EnumerableFilter<ItemFilter>).GetElementFilterType());
  }
}
