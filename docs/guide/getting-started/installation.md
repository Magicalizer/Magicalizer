# Installation

Magicalizer is a set of NuGet packages. A web application references the main one, `Magicalizer`, and calls two
methods in its `Program` class.

## Requirements

- .NET 10 and an ASP.NET Core web application (an empty one is enough).
- Entity Framework Core 10 with the provider of your database: SQLite, PostgreSQL, SQL Server, or any other one.

FluentValidation, which validates the DTOs and the models, comes with Magicalizer, so you don’t add it yourself. See
[Requirements](../introduction/requirements.md) for the details.

## Add the packages

Add Magicalizer and the Entity Framework Core provider of your database to the web application (SQLite here):

```console
dotnet add package Magicalizer
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

`Magicalizer` brings the abstractions your classes implement (`IEntity`, `IModel`, `IDto`, `IFilter`, and the rest), so a
single-project application needs nothing else. When the classes live in separate projects, each of them references
only the abstractions of its layer; see [A layered solution](layered-solution.md).

## Register Magicalizer

Register your database context as `DbContext`, then call `AddMagicalizer` and `UseMagicalizer`:

```csharp
using App.Data.Entities;
using Magicalizer.Extensions;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DbContext, AppDbContext>(options => {
  options.UseSqlite(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddMagicalizer();

WebApplication webApplication = builder.Build();

webApplication.UseMagicalizer();
webApplication.Run();
```

- `AddDbContext<DbContext, AppDbContext>` registers your context by the base `DbContext` type: that’s the type the
  services of Magicalizer depend on.
- `AddMagicalizer` finds your classes, registers a service for each model and a controller for each DTO marked with
  `[Magicalized]`, and the validators. It also takes the [options](../customization/options.md), e.g. the limits.
- `UseMagicalizer` maps the controllers.

When the application starts, Magicalizer logs which DTOs it serves and at which routes, and warns about the classes
that are misconfigured (see [Startup diagnostics](../troubleshooting/startup-diagnostics.md)).

## Next steps

Create the classes of your first API: [Your first API](your-first-api.md) walks through it step by step.
