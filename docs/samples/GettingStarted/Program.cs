using App.Data.Entities;
using Magicalizer.Extensions;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DbContext, AppDbContext>(options => {
  options.UseSqlite(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddMagicalizer();
builder.Services.AddOpenApi();

WebApplication webApplication = builder.Build();

// The database is created on the first run (use the migrations of EF Core in a real application).
using (IServiceScope scope = webApplication.Services.CreateScope())
  scope.ServiceProvider.GetRequiredService<DbContext>().Database.EnsureCreated();

webApplication.UseMagicalizer();
webApplication.MapOpenApi();
webApplication.Run();
