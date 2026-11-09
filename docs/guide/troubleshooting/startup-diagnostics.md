# Startup diagnostics

Magicalizer finds your classes by their interfaces (see [Type discovery](../building-blocks/type-discovery.md)), so a
mistake in a class doesn’t stop the application: the class is skipped, or served with a problem that shows up only on
a request. To make such mistakes visible, Magicalizer logs what it has found when the application starts, under the
`Magicalizer` category.

## Reading the log

A healthy start lists each served DTO and the total:

```text
info: Magicalizer[0]
      The magicalized DTO Category is served by MagicalizedController<Category> at /v1/categories: GET, POST (policy Catalog.Edit), PUT (policy Catalog.Edit), PATCH (policy Catalog.Edit), DELETE (policy Catalog.Edit).
info: Magicalizer[0]
      The magicalized DTO Photo is served by PhotosController at /v1/photos: GET (authenticated), POST (authenticated), DELETE (authenticated).
info: Magicalizer[0]
      The magicalized DTO Product is served by MagicalizedController<Product> at /v1/products: GET, POST, PUT, PATCH, DELETE.
info: Magicalizer[0]
      3 controllers are created for the magicalized DTOs.
```

Each line tells:

- the DTO and the controller: `MagicalizedController<...>` is the default one, a class name is your
  [custom controller](../customization/custom-controllers.md);
- the route;
- the [HTTP methods](../customization/http-methods.md) with their [authorization](../customization/authorization.md):
  `authenticated`, `policy ...`, or nothing for the anonymous access.

The warnings come before these lines. Read them first: each of them is a class that doesn’t work as intended.

## More details

The services of the models are logged at the `Debug` level. Turn it on for the `Magicalizer` category, e.g. in
`appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Magicalizer": "Debug"
    }
  }
}
```

```text
dbug: Magicalizer[0]
      The model Category is served by the default service.
dbug: Magicalizer[0]
      The model Product is served by the custom service ProductService.
```

## The messages

### `The magicalized DTO ... is served by ...`

```text
info: The magicalized DTO Product is served by MagicalizedController<Product> at /v1/products: GET, POST, PUT, PATCH, DELETE.
```

The DTO is served. Check the route, the controller, the methods, and their authorization: that’s the API your clients
get.

### `N controllers are created for the magicalized DTOs`

```text
info: 0 controllers are created for the magicalized DTOs.
```

The total. If it’s less than the number of your DTOs, look for the warnings above it. If it’s `0` and there are no
warnings at all, Magicalizer has found no classes: usually the DTOs are in another project whose assembly wasn’t loaded
when `AddMagicalizer` was called (see [Type discovery](../building-blocks/type-discovery.md#which-assemblies)), or the
DTOs don’t have `[Magicalized]`.

### `The magicalized DTO ... is skipped: it doesn't implement IDto<TModel>`

```text
warn: The magicalized DTO Order is skipped: it doesn't implement IDto<TModel>.
```

The class has `[Magicalized]`, but implements the non-generic `IDto` (or nothing), so there is no model to serve it
with. Implement `IDto<TModel>` with the model of the DTO (see [DTOs](../building-blocks/dtos.md)).

### `The magicalized DTO ... is skipped: its model ... doesn't implement IModel<TEntity, TFilter>`

```text
warn: The magicalized DTO Tag is skipped: its model TagModel doesn't implement IModel<TEntity, TFilter>.
```

The model has no entity or no filter (e.g. it implements `IModel<TEntity>` only), so there is no service for it.
Implement `IModel<TEntity, TFilter>` (see [Models](../building-blocks/models.md)).

### `The magicalized DTO ... is skipped: its entity ... doesn't implement IEntity<TKey> ...`

```text
warn: The magicalized DTO Log is skipped: its entity LogEntity doesn't implement IEntity<TKey>, IEntity<TKey1, TKey2> or IEntity<TKey1, TKey2, TKey3>.
```

The entity implements the non-generic `IEntity`, so the types of its primary key are unknown, and so are the routes
by the key. Implement `IEntity<TKey>` (or the one with two or three keys) with the types of the primary key properties,
in the order of the primary key (see [Entities](../building-blocks/entities.md#primary-keys)).

### `The model ... doesn't have a service: its entity ... doesn't implement IEntity<TKey> ...`

```text
warn: The model LogModel doesn't have a service: its entity LogEntity doesn't implement IEntity<TKey>, IEntity<TKey1, TKey2> or IEntity<TKey1, TKey2, TKey3>.
```

The same cause, seen from the side of the model: no `IService<...>` is registered for it, so your own code can’t get
one either. The fix is the same.

### `The magicalized DTO ... doesn't have a constructor that takes its model ...`

```text
warn: The magicalized DTO ProductSummary doesn't have a constructor that takes its model Product, so the requests to its controller will fail.
```

The controller is created, but it converts the models to the DTOs by this constructor, so the requests that return a
DTO fail with `500 Internal Server Error`:

```text
System.InvalidOperationException: The type ProductSummary must have a constructor that takes Product.
```

Add a public constructor that takes the model (and keep the parameterless one for the requests with a body).

### `The model ... of the magicalized DTO ... doesn't have a constructor that takes its entity ...`

```text
warn: The model NoteModel of the magicalized DTO Note doesn't have a constructor that takes its entity NoteEntity, so the requests to the controller of the DTO will fail.
```

The same for the model: the service converts the entities to the models by this constructor. Add a public constructor
that takes the entity.

### `The magicalized DTO ... requires the policy ... for ..., but it isn't registered`

```text
warn: The magicalized DTO Secret requires the policy Secrets.Read for GET, but it isn't registered, so the requests will fail.
```

`[AuthorizedOnly("Secrets.Read")]` names a policy that `AddAuthorization` doesn’t define (often a typo, or a constant
that differs). The requests fail with `500 Internal Server Error`:

```text
System.InvalidOperationException: The AuthorizationPolicy named: 'Secrets.Read' was not found.
```

Register the policy (see [Authorization](../customization/authorization.md)), or fix its name. A constant shared by the
attribute and the registration prevents such typos.

### `The model ... doesn't have a service: it doesn't implement IModel<TEntity, TFilter>`

```text
dbug: The model TagModel doesn't have a service: it doesn't implement IModel<TEntity, TFilter>.
```

`Debug` only: the model can’t have a service. That’s fine for a model used only inside other models; otherwise,
implement `IModel<TEntity, TFilter>`.

### `The model ... is served by the default service / the custom service ...`

```text
dbug: The model Product is served by the custom service ProductService.
```

`Debug` only: which service the model gets. If your [custom service](../customization/custom-services.md) isn’t listed,
it doesn’t implement `IService<...>` of this model, e.g. it derives from the service of another model, or its type
arguments differ (another filter or key type). Deriving from `Service<...>` of the model implements it.

## What isn’t checked

Some mistakes show up only on a request, as `500 Internal Server Error`; see the [FAQ](faq.md):

- the entity isn’t in the model of the `DbContext`;
- the `DbContext` isn’t registered as `DbContext`;
- a model validator depends on the service of its own model.

## See also

- [Type discovery](../building-blocks/type-discovery.md)
- [FAQ](faq.md)
