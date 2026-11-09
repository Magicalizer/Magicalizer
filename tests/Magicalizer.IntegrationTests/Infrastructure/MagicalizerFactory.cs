// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Magicalizer.IntegrationTests.Infrastructure;

/// <summary>
/// Starts the test host on its own database, so each test class works with its own data. It's SQLite (a file in
/// a temporary folder) by default, or PostgreSQL or SQL Server if the connection string to the server is set by
/// <see cref="PostgreSqlVariableName"/> or <see cref="SqlServerVariableName"/> (a database is created for each test
/// class and deleted after it).
/// </summary>
public class MagicalizerFactory : WebApplicationFactory<Program>
{
  /// <summary>
  /// Name of the environment variable with the connection string to a PostgreSQL server.
  /// </summary>
  public const string PostgreSqlVariableName = "MAGICALIZER_TESTS_POSTGRESQL";

  /// <summary>
  /// Name of the environment variable with the connection string to a SQL Server.
  /// </summary>
  public const string SqlServerVariableName = "MAGICALIZER_TESTS_SQLSERVER";

  private readonly string? rootPath;
  private readonly string provider;
  private readonly string connectionString;

  /// <summary>
  /// Messages logged by the test host.
  /// </summary>
  public LogRecorder Logs { get; } = new LogRecorder();

  /// <summary>
  /// Creates the factory.
  /// </summary>
  public MagicalizerFactory()
  {
    string databaseName = $"magicalizer_tests_{Guid.NewGuid():N}";
    string? postgreSqlConnectionString = Environment.GetEnvironmentVariable(PostgreSqlVariableName);
    string? sqlServerConnectionString = Environment.GetEnvironmentVariable(SqlServerVariableName);

    if (!string.IsNullOrEmpty(postgreSqlConnectionString))
    {
      this.provider = "PostgreSql";
      this.connectionString = new NpgsqlConnectionStringBuilder(postgreSqlConnectionString) { Database = databaseName }.ToString();
    }

    else if (!string.IsNullOrEmpty(sqlServerConnectionString))
    {
      this.provider = "SqlServer";
      this.connectionString = new SqlConnectionStringBuilder(sqlServerConnectionString) { InitialCatalog = databaseName }.ToString();
    }

    else
    {
      this.provider = "Sqlite";
      this.rootPath = Path.Combine(Path.GetTempPath(), "magicalizer-tests", Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(this.rootPath);
      this.connectionString = new SqliteConnectionStringBuilder { DataSource = Path.Combine(this.rootPath, "magicalizer.db") }.ToString();
    }
  }

  /// <summary>
  /// Creates an anonymous API client.
  /// </summary>
  public ApiClient CreateApiClient()
  {
    return new ApiClient(this.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }));
  }

  /// <summary>
  /// Runs the action with a database context of a new scope (e.g. to prepare the data directly).
  /// </summary>
  public async Task WithDbAsync(Func<DbContext, Task> action)
  {
    using IServiceScope scope = this.Services.CreateScope();

    await action(scope.ServiceProvider.GetRequiredService<DbContext>());
  }

  /// <inheritdoc/>
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseSetting("Database:Provider", this.provider);
    builder.UseSetting("ConnectionStrings:Default", this.connectionString);
    builder.UseSetting("Logging:LogLevel:Default", "Warning");
    builder.UseSetting("Logging:LogLevel:Magicalizer", "Debug");
    builder.UseEnvironment("Testing");

    // The logs are only recorded for the tests: written to the console, they would be large (e.g. the debug ones of
    // Magicalizer for each host) and could block a test runner that doesn't read the output (e.g. Visual Studio).
    builder.ConfigureLogging(logging => {
      logging.ClearProviders();
      logging.AddProvider(this.Logs);
    });
  }

  /// <inheritdoc/>
  public override async ValueTask DisposeAsync()
  {
    // The database on a server is deleted, the SQLite one is deleted with its folder.
    if (this.rootPath == null)
      await this.WithDbAsync(db => db.Database.EnsureDeletedAsync());

    await base.DisposeAsync();
    SqliteConnection.ClearAllPools();

    if (this.rootPath != null)
    {
      try
      {
        Directory.Delete(this.rootPath, recursive: true);
      }

      // The folder is temporary, so it's fine if it can't be deleted right now.
      catch (IOException)
      {
      }
    }

    GC.SuppressFinalize(this);
  }
}
