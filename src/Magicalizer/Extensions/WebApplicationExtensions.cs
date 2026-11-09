// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Microsoft.AspNetCore.Builder;

namespace Magicalizer.Extensions;

/// <summary>
/// Extensions of <see cref="WebApplication"/> to process REST API requests.
/// </summary>
public static class WebApplicationExtensions
{
  /// <summary>
  /// Maps controller endpoints (including the magicalized ones).
  /// </summary>
  /// <param name="webApplication">Web application to configure.</param>
  /// <returns>Web application, so the calls can be chained.</returns>
  public static WebApplication UseMagicalizer(this WebApplication webApplication)
  {
    webApplication.MapControllers();
    return webApplication;
  }
}
