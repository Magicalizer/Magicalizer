// Copyright © 2025 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;

namespace Magicalizer.Extensions;

/// <summary>
/// Provides extension methods for configuring the web application with the required middleware for processing REST API requests.
/// </summary>
public static class WebApplicationExtensions
{
  /// <summary>
  /// Configures the web application to serve static files and map controller endpoints, enabling support for REST API requests.
  /// </summary>
  /// <param name="webApplication">The instance of the <see cref="WebApplication"/> to configure.</param>
  public static void UseMagicalizer(this WebApplication webApplication)
  {
    webApplication.MapControllers();
  }
}