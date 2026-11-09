// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Magicalizer.IntegrationTests.Infrastructure;

namespace Magicalizer.IntegrationTests.OpenApi;

/// <summary>
/// The description of the magicalized controllers in the OpenAPI document: the query parameters as they are passed,
/// the supported methods and the hidden DTOs.
/// </summary>
public class OpenApiTests : IntegrationTest
{
  /// <summary>
  /// Creates the tests.
  /// </summary>
  public OpenApiTests(MagicalizerFactory factory) : base(factory) { }

  [Fact]
  public async Task Document_MagicalizedAndCustomControllers_AreDescribed()
  {
    JsonObject paths = await this.GetPathsAsync();

    Assert.Contains("/api/categories", paths.Select(p => p.Key));
    Assert.Contains("/api/products/{id}", paths.Select(p => p.Key));
    Assert.Contains("/api/product-tags/{id1}/{id2}", paths.Select(p => p.Key));
    Assert.Contains("/api/photos/count", paths.Select(p => p.Key));
  }

  [Fact]
  public async Task Document_QueryParameters_AreInLowerCaseAsPassed()
  {
    List<string> parameters = await this.GetQueryParametersAsync("/api/products");

    Assert.Contains("name.contains", parameters);
    Assert.Contains("isavailable.equals", parameters);
    Assert.Contains("category.name.equals", parameters);
    Assert.Contains("sort", parameters);
    Assert.Contains("include", parameters);

    // The ignored property isn't a condition of the filter, but it's still passed (to the custom service).
    Assert.Contains("search", parameters);
  }

  [Fact]
  public async Task Document_ValueFilterConditions_AreInOrderOfDeclaration()
  {
    List<string> parameters = await this.GetQueryParametersAsync("/api/products");

    // The conditions of the base filters go first, as they are declared from the base type, and the properties keep
    // their order.
    Assert.Equal(["price.isnull", "price.equals", "price.notequals", "price.in", "price.notin", "price.from", "price.to"], parameters.Where(p => p.StartsWith("price.", StringComparison.Ordinal)));
    Assert.Equal(["diet.isnull", "diet.equals", "diet.notequals", "diet.in", "diet.notin"], parameters.Where(p => p.StartsWith("diet.", StringComparison.Ordinal)));
    Assert.Contains("name.startswith", parameters);
    Assert.Contains("updated.from", parameters);
    Assert.True(parameters.IndexOf("name.notin") < parameters.IndexOf("price.isnull"));
  }

  [Fact]
  public async Task Document_SortingsAndInclusions_AreDescribedAsStrings()
  {
    JsonArray parameters = (await this.GetPathsAsync())["/api/products"]!["get"]!["parameters"]!.AsArray();

    // They are bound from comma-separated property paths, and the paging values can't be negative.
    Assert.Equal("string", GetParameter(parameters, "sort")["schema"]!["type"]!.GetValue<string>());
    Assert.Equal("string", GetParameter(parameters, "include")["schema"]!["type"]!.GetValue<string>());
    Assert.Equal(0, GetParameter(parameters, "offset")["schema"]!["minimum"]!.GetValue<int>());
    Assert.Equal(0, GetParameter(parameters, "limit")["schema"]!["minimum"]!.GetValue<int>());
  }

  [Fact]
  public async Task Document_PreferHeader_IsDescribedForListsOnly()
  {
    JsonObject paths = await this.GetPathsAsync();
    JsonNode preferHeader = GetParameter(paths["/api/products"]!["get"]!["parameters"]!.AsArray(), "Prefer");

    // The action reads the header itself, as the custom controllers override the action (e.g. the photos one).
    Assert.Equal("header", preferHeader["in"]!.GetValue<string>());
    Assert.NotEqual(true, preferHeader["required"]?.GetValue<bool>());
    Assert.Contains("Prefer", paths["/api/photos"]!["get"]!["parameters"]!.AsArray().Select(p => p!["name"]!.GetValue<string>()));
    Assert.DoesNotContain("Prefer", paths["/api/products/{id}"]!["get"]!["parameters"]!.AsArray().Select(p => p!["name"]!.GetValue<string>()));
  }

  [Fact]
  public async Task Document_CollectionConditions_AreDescribedAsElementFilterParameters()
  {
    List<string> categoryParameters = await this.GetQueryParametersAsync("/api/categories");
    List<string> productParameters = await this.GetQueryParametersAsync("/api/products");

    Assert.Contains("products.any.name.contains", categoryParameters);
    Assert.Contains("products.none.price.from", categoryParameters);
    Assert.Contains("products.isempty", categoryParameters);

    // The element filter is expanded with the entities it refers to, but not with the ones already in the path, and its
    // own collections aren't expanded.
    Assert.Contains("tags.any.tag.name.equals", productParameters);
    Assert.DoesNotContain(productParameters, p => p.StartsWith("tags.any.product.", StringComparison.Ordinal));
    Assert.DoesNotContain(categoryParameters, p => p.StartsWith("products.any.category.", StringComparison.Ordinal));
    Assert.DoesNotContain(categoryParameters, p => p.StartsWith("products.any.photos.", StringComparison.Ordinal));
  }

  [Fact]
  public async Task Document_UnsupportedMethods_AreNotDescribed()
  {
    JsonObject paths = await this.GetPathsAsync();

    Assert.Equal(["get", "post"], paths["/api/tags"]!.AsObject().Select(o => o.Key).Where(IsOperation).Order(StringComparer.Ordinal));
    Assert.Equal(["get"], paths["/api/tags/{id}"]!.AsObject().Select(o => o.Key).Where(IsOperation));
  }

  [Fact]
  public async Task Document_IgnoredApi_IsNotDescribed()
  {
    Assert.DoesNotContain((await this.GetPathsAsync()).Select(p => p.Key), p => p.StartsWith("/api/stocks", StringComparison.Ordinal));
  }

  [Fact]
  public async Task Document_GroupName_PutsApiIntoItsDocument()
  {
    JsonObject internalPaths = (await this.Anonymous.GetJsonAsync("/openapi/internal.json"))["paths"]!.AsObject();

    // The API without a group is in all the documents, as in ASP.NET Core.
    Assert.DoesNotContain("/api/notes", (await this.GetPathsAsync()).Select(p => p.Key));
    Assert.Contains("/api/notes", internalPaths.Select(p => p.Key));
    Assert.Contains("/api/products", internalPaths.Select(p => p.Key));
  }

  private async Task<JsonObject> GetPathsAsync()
  {
    return (await this.Anonymous.GetJsonAsync("/openapi/v1.json"))["paths"]!.AsObject();
  }

  private async Task<List<string>> GetQueryParametersAsync(string path)
  {
    JsonArray parameters = (await this.GetPathsAsync())[path]!["get"]!["parameters"]!.AsArray();

    return parameters.Where(p => p!["in"]!.GetValue<string>() == "query").Select(p => p!["name"]!.GetValue<string>()).ToList();
  }

  private static JsonNode GetParameter(JsonArray parameters, string name)
  {
    return parameters.Single(p => p!["name"]!.GetValue<string>() == name)!;
  }

  private static bool IsOperation(string key)
  {
    return key is "get" or "post" or "put" or "patch" or "delete";
  }
}
