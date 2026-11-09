// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;

namespace Magicalizer.IntegrationTests.Infrastructure;

/// <summary>
/// Base of the integration tests: the test class gets its own host and database (see <see cref="MagicalizerFactory"/>),
/// the tests of a class run one by one and share them, so the ones that search their data by name make the names
/// unique (see <see cref="IntegrationTest{TFactory}.Unique"/>).
/// </summary>
public abstract class IntegrationTest : IntegrationTest<MagicalizerFactory>
{
  /// <summary>
  /// Creates the test.
  /// </summary>
  protected IntegrationTest(MagicalizerFactory factory) : base(factory) { }
}

/// <summary>
/// Base of the integration tests whose host is a <see cref="MagicalizerFactory"/> descendant (e.g. one that prepares
/// the data shared by the tests of the class).
/// </summary>
public abstract class IntegrationTest<TFactory> : IClassFixture<TFactory> where TFactory : MagicalizerFactory
{
  /// <summary>
  /// Primary key value that no object has.
  /// </summary>
  protected const int NonexistentId = 100_000;

  private static int counter;

  /// <summary>
  /// Host of the test class.
  /// </summary>
  protected TFactory Factory { get; }

  /// <summary>
  /// Client without credentials.
  /// </summary>
  protected ApiClient Anonymous { get; }

  /// <summary>
  /// Creates the test.
  /// </summary>
  protected IntegrationTest(TFactory factory)
  {
    this.Factory = factory;
    this.Anonymous = factory.CreateApiClient();
  }

  /// <summary>
  /// Returns a unique name with the prefix (e.g. "Soup12"), so the data of a test doesn't match the queries of the
  /// other ones.
  /// </summary>
  protected static string Unique(string prefix)
  {
    return $"{prefix}{Interlocked.Increment(ref counter)}";
  }

  /// <summary>
  /// Returns the IDs of the objects of a JSON array.
  /// </summary>
  protected static List<int> GetIds(JsonNode json)
  {
    return json.AsArray().Select(o => o!["id"]!.GetValue<int>()).ToList();
  }

  /// <summary>
  /// Checks that the response is 400 with the validation errors of exactly these keys (the keys are compared
  /// case-insensitively, as the ones of the properties depend on the naming policy).
  /// </summary>
  protected static async Task AssertValidationErrorsAsync(HttpResponseMessage response, params string[] keys)
  {
    JsonNode json = await ApiClient.ReadJsonAsync(response, HttpStatusCode.BadRequest);
    IEnumerable<string> actualKeys = json["errors"]?.AsObject().Select(e => e.Key) ?? [];

    Assert.Equal(keys.Select(k => k.ToLowerInvariant()).Order(StringComparer.Ordinal), actualKeys.Select(k => k.ToLowerInvariant()).Order(StringComparer.Ordinal));
  }

  /// <summary>
  /// Checks that the response is 400 with a validation error of this key, among the other ones.
  /// </summary>
  protected static async Task AssertValidationErrorAsync(HttpResponseMessage response, string key)
  {
    JsonNode json = await ApiClient.ReadJsonAsync(response, HttpStatusCode.BadRequest);

    Assert.Contains(key, json["errors"]?.AsObject().Select(e => e.Key) ?? [], StringComparer.OrdinalIgnoreCase);
  }
}
