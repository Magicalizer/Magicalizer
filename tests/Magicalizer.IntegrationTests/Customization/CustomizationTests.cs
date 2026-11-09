// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.IntegrationTests.Infrastructure;
using Magicalizer.TestHost.Photos;
using Magicalizer.TestHost.Products;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer.IntegrationTests.Customization;

/// <summary>
/// The custom services and controllers an app registers instead of the default ones of Magicalizer.
/// </summary>
public class CustomizationTests : IntegrationTest
{
  /// <summary>
  /// Creates the tests.
  /// </summary>
  public CustomizationTests(MagicalizerFactory factory) : base(factory) { }

  [Fact]
  public void CustomService_Registration_ReplacesDefaultOne()
  {
    using IServiceScope scope = this.Factory.Services.CreateScope();

    Assert.IsType<ProductService>(scope.ServiceProvider.GetRequiredService<IService<int, ProductModel, ProductFilter>>());
    Assert.IsType<ProductService>(scope.ServiceProvider.GetRequiredService<IProductService>());
  }

  [Fact]
  public void DefaultService_WithoutCustomOne_IsRegistered()
  {
    using IServiceScope scope = this.Factory.Services.CreateScope();

    Assert.IsType<Domain.Services.Service<int, PhotoEntity, PhotoModel, PhotoFilter>>(scope.ServiceProvider.GetRequiredService<IService<int, PhotoModel, PhotoFilter>>());
  }

  [Fact]
  public async Task CustomService_Requests_UseIt()
  {
    string name = Unique("Baguette");
    int categoryId = await this.CreateCategoryAsync();
    DateTime before = DateTime.UtcNow.AddSeconds(-1);
    JsonNode product = await this.Anonymous.PostJsonAsync("/api/products", new { category = new { id = categoryId }, name, price = 2m });

    // The custom service sets the creation time and converts the search to the name condition.
    Assert.True(product["created"]!.GetValue<DateTime>() >= before);
    Assert.Equal([product["id"]!.GetValue<int>()], GetIds(await this.Anonymous.GetJsonAsync($"/api/products?search={name}")));
  }

  [Fact]
  public async Task CustomController_OverriddenAction_ReplacesStandardOne()
  {
    int photoId = await this.CreatePhotoAsync("CROISSANT.JPG");

    // The custom controller saves the filenames in lower case.
    Assert.Equal("croissant.jpg", (await this.Anonymous.GetJsonAsync($"/api/photos/{photoId}"))["filename"]!.GetValue<string>());
  }

  [Fact]
  public async Task CustomController_InheritedAndOwnActions_AreAvailable()
  {
    int photoId = await this.CreatePhotoAsync("bagel.jpg");
    int count = (await this.Anonymous.GetJsonAsync("/api/photos/count")).GetValue<int>();

    Assert.Contains(photoId, GetIds(await this.Anonymous.GetJsonAsync("/api/photos")));
    Assert.Equal(GetIds(await this.Anonymous.GetJsonAsync("/api/photos")).Count, count);
    Assert.Equal(HttpStatusCode.NoContent, (await this.Anonymous.PatchAsync($"/api/photos/{photoId}", new[] { new { op = "replace", path = "/filename", value = "bagels.jpg" } })).StatusCode);
  }

  [Fact]
  public async Task CustomController_AuthenticationRulesOfDto_AreApplied()
  {
    int photoId = await this.CreatePhotoAsync("muffin.jpg");

    Assert.Equal(HttpStatusCode.Unauthorized, (await this.Anonymous.DeleteAsync($"/api/photos/{photoId}")).StatusCode);
    Assert.Equal(HttpStatusCode.NoContent, (await this.Anonymous.AsUser("owner").DeleteAsync($"/api/photos/{photoId}")).StatusCode);
  }

  [Fact]
  public async Task CustomController_InvalidDto_Returns400()
  {
    await AssertValidationErrorsAsync(await this.Anonymous.PostAsync("/api/photos", new { product = new { id = 1 } }), "Filename");
  }

  [Fact]
  public async Task CustomController_RuleSetsOfOverriddenAction_AreInherited()
  {
    int photoId = await this.CreatePhotoAsync("donut.jpg");
    JsonNode photo = await this.Anonymous.GetJsonAsync($"/api/photos/{photoId}?include=product");

    // The overridden POST validates the creation rules (a new photo must have an extension), the inherited PUT doesn't.
    await AssertValidationErrorsAsync(await this.Anonymous.PostAsync("/api/photos", new { product = new { id = photo["product"]!["id"]!.GetValue<int>() }, filename = "donut" }), "Filename");
    Assert.Equal(HttpStatusCode.NoContent, (await this.Anonymous.PutAsync("/api/photos", new { id = photoId, product = new { id = photo["product"]!["id"]!.GetValue<int>() }, filename = "donut" })).StatusCode);
  }

  private async Task<int> CreatePhotoAsync(string filename)
  {
    int categoryId = await this.CreateCategoryAsync();
    int productId = (await this.Anonymous.PostJsonAsync("/api/products", new { category = new { id = categoryId }, name = "Pastry", price = 1m }))["id"]!.GetValue<int>();

    return (await this.Anonymous.PostJsonAsync("/api/photos", new { product = new { id = productId }, filename }))["id"]!.GetValue<int>();
  }

  private async Task<int> CreateCategoryAsync()
  {
    return (await this.Anonymous.PostJsonAsync("/api/categories", new { name = "Bakery" }))["id"]!.GetValue<int>();
  }
}
