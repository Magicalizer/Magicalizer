# What is Magicalizer

Magicalizer is a .NET library that creates a REST API ASP.NET Core web application from your classes. You write the
entities, the models, the DTOs, the filters, and the validators; each DTO marked with `[Magicalized]` gets a
complete API:

```csharp
[Magicalized("v1/products")]
public class Product : IDto<Domain.Models.Product>
```

```http
GET    /v1/products?category.name.equals=Pizza&price.to=15&sort=-price&offset=0&limit=10&include=photos
GET    /v1/products/1
POST   /v1/products
PUT    /v1/products
PATCH  /v1/products/1
DELETE /v1/products/1
```

There are no controllers, services, query builders, or mapping configurations to write, and no code generation: the
controllers and the services are generic classes that Magicalizer closes over your types when the application starts.

## What you get

- **The standard actions** for each DTO: a list, an object by the primary key (including the composite keys), creation,
  replacement, partial update by JSON Patch, and deletion. The HTTP methods can be limited for each DTO.
- **Filtering** by the properties of the object, of the related objects (`category.name.equals=Pizza`) and of the
  elements of the collections (`photos.any.filename.endswith=.jpg`).
- **Sorting** by several properties in both directions, **paging** with the total count in the headers, and
  **inclusion** of the related objects (`include=products.photos`).
- **Strict query validation**: a misspelled parameter, an invalid value, sorting, or inclusion is `400 Bad Request` with
  all the errors of the request at once, not silently ignored.
- **Validation** by FluentValidation of the DTOs (in the API) and of the models (in the services), with the separate
  rules for creation and editing.
- **Errors** as problem details: `400`, `404`, `409` for the duplicates and the objects other ones refer to.
- **Authorization** for each DTO and HTTP method, and **query prefilters** that restrict the data (e.g. the users see
  only their own objects) for every request.
- **OpenAPI**: the filter conditions are described as they are passed.
- **Startup diagnostics**: the log tells which DTOs are served and which classes are misconfigured.

The full description of the API is in the [HTTP API](../http-api/endpoints.md) section.

## What it doesn’t take from you

Magicalizer doesn’t hide ASP.NET Core and Entity Framework Core, it builds on them:

- your `DbContext` is configured as usual, with any EF Core provider and your own migrations;
- the generated controllers are ordinary controllers: the middleware, authentication, CORS, JSON settings, and OpenAPI
  tools of ASP.NET Core apply to them;
- a [custom service](../customization/custom-services.md) or [controller](../customization/custom-controllers.md)
  derived from the standard one replaces it automatically, so you override only what differs and keep the rest;
- the services are available to your own code (background jobs, imports, other controllers), not only to the API.

## The layers

The classes go by the layers, so the API, the domain logic, and the database stay apart:

| Layer | Class | What it is |
|---|---|---|
| API | DTO | What the clients send and get |
| Domain | Model | What your business logic works with |
| Data | Entity | What EF Core maps to the tables |

And beside them, the filter tells which properties can be filtered and how, and the validators check the DTOs and the
models. See [How it works](how-it-works.md) for the path of a request through the layers.

## When it fits

Magicalizer fits the APIs where most of the endpoints are the data: catalogs, directories, admin panels, back offices,
mobile and SPA backends, internal services. There, the routine of the controllers, filters, and paging is the bulk of the
code, and Magicalizer removes it, while the few endpoints that do more get [custom code](../customization/custom-services.md).

It fits less when:

- the API is mostly operations, not resources (e.g. `POST /orders/1/ship`, workflows, RPC-like APIs) — you can still
  use Magicalizer for the resources and plain controllers for the operations, side by side;
- the domain logic is rich and the objects change by the commands of an aggregate (DDD), not by replacing their
  properties;
- the data doesn’t live in a database through EF Core.

## Next

- [How it works](how-it-works.md)
- [Your first API](../getting-started/your-first-api.md), step by step
