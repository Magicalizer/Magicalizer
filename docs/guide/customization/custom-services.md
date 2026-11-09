# Custom services

Each model that implements `IModel<TEntity, TFilter>` gets a service: the default `Service<...>`, which gets, counts,
creates, edits, and deletes the models, applying the filters, sorting, paging, inclusion, prefilters, and validation.
The controllers use it, and so can your code. When a model needs more, derive your service from the default one.

## Using the services

The services are registered in the dependency injection by their interfaces:

| Primary key | Interface |
|---|---|
| One property | `IService<TKey, TModel, TFilter>` |
| Two properties | `IService<TKey1, TKey2, TModel, TFilter>` |
| Three properties | `IService<TKey1, TKey2, TKey3, TModel, TFilter>` |

So your code (a background job, an import, a controller of your own) works with the models the same way the API
does, with the same validation and prefilters:

```csharp
public class PriceUpdater
{
  private readonly IService<int, Product, ProductFilter> productService;

  public PriceUpdater(IService<int, Product, ProductFilter> productService)
  {
    this.productService = productService;
  }

  public async Task RaisePricesAsync(int categoryId, decimal percent, CancellationToken cancellationToken)
  {
    ProductFilter filter = new ProductFilter { Category = new CategoryFilter { Id = new IntegerFilter { Equals = categoryId } } };

    foreach (Product product in await this.productService.GetAllAsync(filter, cancellationToken: cancellationToken))
    {
      product.Price *= 1 + percent / 100;
      await this.productService.EditAsync(product, cancellationToken);
    }
  }
}
```

| Method | Does |
|---|---|
| `GetAllAsync` | Gets the models by the filter, sorting, paging, and inclusions |
| `CountAsync` | Counts the models by the filter |
| `GetByIdAsync` | Gets a model by the primary key (or `null`) |
| `CreateAsync` | Validates (with the `Create` rule set) and creates a model, returns the created one |
| `EditAsync` | Validates (with the `Edit` rule set) and saves a model |
| `DeleteAsync` | Deletes a model by the primary key |

The sorting and the inclusions are built from the properties of the model, so they are checked by the compiler
(`Magicalizer.Domain` namespace):

```csharp
IEnumerable<Category> categories = await categoryService.GetAllAsync(
  sortings: [new SortingBuilder<Category>(isAscending: true).Add(c => c.Name).Build()],
  limit: 10,
  inclusions: [new InclusionBuilder<Category>().Add(c => c.Products).ThenAdd(p => p.Photos).Build()],
  cancellationToken: cancellationToken
);
```

The limits of the [options](options.md) (`MaxLimit`, `MaxInclusionDepth`) are of the HTTP API only: your code gets what
it asks for.

The failures are the exceptions: `ValidationException` of FluentValidation (the model isn’t valid or refers to a model
that doesn’t exist), `ModelNotFoundException`, `ModelAlreadyExistsException`, and `ModelInUseException`. The API turns
them into `400`, `404`, and `409` (see [Errors](../http-api/errors.md)).

## A custom service

A class that implements `IService<...>` of the model is registered instead of the default service: the API and your code
get it by the same interface. No registration is needed. The usual way is to derive it from `Service<...>` (with the
same type arguments) and override only what differs; a class that implements the interface directly implements all its
methods itself.

[!code-csharp[](../../samples/Customization/Domain/Services/ProductService.cs)]

Override what the model needs:

| Member | Override to |
|---|---|
| `GetAllAsync`, `CountAsync`, `GetByIdAsync` | Change how the models are read |
| `CreateAsync`, `EditAsync`, `DeleteAsync` | Add the logic around the saving (e.g. set the values, send the notifications) |
| `GetPreprocessedFilter` | Change the filter before it’s applied (e.g. turn a search text into the conditions) |
| `GetPrefilteredQuery` | Change the query before the filter (the [prefilters](query-prefilters.md) are usually simpler) |
| `EntityToModel` | Change how an entity is converted to the model |

The service has the `dbContext`, `queryPrefilters`, and `validator` fields for its own queries. A query of your own can
apply a filter and the paging the same way the service does, with `ApplyFiltering` and `ApplyPaging` of the
`Magicalizer.Data.Extensions` namespace (e.g. `query.ApplyFiltering(filter).ApplyPaging(offset, limit)`).

### A search text

`GetPreprocessedFilter` above turns the `Search` property of the filter into a condition of the name. `Search` isn’t a
filter, so Magicalizer doesn’t apply it itself (see [Filters](../building-blocks/filters.md#properties-that-arent-applied)),
but it’s still taken from the query:

```http
GET /v1/products?search=Marg
```

## An interface of your own

A custom service may implement its own interface, derived from `IService<...>`, to add the methods the default one
doesn’t have:

[!code-csharp[](../../samples/Customization/Domain/Services/IProductService.cs)]

Then it’s registered by both interfaces: the API gets it by `IService<int, Product, ProductFilter>`, and your code
by `IProductService`.

## See also

- [Query prefilters](query-prefilters.md)
- [Custom controllers](custom-controllers.md)
- [`Service<TKey, TEntity, TModel, TFilter>` in the API reference](xref:Magicalizer.Domain.Services.Service`4)
