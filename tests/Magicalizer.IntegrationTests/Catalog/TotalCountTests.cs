// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Magicalizer.IntegrationTests.Infrastructure;

namespace Magicalizer.IntegrationTests.Catalog;

/// <summary>
/// The total count of a list when it's off by default (see <see cref="TotalCountTestsFactory"/>): a request asks for
/// it with the Prefer header.
/// </summary>
public class TotalCountTests : IntegrationTest<TotalCountTestsFactory>
{
  /// <summary>
  /// Creates the tests.
  /// </summary>
  public TotalCountTests(TotalCountTestsFactory factory) : base(factory) { }

  [Fact]
  public async Task TotalCount_NotPreferred_IsSkipped()
  {
    HttpResponseMessage response = await this.Anonymous.GetAsync("/api/products?limit=2");

    Assert.Equal(2, GetIds(await ApiClient.ReadJsonAsync(response, HttpStatusCode.OK)).Count);
    Assert.False(response.Headers.Contains("Paging-Total-Count"));
    Assert.False(response.Headers.Contains("Preference-Applied"));
    Assert.Equal("2", response.Headers.GetValues("Paging-Limit").Single());
  }

  [Fact]
  public async Task TotalCount_PreferredExact_IsReturned()
  {
    HttpResponseMessage response = await this.Anonymous.WithHeader("Prefer", "count=exact").GetAsync("/api/products?limit=2");

    Assert.Equal(2, GetIds(await ApiClient.ReadJsonAsync(response, HttpStatusCode.OK)).Count);
    Assert.Equal("5", response.Headers.GetValues("Paging-Total-Count").Single());
    Assert.Equal("count=exact", response.Headers.GetValues("Preference-Applied").Single());
  }
}
