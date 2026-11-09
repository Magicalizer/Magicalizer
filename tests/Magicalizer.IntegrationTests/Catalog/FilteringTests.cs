// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.IntegrationTests.Infrastructure;

namespace Magicalizer.IntegrationTests.Catalog;

/// <summary>
/// The filters of the standard API on a known data set (see <see cref="CatalogTestsFactory"/>): the value conditions,
/// the nested objects and both forms of the collection conditions.
/// </summary>
public class FilteringTests : IntegrationTest<CatalogTestsFactory>
{
  /// <summary>
  /// Creates the tests.
  /// </summary>
  public FilteringTests(CatalogTestsFactory factory) : base(factory) { }

  [Theory]
  [InlineData("", "1,2,3,4,5")]
  [InlineData("id.equals={2}", "2")]
  [InlineData("id.notequals={2}", "1,3,4,5")]
  [InlineData("id.in={1}&id.in={3}", "1,3")]
  [InlineData("id.notin={1}&id.notin={3}", "2,4,5")]
  [InlineData("id.from={3}", "3,4,5")]
  [InlineData("quantity.equals=0", "2,5")]
  [InlineData("quantity.from=5&quantity.to=7", "3,4")]
  public Task Filter_IntegerConditions_ReturnMatching(string query, string expected) => this.AssertProductsAsync(query, expected);

  [Theory]
  [InlineData("price.equals=90", "4,5")]
  [InlineData("price.from=100", "1,2,3")]
  [InlineData("price.to=100", "1,4,5")]
  [InlineData("price.from=95&price.to=160", "1,2")]
  [InlineData("price.in=100&price.in=200.5", "1,3")]
  public Task Filter_DecimalConditions_ReturnMatching(string query, string expected) => this.AssertProductsAsync(query, expected);

  [Theory]
  [InlineData("name.equals=Borscht", "4")]
  [InlineData("name.notequals=Borscht", "1,2,3,5")]
  [InlineData("name.contains=epp", "2")]
  [InlineData("name.contains=e", "1,2,3,5")]
  [InlineData("name.startswith=Bor", "4")]
  [InlineData("name.endswith=ese", "3")]
  [InlineData("name.in=Borscht&name.in=Minestrone", "4,5")]
  [InlineData("name.notin=Borscht&name.notin=Minestrone", "1,2,3")]
  [InlineData("description.isnull=true", "2,4")]
  [InlineData("description.isnull=false", "1,3,5")]
  public Task Filter_StringConditions_ReturnMatching(string query, string expected) => this.AssertProductsAsync(query, expected);

  [Theory]
  [InlineData("isavailable.equals=true", "1,3,4")]
  [InlineData("isavailable.equals=false", "2,5")]
  public Task Filter_BooleanConditions_ReturnMatching(string query, string expected) => this.AssertProductsAsync(query, expected);

  [Theory]
  [InlineData("created.from=2026-01-01", "1,2,3,5")]
  [InlineData("created.to=2026-01-10", "1,4,5")]
  [InlineData("created.equals=2026-02-15", "2")]
  [InlineData("created.from=2026-01-06&created.to=2026-02-28", "1,2")]
  [InlineData("created.in=2026-01-10&created.in=2026-02-15", "1,2")]
  [InlineData("created.notin=2026-01-10&created.notin=2026-02-15", "3,4,5")]
  public Task Filter_DateTimeConditions_ReturnMatching(string query, string expected) => this.AssertProductsAsync(query, expected);

  [Theory]
  [InlineData("code.equals={code-3}", "3")]
  [InlineData("code.notequals={code-3}", "1,2,4,5")]
  [InlineData("code.in={code-1}&code.in={code-2}", "1,2")]
  [InlineData("code.notin={code-1}&code.notin={code-2}", "3,4,5")]
  public Task Filter_GuidConditions_ReturnMatching(string query, string expected) => this.AssertProductsAsync(query, expected);

  [Theory]
  [InlineData("diet.equals=Vegetarian", "1,3")]
  [InlineData("diet.equals=vegan", "5")]
  [InlineData("diet.equals=2", "5")]
  [InlineData("diet.notequals=Regular", "1,3,5")]
  [InlineData("diet.in=Vegetarian&diet.in=Vegan", "1,3,5")]
  [InlineData("diet.notin=Vegetarian&diet.notin=Vegan", "2,4")]
  public Task Filter_EnumConditions_ReturnMatching(string query, string expected) => this.AssertProductsAsync(query, expected);

  [Theory]
  [InlineData("updated.isnull=true", "2,4")]
  [InlineData("updated.equals=2026-01-12T08:00:00Z", "1")]
  [InlineData("updated.notequals=2026-01-12T08:00:00Z", "2,3,4,5")]
  [InlineData("updated.from=2026-01-06T06:00:00Z", "1,3,5")]
  [InlineData("updated.from=2026-01-06T06:00:01Z", "1,3")]
  [InlineData("updated.to=2026-01-12T10:00:00%2B02:00", "1,5")]
  [InlineData("updated.in=2026-03-21T00:00:00Z&updated.in=2026-01-06T01:00:00-05:00", "3,5")]
  [InlineData("updated.notin=2026-03-21T00:00:00Z", "1,2,4,5")]
  public Task Filter_DateTimeOffsetConditions_ReturnMatching(string query, string expected) => this.AssertProductsAsync(query, expected);

  [Theory]
  [InlineData("isavailable.equals=true&price.from=100", "1,3")]
  [InlineData("name.contains=o&quantity.equals=0&price.to=100", "5")]
  public Task Filter_SeveralConditions_ReturnMatchingAll(string query, string expected) => this.AssertProductsAsync(query, expected);

  [Theory]
  [InlineData("category.id.equals={Pizza}", "1,2,3")]
  [InlineData("category.name.equals=Soups", "4,5")]
  [InlineData("category.description.isnull=true", "4,5")]
  [InlineData("category.name.equals=Pizza&price.from=150", "2,3")]
  public Task Filter_NestedObjectConditions_ReturnMatching(string query, string expected) => this.AssertProductsAsync(query, expected);

  [Theory]
  [InlineData("product.category.name.equals=Pizza", "margherita-1,margherita-2,pepperoni-1")]
  [InlineData("product.name.contains=Bor&filename.contains=.png", "borscht-1")]
  public async Task Filter_TwoLevelNestedObjectConditions_ReturnMatching(string query, string expected)
  {
    CatalogDataSet dataSet = this.Factory.DataSet;

    Assert.Equal(dataSet.Others(expected).Order(), GetIds(await this.Anonymous.GetJsonAsync($"/api/photos?{query}")).Order());
  }

  [Theory]
  [InlineData("photos.any.filename.contains=.png", "4")]
  [InlineData("photos.none.filename.contains=.jpg", "3,4,5")]
  [InlineData("photos.isempty=true", "3,5")]
  [InlineData("photos.isempty=", "1,2,3,4,5")]
  [InlineData("photos.isempty=false", "1,2,4")]
  [InlineData("tags.any.tag.name.equals=New", "2,4")]
  [InlineData("tags.any.tag.id.equals={Hot}&tags.any.position.equals=1", "")]
  public Task Filter_CollectionConditions_ReturnMatching(string query, string expected) => this.AssertProductsAsync(query, expected);

  [Theory]
  [InlineData("products.any.price.from=150", "Pizza")]
  [InlineData("products.none.price.from=150", "Soups,Напої")]
  [InlineData("products.isempty=true", "Напої")]
  [InlineData("products.isempty=false", "Pizza,Soups")]
  [InlineData("products.none.isavailable.equals=false", "Напої")]
  [InlineData("products.any.photos.any.filename.contains=.png", "Soups")]
  [InlineData("products.any.tags.any.tag.name.equals=Hot", "Pizza")]
  public Task Filter_CategoryCollectionConditions_ReturnMatching(string query, string expected) => this.AssertCategoriesAsync(query, expected);

  [Theory]
  [InlineData("photos.isempty=abc", "photos.isempty")]
  [InlineData("description.isnull=abc", "description.isnull")]
  public async Task Filter_InvalidEmptinessOrNullValue_Returns400(string query, string parameter)
  {
    // An invalid value of a collection condition isn't ignored, as the one of a value condition isn't.
    await AssertValidationErrorsAsync(await this.Anonymous.GetAsync($"/api/products?{query}"), parameter);
  }

  [Fact]
  public async Task Filter_CollectionSingleForm_OneElementMatchesAllConditions()
  {
    // The only Pizza with "pep" in the name isn't available, so no category has a product that matches both.
    await this.AssertCategoriesAsync("products.any.name.contains=Pep&products.any.isavailable.equals=true", "");
    await this.AssertCategoriesAsync("products.any.name.contains=Pep&products.any.isavailable.equals=false", "Pizza");
  }

  [Fact]
  public async Task Filter_CollectionListForm_EachFilterIsMatchedByAnyElement()
  {
    // Pepperoni matches the first filter and Margherita matches the second one.
    await this.AssertCategoriesAsync("products.any[0].name.contains=Pep&products.any[1].isavailable.equals=true", "Pizza");
    await this.AssertCategoriesAsync("products.any[0].name.contains=Pep&products.any[1].name.contains=Borscht", "");
    await this.AssertCategoriesAsync("products.none[0].price.from=150&products.none[1].name.contains=Borscht", "Напої");
  }

  [Theory]
  // Pepperoni is unavailable and Four Cheese is available and costs 200.5.
  [InlineData("products.any[0].name.contains=Pep&products.any[0].isavailable.equals=false&products.any[1].price.from=200&products.any[1].isavailable.equals=true", "Pizza")]
  // No product with "pep" in the name is available, so the first filter isn't matched.
  [InlineData("products.any[0].name.contains=Pep&products.any[0].isavailable.equals=true&products.any[1].price.from=200&products.any[1].isavailable.equals=true", "")]
  // Borscht and Minestrone have the same price, one of them is available and the other one isn't.
  [InlineData("products.any[0].price.equals=90&products.any[0].isavailable.equals=true&products.any[1].price.equals=90&products.any[1].isavailable.equals=false", "Soups")]
  [InlineData("products.any[0].price.equals=90&products.any[0].quantity.equals=7&products.any[1].price.equals=90&products.any[1].quantity.equals=7", "Soups")]
  // The same element may match several filters (Margherita costs 100).
  [InlineData("products.any[0].price.from=100&products.any[0].quantity.from=10&products.any[1].price.to=100&products.any[1].isavailable.equals=true", "Pizza")]
  // Pizza has an unavailable product that costs more than 100, but not the one that costs less.
  [InlineData("products.none[0].isavailable.equals=false&products.none[0].price.to=100", "Pizza,Напої")]
  [InlineData("products.any[0].isavailable.equals=true&products.any[0].quantity.from=7&products.none[0].isavailable.equals=false&products.none[0].price.from=150", "Soups")]
  // The conditions of an element can be the collections of the element too.
  [InlineData("products.any[0].tags.any.tag.name.equals=New&products.any[0].isavailable.equals=true", "Soups")]
  [InlineData("products.any[0].tags.any.tag.name.equals=New&products.any[0].isavailable.equals=false&products.any[1].photos.isempty=false&products.any[1].quantity.equals=10", "Pizza")]
  public Task Filter_CollectionListFormWithSeveralConditions_EachFilterIsMatchedByOneElement(string query, string expected) => this.AssertCategoriesAsync(query, expected);

  [Theory]
  // Pepperoni has Hot at 0 and New at 1, Borscht has New at 0.
  [InlineData("tags.any[0].tag.name.equals=Hot&tags.any[0].position.equals=0&tags.any[1].tag.name.equals=New&tags.any[1].position.equals=1", "2")]
  [InlineData("tags.any[0].tag.name.equals=Hot&tags.any[0].position.equals=0&tags.any[1].tag.name.equals=New&tags.any[1].position.equals=0", "")]
  [InlineData("tags.any[0].tag.name.equals=New&tags.any[0].position.equals=0", "4")]
  [InlineData("tags.any[0].tag.name.equals=Hot&tags.none[0].tag.name.equals=New&tags.none[0].position.equals=1", "1")]
  public Task Filter_TagCollectionListFormWithSeveralConditions_EachFilterIsMatchedByOneElement(string query, string expected) => this.AssertProductsAsync(query, expected);

  [Fact]
  public async Task Filter_CollectionListFormWithOneFilter_IsTheSameAsSingleForm()
  {
    await this.AssertCategoriesAsync("products.any[0].price.from=150", "Pizza");
    await this.AssertCategoriesAsync("products.any.price.from=150", "Pizza");
  }

  [Theory]
  [InlineData("name.contains=апо")]
  [InlineData("name.startswith=Нап")]
  [InlineData("name.endswith=ої")]
  public async Task Filter_StringConditionsOfNonLatinLetters_ReturnMatching(string query)
  {
    await this.AssertCategoriesAsync(query, "Напої");
  }

  [Fact]
  public async Task Filter_IgnoredProperty_IsAppliedByService()
  {
    // The search isn't a condition of the filter, the custom service converts it to the name condition.
    await this.AssertProductsAsync("search=Cheese", "3");
    await this.AssertProductsAsync("search=Cheese&name.contains=Pep", "3");
  }

  [Theory]
  [InlineData("price.from=abc", "price.from")]
  [InlineData("isavailable.equals=maybe", "isavailable.equals")]
  [InlineData("created.from=yesterday", "created.from")]
  [InlineData("code.equals=nope", "code.equals")]
  [InlineData("diet.equals=Spicy", "diet.equals")]
  [InlineData("updated.from=yesterday", "updated.from")]
  [InlineData("quantity.in=1&quantity.in=x", "quantity.in")]
  [InlineData("quantity.in=0,10", "quantity.in")]
  public async Task Filter_InvalidValue_Returns400WithParameter(string query, string parameter)
  {
    // The values of the list conditions are passed by repeating the parameter, so "0,10" isn't a number.
    await AssertValidationErrorsAsync(await this.Anonymous.GetAsync($"/api/products?{query}"), parameter);
  }

  private async Task AssertProductsAsync(string query, string expected)
  {
    CatalogDataSet dataSet = this.Factory.DataSet;
    List<int> ids = GetIds(await this.Anonymous.GetJsonAsync($"/api/products?{dataSet.Resolve(query)}"));

    Assert.Equal(dataSet.Products(expected).Order(), ids.Order());
  }

  private async Task AssertCategoriesAsync(string query, string expected)
  {
    CatalogDataSet dataSet = this.Factory.DataSet;
    List<int> ids = GetIds(await this.Anonymous.GetJsonAsync($"/api/categories?{dataSet.Resolve(query)}"));

    Assert.Equal(dataSet.Others(expected).Order(), ids.Order());
  }
}
