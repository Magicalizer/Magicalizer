// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Magicalizer.TestHost;

namespace Magicalizer.IntegrationTests.Infrastructure;

/// <summary>
/// HTTP client of the API with an optional test user (see <see cref="TestAuthenticationHandler"/>) and JSON helpers.
/// </summary>
public class ApiClient
{
  /// <summary>
  /// JSON options of the API (camel case).
  /// </summary>
  public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

  private readonly HttpClient httpClient;
  private readonly string? userName;
  private readonly string[] permissions;
  private readonly (string Name, string Value)[] headers;

  /// <summary>
  /// Creates an anonymous client.
  /// </summary>
  public ApiClient(HttpClient httpClient) : this(httpClient, null, [], []) { }

  private ApiClient(HttpClient httpClient, string? userName, string[] permissions, (string Name, string Value)[] headers)
  {
    this.httpClient = httpClient;
    this.userName = userName;
    this.permissions = permissions;
    this.headers = headers;
  }

  /// <summary>
  /// Returns a client with the same connection, authenticated as a user with the permissions.
  /// </summary>
  public ApiClient AsUser(string userName, params string[] permissions)
  {
    return new ApiClient(this.httpClient, userName, permissions, this.headers);
  }

  /// <summary>
  /// Returns a client with the same connection and user that sends the header with each request.
  /// </summary>
  public ApiClient WithHeader(string name, string value)
  {
    return new ApiClient(this.httpClient, this.userName, this.permissions, [.. this.headers, (name, value)]);
  }

  /// <summary>
  /// Sends a request with an optional JSON body.
  /// </summary>
  public Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null)
  {
    HttpRequestMessage request = new HttpRequestMessage(method, url);

    if (body != null)
      request.Content = body as HttpContent ?? new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");

    if (this.userName != null)
    {
      request.Headers.Add(TestAuthenticationHandler.UserHeaderName, this.userName);
      request.Headers.Add(TestAuthenticationHandler.PermissionsHeaderName, string.Join(',', this.permissions));
    }

    foreach ((string name, string value) in this.headers)
      request.Headers.Add(name, value);

    return this.httpClient.SendAsync(request, TestContext.Current.CancellationToken);
  }

  /// <summary>
  /// Sends a GET request.
  /// </summary>
  public Task<HttpResponseMessage> GetAsync(string url) => this.SendAsync(HttpMethod.Get, url);

  /// <summary>
  /// Sends a POST request with a JSON body.
  /// </summary>
  public Task<HttpResponseMessage> PostAsync(string url, object body) => this.SendAsync(HttpMethod.Post, url, body);

  /// <summary>
  /// Sends a PUT request with a JSON body.
  /// </summary>
  public Task<HttpResponseMessage> PutAsync(string url, object body) => this.SendAsync(HttpMethod.Put, url, body);

  /// <summary>
  /// Sends a PATCH request with a JSON Patch body.
  /// </summary>
  public Task<HttpResponseMessage> PatchAsync(string url, object patch)
  {
    return this.SendAsync(HttpMethod.Patch, url, new StringContent(JsonSerializer.Serialize(patch, JsonOptions), Encoding.UTF8, "application/json-patch+json"));
  }

  /// <summary>
  /// Sends a DELETE request.
  /// </summary>
  public Task<HttpResponseMessage> DeleteAsync(string url) => this.SendAsync(HttpMethod.Delete, url);

  /// <summary>
  /// Sends a GET request and returns the JSON of the response, which must be successful.
  /// </summary>
  public async Task<JsonNode> GetJsonAsync(string url)
  {
    return await ReadJsonAsync(await this.GetAsync(url), HttpStatusCode.OK);
  }

  /// <summary>
  /// Sends a POST request and returns the JSON of the response, which must be 201.
  /// </summary>
  public async Task<JsonNode> PostJsonAsync(string url, object body)
  {
    return await ReadJsonAsync(await this.PostAsync(url, body), HttpStatusCode.Created);
  }

  /// <summary>
  /// Reads the JSON of the response, checking its status first (the body is in the failure message).
  /// </summary>
  public static async Task<JsonNode> ReadJsonAsync(HttpResponseMessage response, HttpStatusCode? expectedStatus = null)
  {
    string content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

    if (expectedStatus != null)
      Assert.True(response.StatusCode == expectedStatus, $"Expected {(int)expectedStatus} but got {(int)response.StatusCode}: {content}");

    return JsonNode.Parse(content)!;
  }
}
