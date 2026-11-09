using System.Reflection;
using App.Data.Entities;
using Magicalizer.Extensions;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DbContext, AppDbContext>(options => {
  options.UseSqlite(builder.Configuration.GetConnectionString("Default"));
});

#region LoadAssemblies
// Magicalizer looks for the classes in the loaded assemblies, and .NET loads an assembly only when its types are used.
// Program.cs doesn't use the DTOs, models or filters, so their assemblies are loaded explicitly.
foreach (string assemblyName in (string[])["App.Api.Dto", "App.Domain.Models", "App.Filters", "App.Data.Entities"])
  Assembly.Load(assemblyName);

builder.Services.AddMagicalizer();
#endregion

WebApplication webApplication = builder.Build();

// The database is created on the first run (use the migrations of EF Core in a real application).
using (IServiceScope scope = webApplication.Services.CreateScope())
  scope.ServiceProvider.GetRequiredService<DbContext>().Database.EnsureCreated();

webApplication.UseMagicalizer();
webApplication.Run();
