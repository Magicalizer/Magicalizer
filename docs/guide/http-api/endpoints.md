# Endpoints

Each DTO marked with `[Magicalized]` gets a REST API at the route of the attribute. This page describes its endpoints
as the clients see them. The examples use the catalog of [Your first API](../getting-started/your-first-api.md):
`[Magicalized("v1/products")]` on the `Product` DTO.

| Request | Does | Success |
|---|---|---|
| `GET /v1/products` | Gets a list of products (filtered, sorted, paged) | `200 OK` with an array |
| `GET /v1/products/{id}` | Gets a product by its primary key | `200 OK` with the object |
| `POST /v1/products` | Creates a product | `201 Created` with the object |
| `PUT /v1/products` | Replaces a product | `204 No Content` |
| `PATCH /v1/products/{id}` | Changes a part of a product (JSON Patch) | `204 No Content` |
| `DELETE /v1/products/{id}` | Deletes a product | `204 No Content` |

An application may serve only some of them (see [HTTP methods](../customization/http-methods.md)) and require
authorization for some of them (see [Authorization](../customization/authorization.md)). The failures are described
in [Errors](errors.md).

## Get a list

```http
GET /v1/products?category.name.equals=Pizza&sort=-price&offset=0&limit=10&include=photos
```

The list takes the query parameters of [filtering](filtering.md), [sorting](sorting.md), [paging](paging.md) and
[inclusion](inclusion.md). With none of them, it returns all the objects (or as many as the application’s maximum
limit allows). An empty list is `200 OK` with `[]`.

## Get by the primary key

```http
GET /v1/products/1?include=category,photos
```

Returns the object, or `404 Not Found` if there is no object with this primary key. The only query parameter it takes
is `include`.

### Composite primary keys

When the primary key consists of several properties (an entity implements `IEntity<TKey1, TKey2>` or
`IEntity<TKey1, TKey2, TKey3>`), the route has a segment for each of them, in the order of the primary key:

```http
GET /v1/order-lines/12/3
PATCH /v1/order-lines/12/3
DELETE /v1/order-lines/12/3
```

## Create

```http
POST /v1/products
Content-Type: application/json

{"category": {"id": 1}, "name": "Margherita", "price": 9.5}
```

```http
HTTP/1.1 201 Created
Location: /v1/products/1
Content-Type: application/json

{"id": 1, "category": {"id": 1, "name": null, "products": null}, "name": "Margherita", "price": 9.5, ...}
```

The response has the created object (with the values the application has set, e.g. the primary key or the creation
date) and its address in the `Location` header.

- The primary key the database generates can’t be chosen by a client: the one in the request is ignored. When the
  primary key isn’t generated (e.g. a code), it’s taken from the request, and an existing one is
  `409 Conflict`.
- The related objects are referred to by their primary keys (`"category": {"id": 1}`). A reference to an object that
  doesn’t exist is `400 Bad Request`.
- The rules of the validators without a rule set and of the `Create` rule set are checked.

## Replace

```http
PUT /v1/products
Content-Type: application/json

{"id": 1, "category": {"id": 1}, "name": "Margherita", "price": 10.5}
```

`PUT` replaces the whole object, which is found by the primary key in the body: the properties that aren’t in the
request get their default values, so a client sends all of them. To change only some properties, use `PATCH`.

The object that doesn’t exist is `404 Not Found`. The rules without a rule set and the ones of the `Edit` rule set
are checked.

## Change a part

```http
PATCH /v1/products/1
Content-Type: application/json-patch+json

[
  {"op": "replace", "path": "/price", "value": 10.5},
  {"op": "replace", "path": "/name", "value": "Margherita Classica"}
]
```

The body is a [JSON Patch](https://jsonpatch.com/) (RFC 6902) of the DTO. The current object is loaded, the patch is
applied to it, the result is validated (the rules without a rule set and the ones of the `Edit` rule set) and saved.

- The content type must be `application/json-patch+json`; `application/json` is `415 Unsupported Media Type`.
- The primary key can’t be changed: such a patch is `400 Bad Request` (`The primary key can't be changed.`).
- A path that the DTO doesn’t have is `400 Bad Request` (e.g. `The target location specified by path segment 'colour' was not found.`).

## Delete

```http
DELETE /v1/products/1
```

Deletes the object. The object that doesn’t exist is `404 Not Found`. When the database doesn’t allow the deletion
(e.g. other objects refer to this one and the foreign key restricts it), it’s `409 Conflict`.

## The JSON of the objects

The objects are serialized as ASP.NET Core does it by default: the property names are in camel case, and the related
objects that aren’t [included](inclusion.md) have only their primary keys (or are `null`, depending on the DTO). The
properties the DTO doesn’t have are ignored in the requests.

## See also

- [Filtering](filtering.md), [Sorting](sorting.md), [Paging](paging.md), [Inclusion](inclusion.md)
- [Errors](errors.md)
- [DTOs](../building-blocks/dtos.md): how the DTOs and their routes are defined
