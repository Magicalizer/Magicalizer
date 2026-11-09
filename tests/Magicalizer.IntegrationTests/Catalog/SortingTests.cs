// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Magicalizer.IntegrationTests.Infrastructure;

namespace Magicalizer.IntegrationTests.Catalog;

/// <summary>
/// The sorting and the paging of the standard API on a known data set (see <see cref="CatalogTestsFactory"/>).
/// </summary>
public class SortingTests : IntegrationTest<CatalogTestsFactory>
{
  /// <summary>
  /// Creates the tests.
  /// </summary>
  public SortingTests(CatalogTestsFactory factory) : base(factory) { }

  [Theory]
  [InlineData("id", "1,2,3,4,5")]
  [InlineData("-id", "5,4,3,2,1")]
  [InlineData("name", "4,3,1,5,2")]
  [InlineData("-name", "2,5,1,3,4")]
  [InlineData("created", "4,5,1,2,3")]
  [InlineData("-created", "3,2,1,5,4")]
  [InlineData("-quantity,id", "1,4,3,2,5")]
  public Task Sort_OneProperty_ReturnsOrdered(string sort, string expected) => this.AssertOrderAsync($"sort={sort}", expected);

  [Theory]
  [InlineData("price,name", "4,5,1,2,3")]
  [InlineData("price,-name", "5,4,1,2,3")]
  [InlineData("-price,name", "3,2,1,4,5")]
  [InlineData("isAvailable,name", "5,2,4,3,1")]
  [InlineData("-isAvailable,-price,name", "3,1,4,2,5")]
  public Task Sort_SeveralProperties_ReturnsOrdered(string sort, string expected) => this.AssertOrderAsync($"sort={sort}", expected);

  [Theory]
  [InlineData("category.name,-price,name", "3,2,1,4,5")]
  [InlineData("-category.name,price,name", "4,5,1,2,3")]
  [InlineData("category.id,id", "1,2,3,4,5")]
  public Task Sort_NestedObjectProperty_ReturnsOrdered(string sort, string expected) => this.AssertOrderAsync($"sort={sort}", expected);

  [Theory]
  [InlineData("NAME", "4,3,1,5,2")]
  [InlineData("Category.Name,-Price,Name", "3,2,1,4,5")]
  [InlineData("price,%20-name", "5,4,1,2,3")]
  public Task Sort_CaseAndSpaces_AreIgnored(string sort, string expected) => this.AssertOrderAsync($"sort={sort}", expected);

  [Fact]
  public async Task Sort_WithFilter_SortsMatchingOnly()
  {
    await this.AssertOrderAsync("category.id.equals={Pizza}&sort=-price", "3,2,1");
  }

  [Fact]
  public async Task Sort_WithSearchOfCustomService_SortsMatchingOnly()
  {
    // The search of the custom service is combined with the sorting.
    await this.AssertOrderAsync("search=e&sort=-name", "2,5,1,3");
  }

  [Theory]
  [InlineData("nope")]
  [InlineData("category")]
  [InlineData("photos")]
  [InlineData("photos.filename")]
  [InlineData("category.nope")]
  [InlineData("category.products")]
  [InlineData(".name")]
  [InlineData("category..name")]
  public async Task Sort_InvalidPath_Returns400(string sort)
  {
    await AssertValidationErrorsAsync(await this.Anonymous.GetAsync($"/api/products?sort={sort}"), "sort");
  }

  [Fact]
  public async Task Paging_OffsetAndLimit_ReturnPageAndHeaders()
  {
    CatalogDataSet dataSet = this.Factory.DataSet;
    HttpResponseMessage response = await this.Anonymous.GetAsync("/api/products?sort=id&offset=1&limit=2");

    Assert.Equal(dataSet.Products("2,3"), GetIds(await ApiClient.ReadJsonAsync(response, HttpStatusCode.OK)));
    Assert.Equal("5", response.Headers.GetValues("Paging-Total-Count").Single());
    Assert.Equal("1", response.Headers.GetValues("Paging-Offset").Single());
    Assert.Equal("2", response.Headers.GetValues("Paging-Limit").Single());
  }

  [Fact]
  public async Task Paging_TotalCountRespectsFilter()
  {
    CatalogDataSet dataSet = this.Factory.DataSet;
    HttpResponseMessage response = await this.Anonymous.GetAsync($"/api/products?category.id.equals={dataSet.OtherIds["Pizza"]}&sort=-price&offset=1&limit=1");

    Assert.Equal(dataSet.Products("2"), GetIds(await ApiClient.ReadJsonAsync(response, HttpStatusCode.OK)));
    Assert.Equal("3", response.Headers.GetValues("Paging-Total-Count").Single());
  }

  [Fact]
  public async Task Paging_WithoutParameters_ReturnsAllWithoutPagingHeaders()
  {
    HttpResponseMessage response = await this.Anonymous.GetAsync("/api/products");

    Assert.Equal(5, GetIds(await ApiClient.ReadJsonAsync(response, HttpStatusCode.OK)).Count);
    Assert.Equal("5", response.Headers.GetValues("Paging-Total-Count").Single());
    Assert.False(response.Headers.Contains("Paging-Offset"));
    Assert.False(response.Headers.Contains("Paging-Limit"));
  }

  [Theory]
  [InlineData("count=none")]
  [InlineData("return=minimal, COUNT=\"None\"; strict")]
  public async Task Paging_PreferCountNone_SkipsTotalCount(string preferences)
  {
    HttpResponseMessage response = await this.Anonymous.WithHeader("Prefer", preferences).GetAsync("/api/products?sort=id&offset=1&limit=2");

    // The names are case-insensitive, the values may be quoted, and the other preferences and parameters are ignored.
    Assert.Equal(2, GetIds(await ApiClient.ReadJsonAsync(response, HttpStatusCode.OK)).Count);
    Assert.False(response.Headers.Contains("Paging-Total-Count"));
    Assert.Equal("count=none", response.Headers.GetValues("Preference-Applied").Single());
    Assert.Equal("1", response.Headers.GetValues("Paging-Offset").Single());
    Assert.Equal("2", response.Headers.GetValues("Paging-Limit").Single());
  }

  [Theory]
  [InlineData(null, null)]
  [InlineData("count=exact", "count=exact")]
  [InlineData("count=planned", null)]
  [InlineData("count=exact, count=none", "count=exact")]
  public async Task Paging_OtherCountPreferences_ReturnTotalCount(string? preferences, string? appliedPreferences)
  {
    ApiClient client = preferences == null ? this.Anonymous : this.Anonymous.WithHeader("Prefer", preferences);
    HttpResponseMessage response = await client.GetAsync("/api/products");

    // The total count is the default, an unsupported value is ignored, and only the first preference counts.
    Assert.Equal("5", response.Headers.GetValues("Paging-Total-Count").Single());
    Assert.Equal(appliedPreferences, response.Headers.TryGetValues("Preference-Applied", out IEnumerable<string>? values) ? values.Single() : null);
    Assert.Contains("Prefer", response.Headers.Vary);
  }

  [Theory]
  [InlineData("offset=-1", "offset")]
  [InlineData("limit=-1", "limit")]
  [InlineData("offset=-1&limit=-1", "offset,limit")]
  public async Task Paging_NegativeValues_Return400(string query, string parameters)
  {
    await AssertValidationErrorsAsync(await this.Anonymous.GetAsync($"/api/products?{query}"), parameters.Split(','));
  }

  [Fact]
  public async Task Paging_OffsetBeyondEnd_ReturnsEmptyList()
  {
    Assert.Empty(GetIds(await this.Anonymous.GetJsonAsync("/api/products?sort=id&offset=10")));
  }

  private async Task AssertOrderAsync(string query, string expected)
  {
    CatalogDataSet dataSet = this.Factory.DataSet;

    Assert.Equal(dataSet.Products(expected), GetIds(await this.Anonymous.GetJsonAsync($"/api/products?{dataSet.Resolve(query)}")));
  }
}
