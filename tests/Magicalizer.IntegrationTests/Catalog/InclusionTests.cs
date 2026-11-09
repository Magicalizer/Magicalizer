// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Magicalizer.IntegrationTests.Infrastructure;

namespace Magicalizer.IntegrationTests.Catalog;

/// <summary>
/// The inclusions of related objects of the standard API on a known data set (see <see cref="CatalogTestsFactory"/>):
/// references, collections and nested ones.
/// </summary>
public class InclusionTests : IntegrationTest<CatalogTestsFactory>
{
  /// <summary>
  /// Creates the tests.
  /// </summary>
  public InclusionTests(CatalogTestsFactory factory) : base(factory) { }

  [Fact]
  public async Task Include_Nothing_ReturnsReferencesByIdOnly()
  {
    CatalogDataSet dataSet = this.Factory.DataSet;
    JsonNode product = await this.GetProductAsync(1, null);

    Assert.Equal(dataSet.OtherIds["Pizza"], product["category"]!["id"]!.GetValue<int>());
    Assert.Null(product["category"]!["name"]);
    Assert.Null(product["photos"]);
    Assert.Null(product["tags"]);
  }

  [Fact]
  public async Task Include_Reference_ReturnsItWithoutOtherOnes()
  {
    JsonNode product = await this.GetProductAsync(1, "category");

    Assert.Equal("Pizza", product["category"]!["name"]!.GetValue<string>());
    Assert.Equal("Italian", product["category"]!["description"]!.GetValue<string>());
    Assert.Null(product["photos"]);
  }

  [Fact]
  public async Task Include_Collection_ReturnsAllElements()
  {
    JsonNode product = await this.GetProductAsync(1, "photos");

    Assert.Equal(["margherita-1.jpg", "margherita-2.jpg"], product["photos"]!.AsArray().Select(p => p!["filename"]!.GetValue<string>()).Order(StringComparer.Ordinal));
    Assert.Empty((await this.GetProductAsync(3, "photos"))["photos"]!.AsArray());
  }

  [Fact]
  public async Task Include_Several_ReturnsAllOfThem()
  {
    JsonNode product = await this.GetProductAsync(2, "category,photos,tags.tag");

    Assert.Equal("Pizza", product["category"]!["name"]!.GetValue<string>());
    Assert.Equal("pepperoni-1.jpg", product["photos"]!.AsArray().Single()!["filename"]!.GetValue<string>());
    Assert.Equal(["Hot", "New"], product["tags"]!.AsArray().Select(t => t!["tag"]!["name"]!.GetValue<string>()).Order(StringComparer.Ordinal));
  }

  [Fact]
  public async Task Include_NestedCollections_ReturnsWholeTree()
  {
    CatalogDataSet dataSet = this.Factory.DataSet;
    JsonNode category = await this.Anonymous.GetJsonAsync($"/api/categories/{dataSet.OtherIds["Pizza"]}?include=products.photos");
    JsonArray products = category["products"]!.AsArray();

    Assert.Equal(dataSet.Products("1,2,3"), GetIds(products).Order());
    Assert.Equal(2, products.Single(p => p!["name"]!.GetValue<string>() == "Margherita")!["photos"]!.AsArray().Count);
  }

  [Fact]
  public async Task Include_NestedReferences_ReturnsWholeChain()
  {
    CatalogDataSet dataSet = this.Factory.DataSet;
    JsonNode photo = await this.Anonymous.GetJsonAsync($"/api/photos/{dataSet.OtherIds["borscht-1"]}?include=product.category");

    Assert.Equal("Borscht", photo["product"]!["name"]!.GetValue<string>());
    Assert.Equal("Soups", photo["product"]!["category"]!["name"]!.GetValue<string>());
  }

  [Fact]
  public async Task Include_InList_IncludesForEachElement()
  {
    JsonArray products = (await this.Anonymous.GetJsonAsync("/api/products?include=category&sort=id")).AsArray();

    Assert.Equal(["Pizza", "Pizza", "Pizza", "Soups", "Soups"], products.Select(p => p!["category"]!["name"]!.GetValue<string>()));
  }

  [Fact]
  public async Task Include_CaseAndSpaces_AreIgnored()
  {
    JsonNode product = await this.GetProductAsync(2, "Category,%20Tags.Tag");

    Assert.Equal("Pizza", product["category"]!["name"]!.GetValue<string>());
    Assert.Equal(2, product["tags"]!.AsArray().Count);
  }

  [Theory]
  [InlineData("nope")]
  [InlineData("name")]
  [InlineData("category.nope")]
  [InlineData("category.name")]
  [InlineData("category..products")]
  public async Task Include_InvalidPath_Returns400(string include)
  {
    await AssertValidationErrorsAsync(await this.Anonymous.GetAsync($"/api/products?include={include}"), "include");
    await AssertValidationErrorsAsync(await this.Anonymous.GetAsync($"/api/products/{this.Factory.DataSet.ProductIds[0]}?include={include}"), "include");
  }

  [Theory]
  [InlineData("/api/products", "category.products")]
  [InlineData("/api/products", "photos.product")]
  [InlineData("/api/products", "tags.product")]
  [InlineData("/api/categories", "products.category")]
  public async Task Include_Cycle_Returns400(string url, string include)
  {
    // EF Core doesn't include a navigation that leads back by the inverse of the previous one in no-tracking queries.
    await AssertValidationErrorsAsync(await this.Anonymous.GetAsync($"{url}?include={include}"), "include");
  }

  private Task<JsonNode> GetProductAsync(int number, string? include)
  {
    string url = $"/api/products/{this.Factory.DataSet.ProductIds[number - 1]}";

    return this.Anonymous.GetJsonAsync(include == null ? url : $"{url}?include={include}");
  }
}
