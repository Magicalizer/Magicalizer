# Requirements

## The platform

| | Version | Notes |
|---|---|---|
| .NET | 10 | `Magicalizer` targets `net10.0` and uses the shared framework of ASP.NET Core 10 |
| ASP.NET Core | 10 | A web application; an empty one (`dotnet new web`) is enough |
| Entity Framework Core | 10 | With the provider of your database |

The abstraction packages (`Magicalizer.*.Abstractions`) target .NET Standard 2.0, so the projects of the entities,
models, DTOs, and filters can target any framework that supports it (see [Packages and layers](packages-and-layers.md)).

## The database

Magicalizer works through EF Core and builds the usual LINQ queries, so it doesn’t depend on a particular provider. The
tests of Magicalizer run on:

- SQLite (`Microsoft.EntityFrameworkCore.Sqlite`);
- PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`);
- SQL Server (`Microsoft.EntityFrameworkCore.SqlServer`).

The results of the string conditions depend on the database: its collation decides, e.g., whether `name.contains=pizza`
finds “Pizza” (see [Strings and collations](../http-api/filtering.md#strings-and-collations)).

The entities need a primary key of one, two, or three properties (see [Entities](../building-blocks/entities.md)).

## The dependencies

`Magicalizer` brings these packages, so you don’t add them yourself:

| Package | What for |
|---|---|
| `FluentValidation`, `FluentValidation.DependencyInjectionExtensions` 12 | The validation of the DTOs and the models |
| `Microsoft.EntityFrameworkCore` 10 | The queries (the provider is yours) |
| `Microsoft.AspNetCore.JsonPatch.SystemTextJson` 10 | `PATCH` by JSON Patch with System.Text.Json |

A project of validators that doesn’t reference `Magicalizer` (e.g. in a [layered solution](packages-and-layers.md#separate-projects))
adds `FluentValidation` itself, of the same major version.

The OpenAPI document isn’t built in: add `Microsoft.AspNetCore.OpenApi` (or another tool) if you need it (see
[OpenAPI](../customization/openapi.md)).

## See also

- [Installation](../getting-started/installation.md)
