// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using System.Data.Common;
using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Magicalizer.TestHost;

/// <summary>
/// Makes SQLite behave like the other databases where Magicalizer relies on it. Any app on SQLite needs it, Magicalizer
/// itself doesn't depend on a database provider.
/// <para>
/// It replaces the EF_DECIMAL collation of the EF Core SQLite provider. SQLite has no decimal type, so EF Core stores
/// the decimals as text (in the invariant culture) and sorts them with this collation, but its own one parses the text
/// with the current culture (https://github.com/dotnet/efcore/issues/37432). With a culture where the decimal separator
/// is a comma (e.g. uk-UA) it throws inside SQLite on any sorting by a decimal column, and the exception crashes the
/// process; with a culture where the point separates the thousands (e.g. de-DE) the order is wrong.
/// </para>
/// </summary>
public class SqliteCompatibilityInterceptor : DbConnectionInterceptor
{
  /// <inheritdoc/>
  public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
  {
    Configure(connection);
  }

  /// <inheritdoc/>
  public override Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
  {
    Configure(connection);
    return Task.CompletedTask;
  }

  // EF Core registers its collation when it creates the connection, so it's replaced after the connection is opened.
  private static void Configure(DbConnection connection)
  {
    if (connection is not SqliteConnection sqliteConnection)
      return;

    sqliteConnection.CreateCollation("EF_DECIMAL", (x, y) => decimal.Compare(ParseDecimal(x), ParseDecimal(y)));
  }

  // A value that isn't a number (it can't be written by EF Core, but can be by hand) is sorted as zero, as an
  // exception here crashes the process (see the summary).
  private static decimal ParseDecimal(string value)
  {
    return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal result) ? result : 0m;
  }
}
