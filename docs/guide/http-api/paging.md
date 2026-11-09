# Paging

A list is paged by two query parameters:

- `offset`: how many objects to skip (0 or more);
- `limit`: how many objects to take at most (0 or more).

```http
# The third page of 10 products
GET /v1/products?sort=name&offset=20&limit=10
```

Page the [sorted](sorting.md) lists only: without sorting, the order isn’t defined, and the pages may overlap.

## Paging headers

The response has the paging metadata in its headers:

```http
HTTP/1.1 200 OK
Paging-Total-Count: 23
Paging-Offset: 20
Paging-Limit: 10
Vary: Prefer
```

| Header | Contains |
|---|---|
| `Paging-Total-Count` | The number of all the objects that match the [filters](filtering.md), regardless of the paging. |
| `Paging-Offset` | The applied offset, when it’s passed. |
| `Paging-Limit` | The applied limit, when it’s passed or the application has the maximum limit (see below). |

So the number of the pages is `ceil(Paging-Total-Count / limit)`.

## Maximum limit

An application may set the maximum number of the objects a request gets (the `MaxLimit`
[option](../customization/options.md)). Then:

- the requests without `limit` get the maximum number of the objects, and `Paging-Limit` tells which one;
- a greater `limit` is `400 Bad Request`:

```json
{
  "status": 400,
  "errors": {
    "limit": ["The limit can't be greater than 100."]
  }
}
```

Without the maximum limit (the default), a request without `limit` gets all the objects.

## Skipping the total count

The total count takes a separate database query, as costly as the filters make it. A client that doesn’t need it
skips it by the `Prefer` header ([RFC 7240](https://www.rfc-editor.org/rfc/rfc7240)):

```http
GET /v1/products?offset=20&limit=10
Prefer: count=none
```

```http
HTTP/1.1 200 OK
Preference-Applied: count=none
Paging-Offset: 20
Paging-Limit: 10
Vary: Prefer
```

An application may turn the count off by default (the `CountTotalByDefault` [option](../customization/options.md)),
then a client asks for it with `Prefer: count=exact`. The response reports the applied preference in the
`Preference-Applied` header, and its `Vary: Prefer` header lets the caches keep the responses with and without the
count apart.

## Browser clients

The browsers let the scripts read only some headers of the responses from the other origins. To make the paging
headers available, expose them by the CORS policy of the application:

```csharp
builder.Services.AddCors(options => {
  options.AddDefaultPolicy(policy => {
    policy.WithOrigins("https://example.com")
      .WithExposedHeaders("Paging-Total-Count", "Paging-Offset", "Paging-Limit", "Preference-Applied");
  });
});
```

See [Browser clients](../recipes/browser-clients.md) for the details.

## See also

- [Filtering](filtering.md), [Sorting](sorting.md), [Inclusion](inclusion.md)
- [Options](../customization/options.md)
