# OpenAPI

The magicalized controllers are the usual controllers of ASP.NET Core, so the API description tools (the API explorer)
see them: `Microsoft.AspNetCore.OpenApi`, Swashbuckle, NSwag, and the UIs built on them (Swagger UI, Scalar) need
nothing special. Add the OpenAPI document as usual:

```csharp
builder.Services.AddOpenApi();

// ...

webApplication.MapOpenApi();
```

`GET /openapi/v1.json` then describes all the magicalized APIs, the [custom controllers](custom-controllers.md)
included.

## The parameters

Magicalizer describes the query parameters as they are passed, not as the C# types that receive them:

- a filter is a flat list of its conditions in lower case, with the [property paths](../http-api/filtering.md) of the
  filters it refers to, e.g. `name.contains`, `price.from`, `category.name.equals`, `isarchived.equals`;
- a condition of a collection is described by the conditions of its element filter, e.g.
  `photos.any.filename.contains`, `photos.none.filename.endswith` (the list form `photos.any[0]...` isn’t described);
- your own filter properties are described as they are, e.g. `search` of
  [the custom service](custom-services.md#a-search-text);
- `sort` and `include` are strings with comma-separated property paths, `offset` and `limit` are integers;
- the list action has the optional `Prefer` header ([Paging](../http-api/paging.md#skipping-the-total-count)).

The element filters are expanded with the filters of the entities they refer to, but only one level deep and without
the ones already in the path, so cyclic references (a product has photos, a photo has a product) don’t produce endless
parameters.

The conditions of a value filter go in the order of their declaration, e.g. `price.isnull`, `price.equals`,
`price.notequals`, `price.in`, `price.notin`, `price.from`, `price.to`.

## Hiding an API

A generated controller has no class to mark with `[ApiExplorerSettings]`, so `[Magicalized]` has the same settings.
`IgnoreApi` hides the API from the description tools (it’s still served):

```csharp
[Magicalized("v1/audit-records", HttpMethod.Get, IgnoreApi = true)]
public class AuditRecord : IDto<Domain.Models.AuditRecord>
```

## Separate documents

`GroupName` puts the API into a group, which `Microsoft.AspNetCore.OpenApi` uses as the name of the document. E.g. the
photos of the sample are an internal API:

[!code-csharp[](../../samples/Customization/Api/Dto/Photo.cs#L1-L8)]

And the application has two documents:

[!code-csharp[](../../samples/Customization/Program.cs#OpenApi)]

| Document | Paths |
|---|---|
| `/openapi/v1.json` | `/v1/categories`, `/v1/categories/{id}`, `/v1/products`, `/v1/products/{id}` |
| `/openapi/internal.json` | `/v1/photos`, `/v1/photos/{id}`, `/v1/photos/count` |

The custom action `/v1/photos/count` of [PhotosController](custom-controllers.md) is in the internal document too, as the
controller has no settings of its own and gets the ones of the DTO.

> [!NOTE]
> By default, a document of `Microsoft.AspNetCore.OpenApi` includes the APIs of its group **and the APIs without a
> group**. So without `ShouldInclude`, `/openapi/internal.json` would describe the categories and the products too. The
> `v1` document doesn’t get the photos either way, as they are in another group.

## Precedence

The settings of the API explorer are taken in the order, as in ASP.NET Core:

1. `[ApiExplorerSettings]` of an action;
2. `[ApiExplorerSettings]` of a [custom controller](custom-controllers.md);
3. `IgnoreApi` and `GroupName` of `[Magicalized]`.

The more specific ones win, so a custom controller can, e.g., put its API into another document than its DTO says, or
hide one of its actions.

## The methods and authorization

The description has only the served [HTTP methods](http-methods.md): the photos have no `PUT` and `PATCH` in it. The
[authorization](authorization.md) is the usual metadata of the endpoints, so describe the security schemes of your
application with the document transformers of `Microsoft.AspNetCore.OpenApi`, as for any other controller.

## See also

- [OpenAPI support in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/openapi/overview)
- [`MagicalizedAttribute` in the API reference](xref:Magicalizer.Api.Dto.Abstractions.MagicalizedAttribute)
