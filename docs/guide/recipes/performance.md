# Performance

A Magicalizer request is one or two SQL queries built by EF Core, with the filtering, sorting, and paging done by the
database, and the entities read without tracking. So the performance is mostly the performance of these queries: what
the clients may ask for, and what the database has to do for it.

## Limit what a request may ask for

Without the limits, `GET /v1/products` returns the whole table, and `include=a.b.c.d` joins half the database. Set the
[options](../customization/options.md) of a public API:

```csharp
builder.Services.AddMagicalizer(options => {
  options.MaxLimit = 100;
  options.MaxInclusionDepth = 2;
});
```

`MaxLimit` is also the limit of the requests without `limit`, so a client that forgets it gets the first 100 objects,
not all of them.

## Skip the total count

A list comes with `Paging-Total-Count`, which is a second query, a `COUNT` with the same conditions. On a large table
with heavy conditions, it may cost as much as the list. A client that doesn’t need it (e.g. an infinite scroll) sends
`Prefer: count=none`, or the application turns it off for everyone with `CountTotalByDefault = false`, and the clients
that need it send `Prefer: count=exact` (see [Paging](../http-api/paging.md#skipping-the-total-count)).

## Sort by default

Without `sort`, the order of the objects is up to the database, and it may differ from one query to the next, so the
pages may repeat or skip objects. EF Core warns about it in the log of each such query with a limit (and with
`MaxLimit`, all of them have one):

```text
warn: Microsoft.EntityFrameworkCore.Query[10102]
      The query uses a row limiting operator ('Skip'/'Take') without an 'OrderBy' operator. This may lead to unpredictable results. ...
```

A custom service can sort its queries by default; the `sort` of a request replaces this order, as its `OrderBy` comes
later:

[!code-csharp[](../../samples/Recipes/Domain/Services/NoteService.cs#DefaultSorting)]

## Index what is filtered and sorted

The conditions become `WHERE`, the sorting `ORDER BY`, so the columns the clients filter and sort by need indexes, as
for any other query:

- the columns of the frequent conditions and sortings (e.g. `created` of a list sorted by date);
- the columns of the [prefilters](../customization/query-prefilters.md), as they are in every query (e.g. the user of
  [the own data](own-data.md): `HasIndex(e => e.UserId)`);
- the foreign keys, which the conditions of the related objects and the inclusions join by (EF Core creates their
  indexes by itself).

`equals`, `in`, `from`, `to`, and `startswith` can use an ordinary index; `contains` and `endswith` can’t, as they search
inside the strings. For a large table searched by a substring, use the full-text search of the database (e.g. in a
[custom service](../customization/custom-services.md) with a search property of the filter).

## Include collections with split queries

An inclusion of a collection (`include=products.photos`) is a join, which repeats the columns of a category for each of
its products and of a product for each of its photos. With large collections, let EF Core load them by separate
queries:

```csharp
builder.Services.AddDbContext<DbContext, AppDbContext>(options => {
  options.UseSqlite(builder.Configuration.GetConnectionString("Default"), o => {
    o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
  });
});
```

The split queries need a stable order of the paged objects too, so [sort by default](#sort-by-default) along with them.

## Deep pages

`offset=100000` makes the database read and skip 100000 rows. For a long list that is read from the start to the end
(an export, a sync, an infinite scroll), page by the key instead of the offset: the client remembers the last
identifier and asks for the next ones.

```http
GET /v1/notes?sort=id&limit=100
GET /v1/notes?id.from=101&sort=id&limit=100
```

The condition goes by the index of the primary key, so each page costs the same. Add `Prefer: count=none`, as the
total isn’t needed here.

## See what is executed

Turn on the log of the SQL commands in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

Each request then logs its queries with their durations, which is the quickest way to find the missing index or the
inclusion that loads too much.

## See also

- [Options](../customization/options.md)
- [Efficient querying in EF Core](https://learn.microsoft.com/ef/core/performance/efficient-querying)
- [Single and split queries in EF Core](https://learn.microsoft.com/ef/core/querying/single-split-queries)
