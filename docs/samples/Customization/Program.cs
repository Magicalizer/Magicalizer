using App;
using App.Data.Entities;
using App.Data.Prefilters;
using App.Filters;
using App.Infrastructure;
using Magicalizer.Domain.Services.Abstractions;
using Magicalizer.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DbContext, AppDbContext>(options => {
  options.UseSqlite(builder.Configuration.GetConnectionString("Default"));
});

#region Options
builder.Services.AddMagicalizer(options => {
  // The maximum number of objects a request gets, the default limit too.
  options.MaxLimit = 100;

  // The maximum number of properties in a property path of an inclusion.
  options.MaxInclusionDepth = 2;

  // The query parameters something else processes (e.g. the request localization).
  options.IgnoredQueryParameters.Add("culture");
});
#endregion

#region Prefilters
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IQueryPrefilter<App.Data.Entities.Product, ProductFilter>, ArchivedProductPrefilter>();
#endregion

#region Authorization
builder.Services.AddAuthentication(HeaderAuthenticationHandler.SchemeName)
  .AddScheme<AuthenticationSchemeOptions, HeaderAuthenticationHandler>(HeaderAuthenticationHandler.SchemeName, null);

builder.Services.AddAuthorization(options => {
  options.AddPolicy(Policies.EditCatalog, policy => policy.RequireRole("editor"));
});
#endregion

#region OpenApi
// The public document: the APIs without a group (and the ones of the "v1" group).
builder.Services.AddOpenApi();

// The internal document: only the APIs of the "internal" group.
builder.Services.AddOpenApi("internal", options => {
  options.ShouldInclude = description => description.GroupName == "internal";
});
#endregion

WebApplication webApplication = builder.Build();

// The database is created on the first run (use the migrations of EF Core in a real application).
using (IServiceScope scope = webApplication.Services.CreateScope())
  scope.ServiceProvider.GetRequiredService<DbContext>().Database.EnsureCreated();

webApplication.UseAuthentication();
webApplication.UseAuthorization();
webApplication.UseMagicalizer();
webApplication.MapOpenApi();
webApplication.Run();
