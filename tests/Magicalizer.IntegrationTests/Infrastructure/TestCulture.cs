// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Runtime.CompilerServices;

namespace Magicalizer.IntegrationTests.Infrastructure;

/// <summary>
/// Culture of the tests: the one with the comma as the decimal separator (uk-UA), so the tests catch the code that
/// depends on the culture on any machine (e.g. the sorting by decimals on SQLite, see
/// <c>SqliteCompatibilityInterceptor</c> of the test host).
/// </summary>
internal static class TestCulture
{
  // Runs when the test assembly is loaded, before any host starts, and applies to all the threads that don't set their
  // own culture (e.g. the ones that handle the requests).
  [ModuleInitializer]
  internal static void Initialize()
  {
    CultureInfo culture = CultureInfo.GetCultureInfo("uk-UA");

    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;
  }
}
