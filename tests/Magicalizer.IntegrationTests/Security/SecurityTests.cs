// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Magicalizer.IntegrationTests.Infrastructure;
using Magicalizer.TestHost.Products;
using Microsoft.EntityFrameworkCore;

namespace Magicalizer.IntegrationTests.Security;

/// <summary>
/// The authentication and authorization rules of the DTOs, the supported HTTP methods and the query prefilters.
/// </summary>
public class SecurityTests : IntegrationTest
{
  /// <summary>
  /// Creates the tests.
  /// </summary>
  public SecurityTests(MagicalizerFactory factory) : base(factory) { }

  [Fact]
  public async Task AuthenticatedOnly_Anonymous_Returns401()
  {
    Assert.Equal(HttpStatusCode.Unauthorized, (await this.Anonymous.GetAsync("/api/notes")).StatusCode);
    Assert.Equal(HttpStatusCode.Unauthorized, (await this.Anonymous.GetAsync($"/api/notes/{NonexistentId}")).StatusCode);
    Assert.Equal(HttpStatusCode.OK, (await this.Anonymous.AsUser("reader").GetAsync("/api/notes")).StatusCode);
  }

  [Fact]
  public async Task AuthenticatedOnly_AnyMethod_ProtectsAllActions()
  {
    Assert.Equal(HttpStatusCode.Unauthorized, (await this.Anonymous.GetAsync("/api/stocks")).StatusCode);
    Assert.Equal(HttpStatusCode.Unauthorized, (await this.Anonymous.PostAsync("/api/stocks", new { warehouseCode = "kyiv", productId = 1, batchNumber = 1 })).StatusCode);
    Assert.Equal(HttpStatusCode.Unauthorized, (await this.Anonymous.DeleteAsync("/api/stocks/kyiv/1/1")).StatusCode);
  }

  [Fact]
  public async Task AuthorizedOnly_WithoutPermission_Returns403()
  {
    ApiClient reader = this.Anonymous.AsUser("reader");
    int id = await this.CreateNoteAsync("Secret recipe");

    Assert.Equal(HttpStatusCode.Forbidden, (await reader.PostAsync("/api/notes", new { text = "Stolen recipe" })).StatusCode);
    Assert.Equal(HttpStatusCode.Forbidden, (await reader.PutAsync("/api/notes", new { id, text = "Spoiled recipe" })).StatusCode);
    Assert.Equal(HttpStatusCode.Forbidden, (await reader.PatchAsync($"/api/notes/{id}", new[] { new { op = "replace", path = "/text", value = "Spoiled recipe" } })).StatusCode);
    Assert.Equal(HttpStatusCode.Forbidden, (await reader.DeleteAsync($"/api/notes/{id}")).StatusCode);
    Assert.Equal(HttpStatusCode.Unauthorized, (await this.Anonymous.PostAsync("/api/notes", new { text = "Stolen recipe" })).StatusCode);
    Assert.Equal("Secret recipe", (await reader.GetJsonAsync($"/api/notes/{id}"))["text"]!.GetValue<string>());
  }

  [Fact]
  public async Task AuthorizedOnly_WithPermission_Succeeds()
  {
    ApiClient editor = this.Anonymous.AsUser("editor", "edit");
    int id = await this.CreateNoteAsync("Draft recipe");

    Assert.Equal(HttpStatusCode.NoContent, (await editor.PutAsync("/api/notes", new { id, text = "Final recipe" })).StatusCode);
    Assert.Equal(HttpStatusCode.NoContent, (await editor.PatchAsync($"/api/notes/{id}", new[] { new { op = "replace", path = "/text", value = "Perfect recipe" } })).StatusCode);
    Assert.Equal("Perfect recipe", (await editor.GetJsonAsync($"/api/notes/{id}"))["text"]!.GetValue<string>());
  }

  [Fact]
  public async Task AuthorizedOnly_SeveralPolicies_AllMustBeSatisfied()
  {
    int id = await this.CreateNoteAsync("Forgotten recipe");

    Assert.Equal(HttpStatusCode.Forbidden, (await this.Anonymous.AsUser("editor", "edit").DeleteAsync($"/api/notes/{id}")).StatusCode);
    Assert.Equal(HttpStatusCode.Forbidden, (await this.Anonymous.AsUser("cleaner", "delete").DeleteAsync($"/api/notes/{id}")).StatusCode);
    Assert.Equal(HttpStatusCode.NoContent, (await this.Anonymous.AsUser("admin", "edit", "delete").DeleteAsync($"/api/notes/{id}")).StatusCode);
  }

  [Fact]
  public async Task UnsupportedMethods_Requests_Return405()
  {
    int id = (await this.Anonymous.PostJsonAsync("/api/tags", new { name = "Seasonal" }))["id"]!.GetValue<int>();

    Assert.Equal("Seasonal", (await this.Anonymous.GetJsonAsync($"/api/tags/{id}"))["name"]!.GetValue<string>());
    Assert.Equal(HttpStatusCode.MethodNotAllowed, (await this.Anonymous.PutAsync("/api/tags", new { id, name = "Everyday" })).StatusCode);
    Assert.Equal(HttpStatusCode.MethodNotAllowed, (await this.Anonymous.PatchAsync($"/api/tags/{id}", new[] { new { op = "replace", path = "/name", value = "Everyday" } })).StatusCode);
    Assert.Equal(HttpStatusCode.MethodNotAllowed, (await this.Anonymous.DeleteAsync($"/api/tags/{id}")).StatusCode);
    Assert.Equal("Seasonal", (await this.Anonymous.GetJsonAsync($"/api/tags/{id}"))["name"]!.GetValue<string>());
  }

  [Fact]
  public async Task QueryPrefilter_UserWithoutPermission_DoesntSeeArchived()
  {
    string name = Unique("Stew");
    int archivedId = await this.CreateProductAsync($"{name} (archived)", isArchived: true);
    int activeId = await this.CreateProductAsync(name, isArchived: false);
    ApiClient archivist = this.Anonymous.AsUser("archivist", "archive");

    Assert.Equal([activeId], GetIds(await this.Anonymous.GetJsonAsync($"/api/products?name.contains={name}")));
    Assert.Equal(HttpStatusCode.NotFound, (await this.Anonymous.GetAsync($"/api/products/{archivedId}")).StatusCode);
    Assert.Equal([activeId, archivedId], GetIds(await archivist.GetJsonAsync($"/api/products?name.contains={name}&sort=name")));
    Assert.Equal(HttpStatusCode.OK, (await archivist.GetAsync($"/api/products/{archivedId}")).StatusCode);
  }

  [Fact]
  public async Task QueryPrefilter_FilterOfArchived_DoesntBypassIt()
  {
    string name = Unique("Pie");

    await this.CreateProductAsync(name, isArchived: true);

    HttpResponseMessage response = await this.Anonymous.GetAsync($"/api/products?name.contains={name}&isarchived.equals=true");

    Assert.Empty(GetIds(await ApiClient.ReadJsonAsync(response, HttpStatusCode.OK)));
    Assert.Equal("0", response.Headers.GetValues("Paging-Total-Count").Single());
  }

  [Fact]
  public async Task QueryPrefilter_PutOfHidden_Returns404()
  {
    int archivedId = await this.CreateProductAsync("Rye bread", isArchived: true);
    int categoryId = await this.CreateCategoryAsync();
    HttpResponseMessage response = await this.Anonymous.PutAsync("/api/products", new { id = archivedId, category = new { id = categoryId }, name = "Wheat bread", price = 1m, created = DateTime.UtcNow });

    // The update reads the object through the prefilter first, so a hidden one doesn't exist for the user.
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    Assert.Equal("Rye bread", (await this.Anonymous.AsUser("archivist", "archive").GetJsonAsync($"/api/products/{archivedId}"))["name"]!.GetValue<string>());
  }

  [Fact]
  public async Task QueryPrefilter_PatchOfHidden_Returns404()
  {
    int archivedId = await this.CreateProductAsync("Honey cake", isArchived: true);

    // The patch reads the object through the prefilter first.
    Assert.Equal(HttpStatusCode.NotFound, (await this.Anonymous.PatchAsync($"/api/products/{archivedId}", new[] { new { op = "replace", path = "/isArchived", value = false } })).StatusCode);
  }

  [Fact]
  public async Task QueryPrefilter_DeleteOfHidden_Returns404()
  {
    int archivedId = await this.CreateProductAsync("Hawaiian pizza", isArchived: true);

    // The deletion must read the object through the prefilter first too, as a hidden one doesn't exist for the user.
    Assert.Equal(HttpStatusCode.NotFound, (await this.Anonymous.DeleteAsync($"/api/products/{archivedId}")).StatusCode);
    Assert.Equal(HttpStatusCode.OK, (await this.Anonymous.AsUser("archivist", "archive").GetAsync($"/api/products/{archivedId}")).StatusCode);
  }

  private async Task<int> CreateNoteAsync(string text)
  {
    return (await this.Anonymous.AsUser("editor", "edit").PostJsonAsync("/api/notes", new { text }))["id"]!.GetValue<int>();
  }

  // Creates the product directly in the database, as only a user with the permission could see an archived one.
  private async Task<int> CreateProductAsync(string name, bool isArchived)
  {
    int categoryId = await this.CreateCategoryAsync();
    ProductEntity product = new ProductEntity { CategoryId = categoryId, Name = name, Price = 1m, IsArchived = isArchived };

    await this.Factory.WithDbAsync(async db => {
      db.Add(product);
      await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    });

    return product.Id;
  }

  private async Task<int> CreateCategoryAsync()
  {
    return (await this.Anonymous.PostJsonAsync("/api/categories", new { name = "Archive" }))["id"]!.GetValue<int>();
  }
}
