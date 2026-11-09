# Custom controllers

Each `[Magicalized]` DTO gets the default `MagicalizedController<...>`. When its API needs more, derive your controller
from it: Magicalizer uses it instead of the default one, with the route, HTTP methods, and authorization of the DTO. No
registration is needed.

[!code-csharp[](../../samples/Customization/Api/Controllers/PhotosController.cs)]

The type arguments are the types of the primary key, the model, the DTO, and the filter:

| Primary key | Base class |
|---|---|
| One property | `MagicalizedController<TKey, TModel, TDto, TFilter>` |
| Two properties | `MagicalizedController<TKey1, TKey2, TModel, TDto, TFilter>` |
| Three properties | `MagicalizedController<TKey1, TKey2, TKey3, TModel, TDto, TFilter>` |

The controller gets the service of the model by its constructor and keeps it in the `service` field.

## Adding actions

The actions you add get the route of the DTO as their base: the `count` action above is `GET /v1/photos/count`.

```http
GET /v1/photos/count
```

```http
HTTP/1.1 200 OK

0
```

## Overriding actions

The standard actions (`GetAsync`, `PostAsync`, `PutAsync`, `PatchAsync`, `DeleteAsync`) are virtual, so they can be
overridden, e.g. to add a header or to change the response. The validation of the DTOs, the strict query validation,
and the problem details of the errors still apply to them.

`ModelToDto` converts the models to the DTOs for the responses; override it to change that for all the actions.

## See also

- [Custom services](custom-services.md): the logic usually belongs there, so it works outside the API too
- [OpenAPI](openapi.md)
