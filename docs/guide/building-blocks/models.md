# Models

The models are the domain layer: the services take and return them, and your own code (e.g. a background job or a
custom service) works with them instead of the entities or the DTOs. A model is converted from and to its entity.

## IModel

A model implements `IModel` of the `Magicalizer.Domain.Models.Abstractions` package:

| Interface | The model |
|---|---|
| `IModel<TEntity, TFilter>` | Is converted to `TEntity`, filtered by `TFilter`, and gets a service (and can have an API) |
| `IModel<TEntity>` | Is converted to `TEntity`, but doesn’t get a service (e.g. a model used only inside other models) |
| `IModel` | Is a marker only |

A model that is served by Magicalizer implements `IModel<TEntity, TFilter>` and has:

- a constructor that takes the entity, which converts the entity to the model;
- `ToEntity`, which converts the model to the entity;
- a parameterless constructor, which the DTOs use to create it.

[!code-csharp[](../../samples/GettingStarted/Domain/Models/Product.cs)]

Magicalizer calls the constructor that takes the entity itself (it’s compiled once, so it’s as fast as a direct
call). A model without it is reported at startup, and the requests to its API fail (see
[Startup diagnostics](../troubleshooting/startup-diagnostics.md)).

## Related models

A model refers to the related models, not to their entities or their foreign keys: a product has its `Category`, not
`CategoryId`. When the related entity is loaded (included), the related model is created from it; when it isn’t, the
model still keeps its primary key, so that `ToEntity` keeps the foreign key:

```csharp
this.Category = productEntity.Category == null ?
  new Category { Id = productEntity.CategoryId } :
  new Category(productEntity.Category, ignoreProducts: true);
```

### Avoiding the cycles

The related entities refer to each other: an included category has its products, each of which refers back to the
category, and so on. A conversion that follows all the references would never end. So the related models leave out
the references back: the constructors take the `ignore...` parameters, and a model passes them when it creates the
related ones.

```csharp
// The products of a category don't need the category: it's the one they belong to.
this.Products = categoryEntity.Products?.Select(p => new Product(p, ignoreCategory: true, ignorePhotos: false)).ToList();
```

The constructor that takes only the entity (the one Magicalizer calls) includes everything, and the ones with the
`ignore...` parameters are used for the related models.

## ToEntity

`ToEntity` creates the entity to save, with the scalar properties and the foreign keys. It usually doesn’t create the
related entities: they are saved by their own services.

It’s also the place for the values the model sets itself, e.g. the creation date:

```csharp
Created = this.Created == default ? DateTime.UtcNow : this.Created
```

## Validation

The models are validated by their validators in the services, on every creation and update, whether it comes from
the API or from your code (see [Validators](validators.md)). That’s where the domain rules belong.

## See also

- [Entities](entities.md), [DTOs](dtos.md), [Filters](filters.md)
- [Custom services](../customization/custom-services.md): working with the models in your code
- [`IModel` in the API reference](xref:Magicalizer.Domain.Models.Abstractions.IModel`2)
