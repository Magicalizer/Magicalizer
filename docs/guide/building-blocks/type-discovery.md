# Type discovery

You don’t register the entities, models, DTOs, filters, validators, or services: `AddMagicalizer` finds them by the
interfaces they implement. This page describes how, and what happens when a class doesn’t fit.

## What is found

When `AddMagicalizer` is called, Magicalizer looks through the classes of the application:

1. Each model that implements `IModel<TEntity, TFilter>`, whose entity implements `IEntity<...>`, gets a service:
   the default `Service<...>` or, if there is one, your class that implements its `IService<...>`, usually by deriving
   from `Service<...>` (see
   [Custom services](../customization/custom-services.md)).
2. Each validator (`AbstractValidator<T>`) is registered, for the DTOs and the models alike.

Then, when ASP.NET Core builds its controllers:

3. Each DTO marked with `[Magicalized]`, that implements `IDto<TModel>` of such a model, gets a controller: the
   default `MagicalizedController<...>` or, if there is one, your class derived from it (see
   [Custom controllers](../customization/custom-controllers.md)).

The namespaces, folders, and names of the classes don’t matter. A `Product` DTO, a `Product` model, and a `Product`
entity are told apart by their interfaces.

## Which assemblies

Magicalizer looks through the assemblies that are loaded when `AddMagicalizer` is called. In a single-project
application (as in [Your first API](../getting-started/your-first-api.md)), that’s always the case.

> [!WARNING]
> When the DTOs, models, or entities live in separate projects, their assemblies may not be loaded yet: .NET loads an
> assembly when its types are used first, and `Program.cs` may not use any of them. Then nothing is found, and the
> application logs `0 controllers are created for the magicalized DTOs.` The support of such solutions is being
> improved; until then, load the assemblies before `AddMagicalizer`, as shown in
> [A layered solution](../getting-started/layered-solution.md#loading-the-assemblies).

## When something doesn’t fit

A class that looks like a part of the API but can’t be served isn’t an error that stops the application: it’s skipped,
and the reason is logged when the application starts. For example:

```text
warn: Magicalizer[0]
      The magicalized DTO Order is skipped: its model Order doesn't implement IModel<TEntity, TFilter>.
```

The list of the messages, with their causes and fixes, is in [Startup diagnostics](../troubleshooting/startup-diagnostics.md).
The served DTOs are logged too, so the log of the start shows the whole API:

```text
info: Magicalizer[0]
      The magicalized DTO Product is served by MagicalizedController<Product> at /v1/products: GET, POST, PUT, PATCH, DELETE.
info: Magicalizer[0]
      3 controllers are created for the magicalized DTOs.
```

## See also

- [How it works](../introduction/how-it-works.md)
- [Startup diagnostics](../troubleshooting/startup-diagnostics.md)
