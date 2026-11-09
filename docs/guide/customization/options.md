# Options

`AddMagicalizer` takes the options of the HTTP API:

[!code-csharp[](../../samples/Customization/Program.cs#Options)]

| Option | Default | What it does |
|---|---|---|
| `MaxLimit` | `null` (no limit) | The maximum number of objects a request gets, and the limit of the requests without `limit` |
| `MaxInclusionDepth` | `null` (no limit) | The maximum number of properties in a property path of an inclusion |
| `IgnoredQueryParameters` | Empty | The query parameters the API allows, although it doesn’t process them |
| `CountTotalByDefault` | `true` | Whether a list is returned with the total count |

The options are of the HTTP API only: the [services](custom-services.md) don’t apply them, so your own code may, e.g.,
get all the objects or include deeper.

## MaxLimit

With `MaxLimit = 100`, a request without `limit` gets 100 objects at most, and the response tells which limit it applied:

```http
GET /v1/products
```

```http
HTTP/1.1 200 OK
Paging-Total-Count: 0
Paging-Limit: 100
```

A greater `limit` is `400 Bad Request`:

```http
GET /v1/products?limit=500
```

```json
{
  "status": 400,
  "errors": {
    "limit": ["The limit can't be greater than 100."]
  }
}
```

Without the maximum limit, a request without `limit` gets all the objects, which is fine for small sets, but a public
API should have it. See [Paging](../http-api/paging.md#maximum-limit).

## MaxInclusionDepth

The inclusions make the joins of the database query, so the long paths make heavy queries. With
`MaxInclusionDepth = 2`, `include=products.photos` is allowed, and a longer path is `400 Bad Request`:

```http
GET /v1/categories?include=products.photos.product
```

```json
{
  "status": 400,
  "errors": {
    "include": ["Unable to include 'products.photos.product': the property path has more than 2 properties."]
  }
}
```

See [Inclusion](../http-api/inclusion.md).

## IgnoredQueryParameters

The API rejects the query parameters it doesn’t know, so a mistyped one isn’t ignored silently:

```http
GET /v1/products?foo=1
```

```json
{
  "status": 400,
  "errors": {
    "foo": ["The query parameter isn't supported."]
  }
}
```

But some parameters are processed by something else, e.g. `culture` of the request localization
(`UseRequestLocalization`) or a cache-busting parameter of a client. Add them to `IgnoredQueryParameters`, and the API
allows them: `GET /v1/products?culture=uk` is `200 OK`. The names are case-insensitive.

## CountTotalByDefault

A list is returned with the `Paging-Total-Count` header, which takes a separate `COUNT` query of the database. If the
clients don’t need it usually (e.g. an infinite scroll), turn it off:

```csharp
builder.Services.AddMagicalizer(options => {
  options.CountTotalByDefault = false;
});
```

Then a client asks for it with the `Prefer: count=exact` header. Either way, `Prefer: count=none` and
`Prefer: count=exact` of a request override the option. See
[Paging](../http-api/paging.md#skipping-the-total-count).

## Other settings

The rest is configured by ASP.NET Core and EF Core as usual:

- the JSON serialization: `AddJsonOptions` of the controllers, e.g. `builder.Services.AddControllers().AddJsonOptions(...)`;
- the routes, the authentication, and the CORS: the usual middleware (see [Authorization](authorization.md) and
  [Paging](../http-api/paging.md#browser-clients));
- the database and its collations: the `DbContext` (see [Filtering](../http-api/filtering.md#strings-and-collations)).

## See also

- [`MagicalizerOptions` in the API reference](xref:Magicalizer.MagicalizerOptions)
