// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Magicalizer.IntegrationTests.Catalog;

/// <summary>
/// Host of the catalog without the total count by default (see <see cref="MagicalizerOptions.CountTotalByDefault"/>).
/// </summary>
public class TotalCountTestsFactory : CatalogTestsFactory
{
  /// <inheritdoc/>
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    base.ConfigureWebHost(builder);
    builder.ConfigureTestServices(services => services.Configure<MagicalizerOptions>(o => o.CountTotalByDefault = false));
  }
}
