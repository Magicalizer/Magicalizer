# DTOs

The DTOs (data transfer objects) are the contract of the API: the JSON the clients send and get. A DTO is converted
from and to its model, and the DTOs marked with `[Magicalized]` get their REST APIs.

## IDto

A DTO implements `IDto<TModel>` of the `Magicalizer.Api.Dto.Abstractions` package and has:

- a constructor that takes the model, which converts the model to the DTO;
- `ToModel`, which converts the DTO to the model;
- a parameterless constructor, which the JSON deserializer uses.

[!code-csharp[](../../samples/GettingStarted/Api/Dto/Product.cs)]

The related DTOs leave out the references back, as the [models](models.md#avoiding-the-cycles) do.

The DTOs don’t depend on ASP.NET Core or Entity Framework Core: the package has only the interfaces and the
attributes. So the DTOs can live in a project of their own (see [Packages and layers](../introduction/packages-and-layers.md)).
Note that a DTO refers to its model (`IDto<TModel>`, `ToModel`), so a project of the DTOs references the models too; a
.NET client that needs only the JSON contract is better served by its own classes or a client generated from the
[OpenAPI](../customization/openapi.md) description.

## [Magicalized]

`[Magicalized]` gives the DTO its REST API:

```csharp
[Magicalized("v1/products")]
public class Product : IDto<Domain.Models.Product>
```

| Parameter | Does |
|---|---|
| `route` | The base route of the API, e.g. `v1/products` (the endpoints are `/v1/products` and `/v1/products/{id}`). |
| `methods` | The HTTP methods the API serves, all of them by default (see [HTTP methods](../customization/http-methods.md)). |
| `IgnoreApi` | Hides the API from the OpenAPI description (see [OpenAPI](../customization/openapi.md)). |
| `GroupName` | Puts the API into an OpenAPI document of its own (see [OpenAPI](../customization/openapi.md)). |

The authorization is set by the attributes next to it, `[AuthenticatedOnly]` and `[AuthorizedOnly]` (see
[Authorization](../customization/authorization.md)).

A DTO without `[Magicalized]` has no API of its own, but it can still be a part of the other DTOs (e.g. a nested
address of an order), converted by their constructors and `ToModel`.

## What a DTO exposes

The DTO decides what the clients see and send, independently of the model:

- a property that the DTO doesn’t have isn’t in the responses, and it isn’t taken from the requests (e.g. a password
  hash of a user);
- the DTO may shape the data differently (e.g. a full name made of the first and the last names), as long as
  `ToModel` can convert it back.

Note that the [filtering](../http-api/filtering.md), [sorting](../http-api/sorting.md) and
[inclusion](../http-api/inclusion.md) work with the properties of the filter and the entity, not of the DTO.

## Creation, PUT, and PATCH

- On **creation** (`POST`), the DTO from the request is converted to the model by `ToModel`, created by the service,
  and the created model is converted back by the constructor (with the values the application has set).
- On **replacement** (`PUT`), the DTO is converted to the model and saved as it is: the properties that aren’t in the
  request get their default values. E.g. `ToEntity` of the product model sets a missing creation date to the current
  time (`this.Created == default ? DateTime.UtcNow : this.Created`), so a `PUT` without `created` resets it. A client
  sends all the properties it wants to keep, or uses `PATCH`.
- On **partial update** (`PATCH`), the current model is loaded and converted to the DTO, the JSON Patch is applied to
  the DTO, and the result is validated and saved. So only the properties of the DTO can be patched, and the primary
  key can’t be changed.

## Validation

The DTOs are validated by their FluentValidation validators before anything is saved (see [Validators](validators.md)).
The DataAnnotations attributes (e.g. `[Required]`) and the implicit `[Required]` of ASP.NET Core for the non-nullable
reference types are turned off for the DTOs: the validators are the only source of the rules.

## See also

- [Models](models.md), [Validators](validators.md)
- [Endpoints](../http-api/endpoints.md): the API a DTO gets
- [`MagicalizedAttribute` in the API reference](xref:Magicalizer.Api.Dto.Abstractions.MagicalizedAttribute)
