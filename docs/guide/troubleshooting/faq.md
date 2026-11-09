# FAQ

The usual problems and questions, with the causes and the fixes. Start with the log of the start: the
[startup diagnostics](startup-diagnostics.md) explain most of the setup problems.

## The setup

### All the requests are 404 Not Found

Look at the log of the start:

- `0 controllers are created for the magicalized DTOs.` without warnings: Magicalizer hasn’t found the DTOs. They have
  no `[Magicalized]`, or they are in another project whose assembly isn’t loaded yet when `AddMagicalizer` is called
  (see [Type discovery](../building-blocks/type-discovery.md#which-assemblies)).
- Warnings that the DTOs are skipped: fix what they say (see [Startup diagnostics](startup-diagnostics.md)).
- The DTOs are served, but at other routes: compare the routes in the log with the requested ones.

And check that `UseMagicalizer()` is called: it maps the controllers.

### `Unable to resolve service for type 'Microsoft.EntityFrameworkCore.DbContext'`

```text
System.InvalidOperationException: Unable to resolve service for type 'Microsoft.EntityFrameworkCore.DbContext' while attempting to activate 'Magicalizer.Domain.Services.Service`4[...]'.
```

The services of Magicalizer take `DbContext`, and the context is registered only as its own type. Register it as
`DbContext` too:

```csharp
builder.Services.AddDbContext<DbContext, AppDbContext>(options => { /* ... */ });
```

### `Cannot create a DbSet for '...' because this type is not included in the model`

The entity of a model isn’t in the `DbContext`: add a `DbSet` property or configure it in `OnModelCreating`. The
startup diagnostics can’t see this, as the model of the context is built later, so it shows up on the first request.

### `A circular dependency was detected for the service of type 'IService<...>'`

A validator of a model depends on the service of the same model, and the service depends on the validator. A model
validator may use the services of other models (as in [Users see only their own data](../recipes/own-data.md#the-references)),
but for the checks against its own model (e.g. a unique name) it takes the `DbContext` instead (see
[Validators](../building-blocks/validators.md#two-levels)).

### The application crashes with a stack overflow when an object is included

The constructors of the models (or the DTOs) follow the references both ways: a category creates its products, each
product creates its category, which creates its products again. The related models must leave out the references back
(see [Avoiding the cycles](../building-blocks/models.md#avoiding-the-cycles)).

### `'HttpMethod' is an ambiguous reference` (CS0104)

A web project imports `System.Net.Http` by default, which has its own `HttpMethod`. Add an alias:
`using HttpMethod = Magicalizer.Api.Dto.Abstractions.HttpMethod;` (see
[HTTP methods](../customization/http-methods.md#the-name-of-httpmethod)).

### Where is the OpenAPI document?

Magicalizer doesn’t add one. Call `AddOpenApi()` and `MapOpenApi()` of `Microsoft.AspNetCore.OpenApi` (or use another
tool), and the magicalized APIs are in it (see [OpenAPI](../customization/openapi.md)).

## The requests

### 400: `The query parameter isn't supported.`

The API rejects the query parameters it doesn’t know, so a misspelled condition (`name.contain=...`) isn’t silently
ignored. If the parameter is right but processed by something else (e.g. `culture` of the localization), add it to
`IgnoredQueryParameters` (see [Options](../customization/options.md#ignoredqueryparameters)).

### 415 Unsupported Media Type on PATCH

`PATCH` takes JSON Patch, with `Content-Type: application/json-patch+json`, not `application/json` (see
[Endpoints](../http-api/endpoints.md#change-a-part)).

### PUT has erased the properties I didn’t send

`PUT` replaces the whole object: the properties that aren’t in the body get their default values. Send all of them, or
change a part with `PATCH` (see [Creation, PUT, and PATCH](../building-blocks/dtos.md#creation-put-and-patch)).

### The related object has only its identifier

```json
{ "id": 1, "category": { "id": 1, "name": null, "products": null }, "name": "Margherita" }
```

The related objects aren’t loaded unless they are included: the model knows only the foreign key. Ask for them with
`include=category` (see [Inclusion](../http-api/inclusion.md)).

### The string conditions don’t find what they should (or find too much)

The database compares the strings, so its collation decides whether the case matters: SQL Server usually ignores it,
PostgreSQL and SQLite don’t (see [Strings and collations](../http-api/filtering.md#strings-and-collations) and
[PostgreSQL and SQL Server](../recipes/postgresql-and-sql-server.md#the-case-of-the-strings)).

### 500 on PostgreSQL with a date

```text
Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone', only UTC is supported.
```

Pass the dates and times in UTC, with `Z` or an offset: `created.from=2026-01-31T00:00:00Z` (see
[Dates and times on PostgreSQL](../recipes/postgresql-and-sql-server.md#dates-and-times-on-postgresql)).

### The pages repeat or skip objects

Without `sort`, the database returns the objects in any order, which may differ from one page to the next (EF Core logs
a warning about it). Sort the requests, or sort by default in a custom service (see
[Performance](../recipes/performance.md#sort-by-default)).

### The browser doesn’t see Paging-Total-Count

The browser hides the response headers that CORS doesn’t expose. Add them with `WithExposedHeaders` (see
[Browser clients](../recipes/browser-clients.md)).

## The behavior

### The prefilter hides an object, but it’s still in the included objects

A [query prefilter](../customization/query-prefilters.md#the-related-objects) restricts only the queries of its own
entity, not the related objects of other entities. Use a global query filter of EF Core where the restricted entity is
reached through others (see [Soft deletion](../recipes/soft-deletion.md)).

### [Required] and the other DataAnnotations of a DTO are ignored

The DTOs are validated by FluentValidation only, and the DataAnnotations (as well as the implicit `[Required]` of the
non-nullable properties) are switched off for them. Write the rules in a validator (see
[Validators](../building-blocks/validators.md#dataannotations)).

### The nested DTO isn’t validated

A validator checks its own DTO. The rules of a related DTO (e.g. the category of a product) aren’t applied
automatically; include them with `SetValidator` of FluentValidation if they’re needed (see
[Validators](../building-blocks/validators.md)).

### Can I have my own controllers and minimal APIs next to the magicalized ones?

Yes. The magicalized controllers are ordinary controllers, and the services are in the dependency injection, so your
controllers and endpoints use them like any other service (`IService<int, Product, ProductFilter>` or your own
interface). Only the routes must not clash.

### Can a response have a computed property or another shape?

The DTO is built from the model by its constructor, so a computed property (e.g. `IsNew` from `Created`) is a property
of the DTO set there. For a really different response, add an action to a
[custom controller](../customization/custom-controllers.md), or write an ordinary controller that uses the services.

### Is there a cache?

No. Each request queries the database. Use the response caching or the output caching of ASP.NET Core if you need one. The
lists depend on the `Prefer` header, so a cache must vary by it (the responses have `Vary: Prefer` for the HTTP caches).

## Still stuck?

Ask on [GitHub](https://github.com/Magicalizer/Magicalizer/issues) with the log of the start, the request, and the
response.

## See also

- [Startup diagnostics](startup-diagnostics.md)
- [Errors](../http-api/errors.md)
