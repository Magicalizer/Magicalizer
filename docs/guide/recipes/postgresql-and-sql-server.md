# PostgreSQL and SQL Server

Magicalizer works through Entity Framework Core, so switching the database is switching the provider of EF Core. The
samples use SQLite for simplicity, and the tests of Magicalizer also run on PostgreSQL and SQL Server. Two things differ
between the databases and show through the API: the comparison of the strings and, on PostgreSQL, the dates and times.

## The provider

# [PostgreSQL](#tab/postgresql)

```console
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

```csharp
builder.Services.AddDbContext<DbContext, AppDbContext>(options => {
  options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});
```

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=app;Username=app;Password=..."
  }
}
```

# [SQL Server](#tab/sqlserver)

```console
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

```csharp
builder.Services.AddDbContext<DbContext, AppDbContext>(options => {
  options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});
```

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=App;Trusted_Connection=true;TrustServerCertificate=true"
  }
}
```

---

Note `AddDbContext<DbContext, AppDbContext>`: the services of Magicalizer take `DbContext`, so the context is
registered as it. Create the database with the migrations of EF Core (`dotnet ef migrations add`,
`dotnet ef database update`) rather than `EnsureCreated` of the samples.

## The case of the strings

The string conditions (`equals`, `contains`, `startswith`, `endswith`, `in`) are compared by the database, so whether
`name.contains=marg` finds “Margherita” depends on it:

| Database | By default |
|---|---|
| SQL Server | The case is usually ignored: the default collations of the servers and databases are case-insensitive (e.g. `SQL_Latin1_General_CP1_CI_AS`) |
| PostgreSQL | The case matters, in all the conditions |

### SQL Server

Nothing to do for the case-insensitive search. Where the case must matter (e.g. a code or a token), set a
case-sensitive collation of the column:

```csharp
modelBuilder.Entity<Coupon>().Property(e => e.Code).UseCollation("Latin1_General_100_CS_AS");
```

### PostgreSQL

The simplest way to ignore the case is the `citext` type (a standard extension of PostgreSQL), for all the string
properties at once:

```csharp
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
  configurationBuilder.Properties<string>().HaveColumnType("citext");
}

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
  modelBuilder.HasPostgresExtension("citext");
  // ...
}
```

Then all the conditions ignore the case: `name.contains=marg`, `name.startswith=marg`, `name.equals=margherita`, and
`name.in=MARGHERITA` all find “Margherita”, and the sorting by the name ignores it too. `citext` doesn’t ignore the
accents, though.

A non-deterministic ICU collation is the other way (its locale also decides, e.g., whether the accents are ignored):

```csharp
modelBuilder.HasCollation("case_insensitive", locale: "und-u-ks-level2", provider: "icu", deterministic: false);
```

```csharp
configurationBuilder.Properties<string>().UseCollation("case_insensitive");
```

> [!WARNING]
> Before PostgreSQL 18, the non-deterministic collations don’t support `LIKE`, so on PostgreSQL 16 `equals` and `in`
> work, but `contains`, `startswith`, and `endswith` fail with
> `0A000: nondeterministic collations are not supported for LIKE` (`500 Internal Server Error`). Use `citext` there,
> or PostgreSQL 18 or newer.

See [collations and case sensitivity](https://www.npgsql.org/efcore/misc/collations-and-case-sensitivity.html) of the
Npgsql provider.

## Dates and times on PostgreSQL

The Npgsql provider maps `DateTime` to `timestamp with time zone` and accepts only the UTC values for it (with
`Kind = Utc`). A value of a filter without a time zone, such as `created.from=2026-01-31`, comes from the query string
without a kind, and the provider rejects it:

```text
System.ArgumentException: Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone', only UTC is supported.
```

The request is `500 Internal Server Error`. The values with a time zone work:

```http
GET /v1/products?created.from=2026-01-31T00:00:00Z
GET /v1/products?created.from=2026-01-31T02:00:00%2B02:00
```

So with PostgreSQL:

- the clients pass the dates and times in UTC, with `Z` or an offset (note `%2B` for `+` in a URL);
- the `DateTime` properties of the DTOs in the bodies of `POST`, `PUT`, and `PATCH` go in UTC with `Z`
  (`"2026-01-31T10:00:00Z"`), as System.Text.Json reads a value without a zone as unspecified and the one with an
  offset as the local time;
- the services set the times in UTC (`DateTime.UtcNow`, as `ProductService` of the samples does).

Otherwise, map the property to `timestamp without time zone` (`HasColumnType("timestamp without time zone")`) if it
stores the local times, not the moments. Then it’s the other way round: the values without a time zone work, and the
ones with `Z` or an offset are rejected.

## See also

- [Strings and collations](../http-api/filtering.md#strings-and-collations)
- [Requirements](../introduction/requirements.md)
- [Collations and case sensitivity in EF Core](https://learn.microsoft.com/ef/core/miscellaneous/collations-and-case-sensitivity)
