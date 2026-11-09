// SPDX-FileCopyrightText: Dmitry Sikorsky <dmitry@sikorsky.pro>
// SPDX-License-Identifier: Apache-2.0

using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Extensions;
using Magicalizer.TestHost;
using Magicalizer.TestHost.Notes;
using Magicalizer.TestHost.Products;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

// The host of the integration tests: an app with the test entities on SQLite, PostgreSQL or SQL Server. The tests set
// the provider and the connection string (see MagicalizerFactory).
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string provider = builder.Configuration["Database:Provider"] ?? "Sqlite";
string connectionString = builder.Configuration.GetConnectionString("Default")
  ?? throw new InvalidOperationException("The connection string 'Default' isn't set.");

// The tests pass the times without a time zone, as the other databases keep them.
if (provider == "PostgreSql")
  AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDbContext<DbContext, TestDbContext>(options => {
  if (provider == "PostgreSql")
    options.UseNpgsql(connectionString);

  else if (provider == "SqlServer")
    options.UseSqlServer(connectionString);

  else options.UseSqlite(connectionString).AddInterceptors(new SqliteCompatibilityInterceptor());
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IQueryPrefilter<ProductEntity, ProductFilter>, ArchivedProductPrefilter>();
builder.Services.AddAuthentication(TestAuthenticationHandler.SchemeName)
  .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.SchemeName, null);

builder.Services.AddAuthorization(options => {
  options.AddPolicy(NoteDto.EditPolicyName, p => p.RequireClaim(TestAuthenticationHandler.PermissionClaimType, "edit"));
  options.AddPolicy(NoteDto.DeletePolicyName, p => p.RequireClaim(TestAuthenticationHandler.PermissionClaimType, "delete"));
});

// The culture is processed by the request localization, if an app uses it.
builder.Services.AddMagicalizer(options => options.IgnoredQueryParameters.Add("culture"));
builder.Services.AddOpenApi();
builder.Services.AddOpenApi(NoteDto.GroupName);

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
  scope.ServiceProvider.GetRequiredService<DbContext>().Database.EnsureCreated();

app.UseAuthentication();
app.UseAuthorization();
app.UseMagicalizer();
app.MapOpenApi();
app.Run();

/// <summary>
/// Entry point of the test host (public, so the tests can start it by <c>WebApplicationFactory&lt;Program&gt;</c>).
/// </summary>
public partial class Program
{
}
