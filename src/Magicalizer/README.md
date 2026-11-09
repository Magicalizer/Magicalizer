# Magicalizer

[Magicalizer](https://github.com/Magicalizer/Magicalizer) creates a fully featured REST API ASP.NET Core web application
from the DTO, model and entity classes, the filters and the validators.

Each DTO marked with `[Magicalized]` gets the standard REST API (GET, POST, PUT, PATCH and DELETE) with filtering,
sorting, paging and inclusion of related objects, validation, authorization and the OpenAPI description. Any default
service or controller can be replaced with a custom one.

```csharp
builder.Services.AddDbContext<DbContext, AppDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddMagicalizer();

WebApplication webApplication = builder.Build();

webApplication.UseMagicalizer();
webApplication.Run();
```

See the [documentation](https://github.com/Magicalizer/Magicalizer#readme).
