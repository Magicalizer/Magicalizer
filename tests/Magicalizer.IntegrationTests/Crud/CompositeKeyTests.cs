// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using Magicalizer.IntegrationTests.Infrastructure;

namespace Magicalizer.IntegrationTests.Crud;

/// <summary>
/// The standard actions of the API of the entities with a composite primary key of two and three properties (the
/// controllers with the keys in the routes).
/// </summary>
public class CompositeKeyTests : IntegrationTest
{
  /// <summary>
  /// Creates the tests.
  /// </summary>
  public CompositeKeyTests(MagicalizerFactory factory) : base(factory) { }

  [Fact]
  public async Task Crud_TwoKeys_WorksEndToEnd()
  {
    int categoryId = (await this.Anonymous.PostJsonAsync("/api/categories", new { name = "Soups" }))["id"]!.GetValue<int>();
    int productId = (await this.Anonymous.PostJsonAsync("/api/products", new { category = new { id = categoryId }, name = "Chili soup", price = 1m }))["id"]!.GetValue<int>();
    int tagId = (await this.Anonymous.PostJsonAsync("/api/tags", new { name = "Spicy" }))["id"]!.GetValue<int>();
    string url = $"/api/product-tags/{productId}/{tagId}";
    HttpResponseMessage createResponse = await this.Anonymous.PostAsync("/api/product-tags", new { product = new { id = productId }, tag = new { id = tagId }, position = 1 });

    Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
    Assert.Equal(url, createResponse.Headers.Location!.ToString());
    Assert.Equal(1, (await this.Anonymous.GetJsonAsync(url))["position"]!.GetValue<int>());
    Assert.Equal(HttpStatusCode.NoContent, (await this.Anonymous.PutAsync("/api/product-tags", new { product = new { id = productId }, tag = new { id = tagId }, position = 2 })).StatusCode);
    Assert.Equal(2, (await this.Anonymous.GetJsonAsync(url))["position"]!.GetValue<int>());
    Assert.Equal(HttpStatusCode.NoContent, (await this.Anonymous.PatchAsync(url, new[] { new { op = "replace", path = "/position", value = 3 } })).StatusCode);

    JsonNode productTag = await this.Anonymous.GetJsonAsync($"{url}?include=product,tag");

    Assert.Equal(3, productTag["position"]!.GetValue<int>());
    Assert.Equal("Chili soup", productTag["product"]!["name"]!.GetValue<string>());
    Assert.Equal("Spicy", productTag["tag"]!["name"]!.GetValue<string>());
    Assert.Equal(HttpStatusCode.NoContent, (await this.Anonymous.DeleteAsync(url)).StatusCode);
    Assert.Equal(HttpStatusCode.NotFound, (await this.Anonymous.GetAsync(url)).StatusCode);
  }

  [Fact]
  public async Task Delete_NonexistentWithTwoKeys_Returns404()
  {
    Assert.Equal(HttpStatusCode.NotFound, (await this.Anonymous.DeleteAsync($"/api/product-tags/{NonexistentId}/{NonexistentId}")).StatusCode);
  }

  [Fact]
  public async Task Crud_ThreeKeysOfDifferentTypes_WorksEndToEnd()
  {
    ApiClient user = this.Anonymous.AsUser("keeper");
    string url = "/api/stocks/kyiv-1/42/7";
    HttpResponseMessage createResponse = await user.PostAsync("/api/stocks", new { warehouseCode = "kyiv-1", productId = 42, batchNumber = 7, quantity = 100 });

    Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
    Assert.Equal(url, createResponse.Headers.Location!.ToString());
    Assert.Equal(100, (await user.GetJsonAsync(url))["quantity"]!.GetValue<int>());
    Assert.Equal(HttpStatusCode.NoContent, (await user.PatchAsync(url, new[] { new { op = "replace", path = "/quantity", value = 90 } })).StatusCode);
    Assert.Equal(90, (await user.GetJsonAsync(url))["quantity"]!.GetValue<int>());

    // The other keys with one different part are other stocks.
    Assert.Equal(HttpStatusCode.NotFound, (await user.GetAsync("/api/stocks/kyiv-2/42/7")).StatusCode);
    Assert.Equal(HttpStatusCode.NotFound, (await user.GetAsync("/api/stocks/kyiv-1/43/7")).StatusCode);
    Assert.Equal(HttpStatusCode.NotFound, (await user.GetAsync("/api/stocks/kyiv-1/42/8")).StatusCode);
    Assert.Equal(HttpStatusCode.NoContent, (await user.DeleteAsync(url)).StatusCode);
    Assert.Equal(HttpStatusCode.NotFound, (await user.GetAsync(url)).StatusCode);
  }

  [Fact]
  public async Task Create_DuplicateKey_Returns409()
  {
    ApiClient user = this.Anonymous.AsUser("keeper");
    object stock = new { warehouseCode = "dnipro", productId = 1, batchNumber = 1, quantity = 5 };

    await user.PostJsonAsync("/api/stocks", stock);

    JsonNode problem = await ApiClient.ReadJsonAsync(await user.PostAsync("/api/stocks", stock), HttpStatusCode.Conflict);

    Assert.Equal("The model with the primary key (dnipro, 1, 1) already exists.", problem["detail"]!.GetValue<string>());
  }

  [Fact]
  public async Task Create_NonexistentReferences_Returns400WithErrorsOfThem()
  {
    await AssertValidationErrorsAsync(await this.Anonymous.PostAsync("/api/product-tags", new { product = new { id = NonexistentId }, tag = new { id = NonexistentId }, position = 1 }), "Product", "Tag");
  }

  [Fact]
  public async Task Put_Nonexistent_Returns404()
  {
    ApiClient user = this.Anonymous.AsUser("keeper");

    Assert.Equal(HttpStatusCode.NotFound, (await user.PutAsync("/api/stocks", new { warehouseCode = "nowhere", productId = 1, batchNumber = 1, quantity = 5 })).StatusCode);
    Assert.Equal(HttpStatusCode.NotFound, (await this.Anonymous.PutAsync("/api/product-tags", new { product = new { id = NonexistentId }, tag = new { id = NonexistentId }, position = 1 })).StatusCode);
  }

  [Fact]
  public async Task GetAll_FilterByKeyParts_ReturnsMatching()
  {
    ApiClient user = this.Anonymous.AsUser("keeper");

    await user.PostJsonAsync("/api/stocks", new { warehouseCode = "lviv", productId = 1, batchNumber = 1, quantity = 5 });
    await user.PostJsonAsync("/api/stocks", new { warehouseCode = "lviv", productId = 1, batchNumber = 2, quantity = 6 });
    await user.PostJsonAsync("/api/stocks", new { warehouseCode = "odesa", productId = 1, batchNumber = 1, quantity = 7 });

    JsonArray stocks = (await user.GetJsonAsync("/api/stocks?warehousecode.equals=lviv&sort=-batchnumber")).AsArray();

    Assert.Equal([6, 5], stocks.Select(s => s!["quantity"]!.GetValue<int>()));
  }
}
