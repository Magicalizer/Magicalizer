# Query prefilters

A query prefilter restricts the entities a service works with, before the filter of the request: e.g. it shows the
users only their own objects, or hides the archived ones. Unlike a filter, a client can’t remove it.

## IQueryPrefilter

A prefilter implements `IQueryPrefilter<TEntity, TFilter>` and adds its conditions to the query. This one hides the
archived products from everyone except the editors:

[!code-csharp[](../../samples/Customization/Data/Prefilters/ArchivedProductPrefilter.cs)]

The prefilters are registered by you, in the dependency injection (this one also needs the `HttpContext`, so the
accessor is registered too):

[!code-csharp[](../../samples/Customization/Program.cs#Prefilters)]

An entity may have several prefilters; they are all applied.

## Where it applies

A prefilter applies to everything the service does with the existing entities, so a hidden object behaves as if it
doesn’t exist:

| Operation | With a hidden object |
|---|---|
| The list and the count | It isn’t there, and it isn’t counted in `Paging-Total-Count` |
| The get by the primary key | `404 Not Found` |
| `PUT`, `PATCH` | `404 Not Found` |
| `DELETE` | `404 Not Found` |

A prefilter doesn’t apply to the creation: there is no existing object to check. Use a
[validator](../building-blocks/validators.md) or a [custom service](custom-services.md) to check what’s created (e.g.
to set the owner of a new object).

The prefilters apply to the services, so your own code that uses the services gets the same restrictions. Where there
is no current user (e.g. a background job), the prefilter decides what to do without one.

## The related objects

A prefilter restricts the queries of its own entity only. It doesn’t restrict the same entities when they come as the
related objects of another one: the included ones, and the conditions of the related objects. With the prefilter above,
an anonymous client doesn’t get the archived products from `/v1/products`, but it does get them, and can search by them,
through the categories:

```http
GET /v1/categories?include=products
GET /v1/categories?products.any.isarchived.equals=true
```

```json
[
  {
    "id": 1,
    "name": "Pizza",
    "products": [
      { "id": 1, "name": "Margherita", "price": 10.0, "isArchived": false },
      { "id": 2, "name": "Old", "price": 5.0, "isArchived": true }
    ]
  }
]
```

So a prefilter fits when the restricted entity is reached only by its own API, or through the objects that are
restricted the same way (e.g. the notes of the user’s own notebooks). When other entities refer to it, either don’t let
the clients include or filter through those references, or restrict the entity in EF Core with a global query filter,
which applies to every query, the related objects included (see [Soft deletion](../recipes/soft-deletion.md) and
[Users see only their own data](../recipes/own-data.md#the-related-objects)).

## The filter

`Apply` also gets the filter of the request, so a prefilter may depend on it: e.g. show the archived products only
when the request asks for them (`isArchived.equals=true`) and the user may see them.

## See also

- [Users see only their own data](../recipes/own-data.md)
- [Soft deletion](../recipes/soft-deletion.md)
- [`IQueryPrefilter` in the API reference](xref:Magicalizer.Domain.Services.Abstractions.IQueryPrefilter`2)
