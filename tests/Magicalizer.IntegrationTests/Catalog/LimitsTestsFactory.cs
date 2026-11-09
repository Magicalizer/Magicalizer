// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer.IntegrationTests.Catalog;

/// <summary>
/// Host of the limits tests: the catalog with the maximum number of models a request gets
/// (<see cref="MaxLimit"/>) and the maximum depth of the inclusions (<see cref="MaxInclusionDepth"/>).
/// </summary>
public class LimitsTestsFactory : CatalogTestsFactory
{
  /// <summary>
  /// Maximum number of models a request gets.
  /// </summary>
  public const int MaxLimit = 3;

  /// <summary>
  /// Maximum number of properties in a property path of an inclusion.
  /// </summary>
  public const int MaxInclusionDepth = 2;

  /// <inheritdoc/>
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    base.ConfigureWebHost(builder);
    builder.ConfigureTestServices(services => services.Configure<MagicalizerOptions>(o => {
      o.MaxLimit = MaxLimit;
      o.MaxInclusionDepth = MaxInclusionDepth;
    }));
  }
}
