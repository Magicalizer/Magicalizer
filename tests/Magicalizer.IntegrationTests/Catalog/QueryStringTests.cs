// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Magicalizer.IntegrationTests.Infrastructure;

namespace Magicalizer.IntegrationTests.Catalog;

/// <summary>
/// The validation of the query parameters on a known data set (see <see cref="CatalogTestsFactory"/>): a parameter the
/// action doesn't support or an invalid collection condition is a client error, as the response would have the objects
/// the client didn't ask for.
/// </summary>
public class QueryStringTests : IntegrationTest<CatalogTestsFactory>
{
  /// <summary>
  /// Creates the tests.
  /// </summary>
  public QueryStringTests(CatalogTestsFactory factory) : base(factory) { }

  [Theory]
  [InlineData("nope=1", "nope")]
  [InlineData("nope.equals=1", "nope.equals")]
  [InlineData("name.nope=1", "name.nope")]
  [InlineData("name=Borscht", "name")]
  [InlineData("name.contains.more=a", "name.contains.more")]
  [InlineData("category=1", "category")]
  [InlineData("category.name=Pizza", "category.name")]
  [InlineData("category.nope.equals=1", "category.nope.equals")]
  [InlineData("photos.any=1", "photos.any")]
  [InlineData("photos.nope.filename.contains=a", "photos.nope.filename.contains")]
  [InlineData("photos.any.nope.contains=a", "photos.any.nope.contains")]
  [InlineData("name[0].contains=a", "name[0].contains")]
  [InlineData("sort[0]=name", "sort[0]")]
  public async Task QueryString_UnsupportedParameter_Returns400(string query, string parameter)
  {
    await AssertValidationErrorsAsync(await this.Anonymous.GetAsync($"/api/products?{query}"), parameter);
  }

  [Fact]
  public async Task QueryString_SeveralErrors_AreReturnedTogether()
  {
    // The errors of the names, values, sortings, inclusions and paging are collected before the action is executed.
    await AssertValidationErrorsAsync(await this.Anonymous.GetAsync("/api/products?naem.contains=a&price.from=abc&sort=nope&include=nope&offset=-1&limit=abc"), "naem.contains", "price.from", "sort", "include", "offset", "limit");
  }

  [Theory]
  [InlineData("products.any[0].name.contains=Pep&products.any[2].name.contains=Borscht", "products.any")]
  [InlineData("products.any[1].name.contains=Borscht", "products.any")]
  [InlineData("products.none[1].price.from=1", "products.none")]
  [InlineData("products.any[0].photos.any[1].filename.contains=a", "products.any[0].photos.any")]
  public async Task QueryString_CollectionListFormWithGaps_Returns400(string query, string collection)
  {
    // Otherwise the conditions after a gap would be ignored silently.
    await AssertValidationErrorsAsync(await this.Anonymous.GetAsync($"/api/categories?{query}"), collection);
  }

  [Theory]
  [InlineData("products.any.name.contains=Pep&products.any[0].name.contains=Borscht", "products.any")]
  [InlineData("products.none[0].price.from=1&products.none.name.contains=a", "products.none")]
  public async Task QueryString_CollectionFormsCombined_Returns400(string query, string collection)
  {
    // Otherwise the conditions of the list form would be ignored silently.
    await AssertValidationErrorsAsync(await this.Anonymous.GetAsync($"/api/categories?{query}"), collection);
  }

  [Theory]
  [InlineData("products.any[x].name.contains")]
  [InlineData("products.any[99999999999].name.contains")]
  [InlineData("products.any[\u0661].name.contains")]
  [InlineData("products.any[-1].name.contains")]
  public async Task QueryString_CollectionIndexNotNumber_Returns400(string parameter)
  {
    // An index that doesn't fit into an int or has non-ASCII digits (e.g. Arabic-Indic) isn't an error of the server.
    await AssertValidationErrorsAsync(await this.Anonymous.GetAsync($"/api/categories?{Uri.EscapeDataString(parameter)}=a"), parameter);
  }

  [Theory]
  [InlineData("products.any.price.from=150&products.none[0].name.contains=Borscht", "Pizza")]
  [InlineData("products.any[0].photos.any[0].filename.contains=margherita-1&products.any[1].photos.any.filename.contains=pepperoni-1", "Pizza")]
  public async Task QueryString_DifferentCollectionsInDifferentForms_AreAllowed(string query, string expected)
  {
    CatalogDataSet dataSet = this.Factory.DataSet;

    Assert.Equal(dataSet.Others(expected), GetIds(await this.Anonymous.GetJsonAsync($"/api/categories?{query}")));
  }

  [Fact]
  public async Task QueryString_NamesAndFilterPrefix_AreCaseInsensitive()
  {
    CatalogDataSet dataSet = this.Factory.DataSet;

    Assert.Equal(dataSet.Products("2"), GetIds(await this.Anonymous.GetJsonAsync("/api/products?Name.Contains=Pep&SORT=-price")));
    Assert.Equal(dataSet.Products("2"), GetIds(await this.Anonymous.GetJsonAsync("/api/products?filter.name.contains=Pep")));
  }

  [Fact]
  public async Task QueryString_IgnoredParameter_IsAllowed()
  {
    // The app processes the culture itself (see the options of Magicalizer in the test host).
    Assert.Equal(HttpStatusCode.OK, (await this.Anonymous.GetAsync("/api/products?culture=uk")).StatusCode);
  }

  [Fact]
  public async Task QueryString_UnsupportedParameterOfOtherActions_Returns400()
  {
    CatalogDataSet dataSet = this.Factory.DataSet;

    Assert.Equal(HttpStatusCode.OK, (await this.Anonymous.GetAsync($"/api/products/{dataSet.ProductIds[0]}?include=category")).StatusCode);
    await AssertValidationErrorsAsync(await this.Anonymous.GetAsync($"/api/products/{dataSet.ProductIds[0]}?name.contains=a"), "name.contains");
    await AssertValidationErrorsAsync(await this.Anonymous.GetAsync($"/api/products/{dataSet.ProductIds[0]}?id=1"), "id");
    await AssertValidationErrorsAsync(await this.Anonymous.GetAsync("/api/photos/count?nope=1"), "nope");
  }
}
