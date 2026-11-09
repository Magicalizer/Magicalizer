using App.Data.Entities;
using App.Data.Prefilters;
using App.Domain;
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

builder.Services.AddMagicalizer(options => {
  options.MaxLimit = 100;

  // The parameters of the request localization (see UseRequestLocalization below).
  options.IgnoredQueryParameters.Add("culture");
  options.IgnoredQueryParameters.Add("ui-culture");
});

#region OwnData
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddScoped<IQueryPrefilter<Notebook, NotebookFilter>, OwnNotebookPrefilter>();
builder.Services.AddScoped<IQueryPrefilter<Note, NoteFilter>, OwnNotePrefilter>();
#endregion

builder.Services.AddAuthentication(HeaderAuthenticationHandler.SchemeName)
  .AddScheme<AuthenticationSchemeOptions, HeaderAuthenticationHandler>(HeaderAuthenticationHandler.SchemeName, null);

builder.Services.AddAuthorization();

#region Cors
builder.Services.AddCors(options => {
  options.AddDefaultPolicy(policy => {
    policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
      .AllowAnyMethod()
      .AllowAnyHeader()
      .WithExposedHeaders("Location", "Paging-Total-Count", "Paging-Offset", "Paging-Limit", "Preference-Applied");
  });
});
#endregion

WebApplication webApplication = builder.Build();

// The database is created on the first run (use the migrations of EF Core in a real application).
using (IServiceScope scope = webApplication.Services.CreateScope())
  scope.ServiceProvider.GetRequiredService<DbContext>().Database.EnsureCreated();

#region Localization
webApplication.UseRequestLocalization(options => {
  string[] cultures = ["en", "uk"];

  options.SetDefaultCulture(cultures[0])
    .AddSupportedCultures(cultures)
    .AddSupportedUICultures(cultures);
});
#endregion

webApplication.UseCors();
webApplication.UseAuthentication();
webApplication.UseAuthorization();
webApplication.UseMagicalizer();
webApplication.Run();
