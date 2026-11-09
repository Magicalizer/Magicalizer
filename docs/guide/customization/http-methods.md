# HTTP methods

By default, a DTO gets all the actions: `GET`, `POST`, `PUT`, `PATCH`, and `DELETE`. To serve only some of them, list
them in `[Magicalized]`:

```csharp
using HttpMethod = Magicalizer.Api.Dto.Abstractions.HttpMethod;

[Magicalized("v1/photos", HttpMethod.Get, HttpMethod.Post, HttpMethod.Delete)]
public class Photo : IDto<Domain.Models.Photo>
```

| Method | Actions |
|---|---|
| `HttpMethod.Get` | The list and the get by the primary key |
| `HttpMethod.Post` | The creation |
| `HttpMethod.Put` | The replacement |
| `HttpMethod.Patch` | The partial update |
| `HttpMethod.Delete` | The deletion |

The other actions aren’t created at all: they aren’t in the OpenAPI description, and the requests to them are
`405 Method Not Allowed`. The log of the start shows what’s served:

```text
info: Magicalizer[0]
      The magicalized DTO Photo is served by PhotosController at /v1/photos: GET (authenticated), POST (authenticated), DELETE (authenticated).
```

## The name of HttpMethod

`HttpMethod` of Magicalizer (`Magicalizer.Api.Dto.Abstractions.HttpMethod`) has the same name as
`System.Net.Http.HttpMethod`, which the web projects import by default (the implicit `using` directives). In such a
project, `HttpMethod.Get` doesn’t compile (CS0104, an ambiguous reference), so add an alias at the top of the file, as
above, or write the full name.

A project of the DTOs that isn’t a web project (see [A layered solution](../getting-started/layered-solution.md))
doesn’t import `System.Net.Http`, so it doesn’t need the alias.

## Read-only APIs

A DTO with `HttpMethod.Get` only is a read-only API: e.g. a catalog that the clients only browse, while the data is
imported by your code through the [services](custom-services.md), which don’t depend on the HTTP methods of the API.

## See also

- [Authorization](authorization.md): requiring an authenticated user or a policy for some methods instead of removing
  them
- [`MagicalizedAttribute` in the API reference](xref:Magicalizer.Api.Dto.Abstractions.MagicalizedAttribute)
