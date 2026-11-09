// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using Magicalizer.Domain;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.IntegrationTests.Infrastructure;
using Magicalizer.TestHost.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer.IntegrationTests.Crud;

/// <summary>
/// The standard actions of the API (create, get, update, patch, delete) of the entities with a primary key of one
/// property, and the validation of the DTOs and the models.
/// </summary>
public class CrudTests : IntegrationTest
{
  /// <summary>
  /// Creates the tests.
  /// </summary>
  public CrudTests(MagicalizerFactory factory) : base(factory) { }

  [Fact]
  public async Task Crud_ValidCategory_WorksEndToEnd()
  {
    HttpResponseMessage createResponse = await this.Anonymous.PostAsync("/api/categories", new { name = "Salads", description = "Fresh" });
    int id = (await ApiClient.ReadJsonAsync(createResponse, HttpStatusCode.Created))["id"]!.GetValue<int>();

    Assert.Equal($"/api/categories/{id}", createResponse.Headers.Location!.ToString());
    Assert.Equal("Salads", (await this.Anonymous.GetJsonAsync($"/api/categories/{id}"))["name"]!.GetValue<string>());
    Assert.Equal(HttpStatusCode.NoContent, (await this.Anonymous.PutAsync("/api/categories", new { id, name = "Green salads", description = "Fresh" })).StatusCode);
    Assert.Equal(HttpStatusCode.NoContent, (await this.Anonymous.PatchAsync($"/api/categories/{id}", new[] { new { op = "replace", path = "/description", value = "Very fresh" } })).StatusCode);

    JsonNode edited = await this.Anonymous.GetJsonAsync($"/api/categories/{id}");

    Assert.Equal("Green salads", edited["name"]!.GetValue<string>());
    Assert.Equal("Very fresh", edited["description"]!.GetValue<string>());
    Assert.Equal(HttpStatusCode.NoContent, (await this.Anonymous.DeleteAsync($"/api/categories/{id}")).StatusCode);
    Assert.Equal(HttpStatusCode.NotFound, (await this.Anonymous.GetAsync($"/api/categories/{id}")).StatusCode);
  }

  [Fact]
  public async Task Create_ValidProduct_ReturnsItWithKey()
  {
    int categoryId = await this.CreateCategoryAsync("Desserts");
    JsonNode product = await this.Anonymous.PostJsonAsync("/api/products", new { category = new { id = categoryId }, name = "Tiramisu", price = 7.5m, quantity = 3 });

    Assert.True(product["id"]!.GetValue<int>() > 0);
    Assert.Equal(categoryId, product["category"]!["id"]!.GetValue<int>());
    Assert.Equal(7.5m, product["price"]!.GetValue<decimal>());
  }

  [Fact]
  public async Task Create_GeneratedKeyFromClient_IsIgnored()
  {
    int existingId = await this.CreateCategoryAsync("Pastries");
    JsonNode created = await this.Anonymous.PostJsonAsync("/api/categories", new { id = existingId, name = "Pies" });

    // A new object is created instead of overwriting the existing one or failing.
    Assert.NotEqual(existingId, created["id"]!.GetValue<int>());
    Assert.Equal("Pastries", (await this.Anonymous.GetJsonAsync($"/api/categories/{existingId}"))["name"]!.GetValue<string>());
  }

  [Fact]
  public async Task Create_NonNullablePropertyWithoutValue_IsAllowed()
  {
    // The description is a non-nullable string, so ASP.NET Core would require it, but only FluentValidation validates
    // the DTOs.
    Assert.Equal(HttpStatusCode.Created, (await this.Anonymous.PostAsync("/api/categories", new { name = "Breads" })).StatusCode);
  }

  [Fact]
  public async Task Create_InvalidDto_Returns400WithErrorsByProperty()
  {
    await AssertValidationErrorsAsync(await this.Anonymous.PostAsync("/api/products", new { name = "", price = -1m }), "Category", "Name", "Price");
  }

  [Fact]
  public async Task Create_InvalidDtoAndQuery_Returns400WithAllErrors()
  {
    // The errors of the query and the DTO are collected before the action is executed.
    await AssertValidationErrorsAsync(await this.Anonymous.PostAsync("/api/categories?nope=1", new { name = "" }), "nope", "Name");
  }

  [Fact]
  public async Task Create_InvalidJson_Returns400()
  {
    Assert.Equal(HttpStatusCode.BadRequest, (await this.Anonymous.PostAsync("/api/categories", "{ \"name\": ")).StatusCode);
  }

  [Fact]
  public async Task Create_InvalidModel_Returns400FromService()
  {
    int categoryId = await this.CreateCategoryAsync("Delicacies");

    // The DTO is valid, but the validator of the model in the service limits the price to a million.
    await AssertValidationErrorsAsync(await this.Anonymous.PostAsync("/api/products", new { category = new { id = categoryId }, name = "Golden caviar", price = 1_000_001m }), "Price");
  }

  [Fact]
  public async Task ModelRuleSets_CreateAndEdit_AreValidatedByService()
  {
    int categoryId = await this.CreateCategoryAsync("Breads");
    string tomorrow = DateTime.UtcNow.AddDays(1).ToString("O");

    // A new product can't be created in the future, and an edited one keeps its creation time (the DTO validator
    // doesn't have these rules).
    await AssertValidationErrorsAsync(await this.Anonymous.PostAsync("/api/products", new { category = new { id = categoryId }, name = "Tomorrow's bread", price = 1m, created = tomorrow }), "Created");

    int id = await this.CreateProductAsync(categoryId, "Today's bread");

    await AssertValidationErrorsAsync(await this.Anonymous.PutAsync("/api/products", new { id, category = new { id = categoryId }, name = "Today's bread", price = 1m }), "Created");
    Assert.Equal(HttpStatusCode.NoContent, (await this.Anonymous.PutAsync("/api/products", new { id, category = new { id = categoryId }, name = "Today's bread", price = 1m, created = tomorrow })).StatusCode);
  }

  [Fact]
  public async Task CreateAndEdit_NonexistentReference_Returns400WithErrorOfIt()
  {
    // The database rejects the product by the foreign key, and the service reports the reference that doesn't exist.
    HttpResponseMessage createResponse = await this.Anonymous.PostAsync("/api/products", new { category = new { id = NonexistentId }, name = "Ghost pie", price = 1m });
    JsonNode problem = await ApiClient.ReadJsonAsync(createResponse, HttpStatusCode.BadRequest);

    Assert.Equal($"The referenced model with the primary key ({NonexistentId}) doesn't exist.", problem["errors"]!["Category"]![0]!.GetValue<string>());

    int categoryId = await this.CreateCategoryAsync("Ghosts");
    int id = await this.CreateProductAsync(categoryId, "Ghost cake");

    await AssertValidationErrorsAsync(await this.Anonymous.PutAsync("/api/products", new { id, category = new { id = NonexistentId }, name = "Ghost cake", price = 1m, created = DateTime.UtcNow.ToString("O") }), "Category");
  }

  [Fact]
  public async Task Get_Nonexistent_Returns404()
  {
    Assert.Equal(HttpStatusCode.NotFound, (await this.Anonymous.GetAsync($"/api/categories/{NonexistentId}")).StatusCode);
  }

  [Fact]
  public async Task Put_ValidProduct_ReplacesAllFields()
  {
    int categoryId = await this.CreateCategoryAsync("Breakfasts");
    JsonNode created = await this.Anonymous.PostJsonAsync("/api/products", new { category = new { id = categoryId }, name = "Omelette", price = 5m, quantity = 2, description = "Eggs" });
    int id = created["id"]!.GetValue<int>();

    await this.Anonymous.PutAsync("/api/products", new { id, category = new { id = categoryId }, name = "Omelette", price = 6m, created = created["created"] });

    JsonNode product = await this.Anonymous.GetJsonAsync($"/api/products/{id}");

    // The fields that aren't sent get their default values, unlike the patch.
    Assert.Equal(6m, product["price"]!.GetValue<decimal>());
    Assert.Equal(0, product["quantity"]!.GetValue<int>());
    Assert.Null(product["description"]);
  }

  [Fact]
  public async Task Put_Nonexistent_Returns404()
  {
    HttpResponseMessage response = await this.Anonymous.PutAsync("/api/categories", new { id = NonexistentId, name = "Ghosts" });
    JsonNode problem = await ApiClient.ReadJsonAsync(response, HttpStatusCode.NotFound);

    Assert.Equal($"The model with the primary key ({NonexistentId}) doesn't exist.", problem["detail"]!.GetValue<string>());
    Assert.Equal(HttpStatusCode.NotFound, (await this.Anonymous.GetAsync($"/api/categories/{NonexistentId}")).StatusCode);
  }

  [Fact]
  public async Task Put_WithoutId_Returns400()
  {
    // The ID isn't required on creation, but it's required on editing (the rule sets of the DTO validator).
    Assert.Equal(HttpStatusCode.Created, (await this.Anonymous.PostAsync("/api/categories", new { name = "Sauces" })).StatusCode);
    await AssertValidationErrorsAsync(await this.Anonymous.PutAsync("/api/categories", new { name = "Sauces" }), "Id");
  }

  [Fact]
  public async Task Patch_ValidOperations_ChangeOnlyPatchedFields()
  {
    int categoryId = await this.CreateCategoryAsync("Snacks");
    int id = (await this.Anonymous.PostJsonAsync("/api/products", new { category = new { id = categoryId }, name = "Nachos", price = 4m, quantity = 8, description = "Spicy" }))["id"]!.GetValue<int>();

    Assert.Equal(HttpStatusCode.NoContent, (await this.Anonymous.PatchAsync($"/api/products/{id}", new object[] {
      new { op = "replace", path = "/price", value = 4.5m },
      new { op = "remove", path = "/description" }
    })).StatusCode);

    JsonNode product = await this.Anonymous.GetJsonAsync($"/api/products/{id}");

    Assert.Equal("Nachos", product["name"]!.GetValue<string>());
    Assert.Equal(4.5m, product["price"]!.GetValue<decimal>());
    Assert.Equal(8, product["quantity"]!.GetValue<int>());
    Assert.Null(product["description"]);
  }

  [Theory]
  [InlineData("/nope", 1)]
  [InlineData("/price", -1)]
  [InlineData("/name", "")]
  public async Task Patch_Invalid_Returns400(string path, object value)
  {
    int categoryId = await this.CreateCategoryAsync("Soups");
    int id = await this.CreateProductAsync(categoryId, "Lentil soup");

    // An unknown path is a client error, and the patched DTO is validated as the updated one.
    Assert.Equal(HttpStatusCode.BadRequest, (await this.Anonymous.PatchAsync($"/api/products/{id}", new[] { new { op = "replace", path, value } })).StatusCode);
    Assert.Equal("Lentil soup", (await this.Anonymous.GetJsonAsync($"/api/products/{id}"))["name"]!.GetValue<string>());
  }

  [Fact]
  public async Task Patch_ChangeOfKey_Returns400()
  {
    int categoryId = await this.CreateCategoryAsync("Pies");
    int appleId = await this.CreateProductAsync(categoryId, "Apple pie");
    int cherryId = await this.CreateProductAsync(categoryId, "Cherry pie");
    HttpResponseMessage response = await this.Anonymous.PatchAsync($"/api/products/{appleId}", new object[] {
      new { op = "replace", path = "/id", value = cherryId },
      new { op = "replace", path = "/name", value = "Plum pie" }
    });

    // The patch of one object must not change another one.
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    Assert.Equal("Cherry pie", (await this.Anonymous.GetJsonAsync($"/api/products/{cherryId}"))["name"]!.GetValue<string>());
  }

  [Fact]
  public async Task Patch_Nonexistent_Returns404()
  {
    Assert.Equal(HttpStatusCode.NotFound, (await this.Anonymous.PatchAsync($"/api/categories/{NonexistentId}", new[] { new { op = "replace", path = "/name", value = "Ghosts" } })).StatusCode);
  }

  [Fact]
  public async Task Delete_Nonexistent_Returns404()
  {
    Assert.Equal(HttpStatusCode.NotFound, (await this.Anonymous.DeleteAsync($"/api/categories/{NonexistentId}")).StatusCode);
  }

  [Fact]
  public async Task Delete_Referenced_Returns409()
  {
    int categoryId = await this.CreateCategoryAsync("Pasta");

    await this.CreateProductAsync(categoryId, "Carbonara");

    // The database restricts the deletion of a category with products.
    Assert.Equal(HttpStatusCode.Conflict, (await this.Anonymous.DeleteAsync($"/api/categories/{categoryId}")).StatusCode);
    Assert.Equal(HttpStatusCode.OK, (await this.Anonymous.GetAsync($"/api/categories/{categoryId}")).StatusCode);
  }

  [Fact]
  public async Task Service_GetAll_ReturnsMaterializedModels()
  {
    await this.CreateCategoryAsync("Spices");

    using IServiceScope scope = this.Factory.Services.CreateScope();
    IEnumerable<CategoryModel> categories = await GetService(scope).GetAllAsync(cancellationToken: TestContext.Current.CancellationToken);

    // The models are converted once, so each enumeration returns the same objects.
    Assert.Same(categories.First(), categories.First());
  }

  [Fact]
  public async Task Service_EditOfNonexistent_ThrowsNotFound()
  {
    using IServiceScope scope = this.Factory.Services.CreateScope();
    ModelNotFoundException exception = await Assert.ThrowsAsync<ModelNotFoundException>(() => GetService(scope).EditAsync(new CategoryModel { Id = NonexistentId, Name = "Ghosts" }, TestContext.Current.CancellationToken));

    Assert.Equal(typeof(CategoryModel), exception.ModelType);
    Assert.Equal([NonexistentId], exception.KeyValues);
  }

  [Fact]
  public async Task Service_DeleteOfNonexistent_ThrowsNotFound()
  {
    using IServiceScope scope = this.Factory.Services.CreateScope();

    await Assert.ThrowsAsync<ModelNotFoundException>(() => GetService(scope).DeleteAsync(NonexistentId, TestContext.Current.CancellationToken));
  }

  [Fact]
  public async Task Service_DeleteOfReferenced_ThrowsInUse()
  {
    int categoryId = await this.CreateCategoryAsync("Noodles");

    await this.CreateProductAsync(categoryId, "Ramen");

    using IServiceScope scope = this.Factory.Services.CreateScope();
    ModelInUseException exception = await Assert.ThrowsAsync<ModelInUseException>(() => GetService(scope).DeleteAsync(categoryId, TestContext.Current.CancellationToken));

    // The database error is kept as the cause.
    Assert.IsType<DbUpdateException>(exception.InnerException, exactMatch: false);
  }

  [Fact]
  public async Task Service_CanceledGetAll_Throws()
  {
    using IServiceScope scope = this.Factory.Services.CreateScope();

    await Assert.ThrowsAnyAsync<OperationCanceledException>(() => GetService(scope).GetAllAsync(cancellationToken: new CancellationToken(canceled: true)));
  }

  [Fact]
  public async Task Service_CanceledDelete_Throws()
  {
    int id = await this.CreateCategoryAsync("Leftovers");
    using IServiceScope scope = this.Factory.Services.CreateScope();

    // The cancellation isn't a failure of the deletion.
    await Assert.ThrowsAnyAsync<OperationCanceledException>(() => GetService(scope).DeleteAsync(id, new CancellationToken(canceled: true)));
    Assert.Equal(HttpStatusCode.OK, (await this.Anonymous.GetAsync($"/api/categories/{id}")).StatusCode);
  }

  [Fact]
  public async Task Service_InvalidSortingFromCode_Throws()
  {
    using IServiceScope scope = this.Factory.Services.CreateScope();

    // The API checks the sortings of the requests, so an invalid one in a service call is an error of the code (500).
    await Assert.ThrowsAsync<Data.InvalidSortingException>(() => GetService(scope).GetAllAsync(sortings: [new Sorting<CategoryModel>(true, "nope")], cancellationToken: TestContext.Current.CancellationToken));
  }

  private async Task<int> CreateCategoryAsync(string name)
  {
    return (await this.Anonymous.PostJsonAsync("/api/categories", new { name }))["id"]!.GetValue<int>();
  }

  private async Task<int> CreateProductAsync(int categoryId, string name)
  {
    return (await this.Anonymous.PostJsonAsync("/api/products", new { category = new { id = categoryId }, name, price = 1m }))["id"]!.GetValue<int>();
  }

  private static IService<int, CategoryModel, CategoryFilter> GetService(IServiceScope scope)
  {
    return scope.ServiceProvider.GetRequiredService<IService<int, CategoryModel, CategoryFilter>>();
  }
}
