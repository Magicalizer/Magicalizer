// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using Magicalizer.IntegrationTests.Infrastructure;

namespace Magicalizer.IntegrationTests.Catalog;

/// <summary>
/// The limits of the options on a known data set (see <see cref="LimitsTestsFactory"/>): the maximum number of models a
/// request gets and the maximum depth of the inclusions.
/// </summary>
public class LimitsTests : IntegrationTest<LimitsTestsFactory>
{
  /// <summary>
  /// Creates the tests.
  /// </summary>
  public LimitsTests(LimitsTestsFactory factory) : base(factory) { }

  [Fact]
  public async Task Limit_NotSet_IsMaxLimit()
  {
    CatalogDataSet dataSet = this.Factory.DataSet;
    HttpResponseMessage response = await this.Anonymous.GetAsync("/api/products?sort=id");

    Assert.Equal(dataSet.Products("1,2,3"), GetIds(await ApiClient.ReadJsonAsync(response, HttpStatusCode.OK)));
    Assert.Equal("5", response.Headers.GetValues("Paging-Total-Count").Single());
    Assert.Equal($"{LimitsTestsFactory.MaxLimit}", response.Headers.GetValues("Paging-Limit").Single());
  }

  [Fact]
  public async Task Limit_UpToMaxLimit_IsApplied()
  {
    CatalogDataSet dataSet = this.Factory.DataSet;

    Assert.Equal(dataSet.Products("1,2"), GetIds(await this.Anonymous.GetJsonAsync("/api/products?sort=id&limit=2")));
    Assert.Equal(dataSet.Products("4,5"), GetIds(await this.Anonymous.GetJsonAsync($"/api/products?sort=id&offset=3&limit={LimitsTestsFactory.MaxLimit}")));
  }

  [Fact]
  public async Task Limit_AboveMaxLimit_Returns400()
  {
    HttpResponseMessage response = await this.Anonymous.GetAsync($"/api/products?limit={LimitsTestsFactory.MaxLimit + 1}");
    JsonNode problem = await ApiClient.ReadJsonAsync(response, HttpStatusCode.BadRequest);

    Assert.Equal($"The limit can't be greater than {LimitsTestsFactory.MaxLimit}.", problem["errors"]!["limit"]![0]!.GetValue<string>());
  }

  [Fact]
  public async Task Limit_AboveMaxLimitWithOtherErrors_Returns400WithAllErrors()
  {
    await AssertValidationErrorsAsync(await this.Anonymous.GetAsync($"/api/products?limit={LimitsTestsFactory.MaxLimit + 1}&nope=1&price.from=abc"), "limit", "nope", "price.from");
  }

  [Fact]
  public async Task Limit_OfOverriddenAction_IsApplied()
  {
    // The custom controller of the photos overrides the action, but its limit has the binder of the standard one.
    Assert.Equal(LimitsTestsFactory.MaxLimit, (await this.Anonymous.GetJsonAsync("/api/photos")).AsArray().Count);
    await AssertValidationErrorsAsync(await this.Anonymous.GetAsync($"/api/photos?limit={LimitsTestsFactory.MaxLimit + 1}"), "limit");
  }

  [Theory]
  [InlineData("/api/products", "tags.tag")]
  [InlineData("/api/categories", "products.photos")]
  public async Task Include_UpToMaxDepth_IsApplied(string url, string include)
  {
    Assert.Equal(HttpStatusCode.OK, (await this.Anonymous.GetAsync($"{url}?include={include}")).StatusCode);
  }

  [Fact]
  public async Task Include_DeeperThanMaxDepth_Returns400()
  {
    HttpResponseMessage response = await this.Anonymous.GetAsync("/api/categories?include=products.tags.tag");
    JsonNode problem = await ApiClient.ReadJsonAsync(response, HttpStatusCode.BadRequest);

    Assert.Equal(
      $"Unable to include 'products.tags.tag': the property path has more than {LimitsTestsFactory.MaxInclusionDepth} properties.",
      problem["errors"]!["include"]![0]!.GetValue<string>()
    );
  }
}
