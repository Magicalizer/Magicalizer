# Entities

The entities are the classes of Entity Framework Core: they are mapped to the tables of the database. Magicalizer
doesn’t change how you write and configure them; it only needs to know their primary keys.

## IEntity

An entity implements `IEntity` of the `Magicalizer.Data.Entities.Abstractions` package, whose type arguments are the
types of its primary key properties:

[!code-csharp[](../../samples/GettingStarted/Data/Entities/Product.cs)]

| Interface | Primary key |
|---|---|
| `IEntity<TKey>` | One property (e.g. `int Id`) |
| `IEntity<TKey1, TKey2>` | Two properties |
| `IEntity<TKey1, TKey2, TKey3>` | Three properties |
| `IEntity` | None (such an entity can be used, but it can’t have a service or an API) |

The interface has no members: it’s a marker that gives the types of the primary key to the generic services and
controllers, so they can take the primary key as typed arguments (e.g. `GetByIdAsync(int id)`) and routes (e.g.
`/v1/products/{id}`).

## Primary keys

The primary key is configured in the database context, as usual. The type arguments of `IEntity` follow its
properties in the same order:

```csharp
public class OrderLine : IEntity<int, int>
{
  public int OrderId { get; set; }
  public int Number { get; set; }
  public string? Product { get; set; }
  public int Quantity { get; set; }
}
```

```csharp
modelBuilder.Entity<OrderLine>(etb => {
  etb.HasKey(e => new { e.OrderId, e.Number });
  etb.ToTable("OrderLines");
});
```

Such an entity gets the routes with a segment for each primary key property: `GET /v1/order-lines/12/3` gets the line
3 of the order 12 (see [Endpoints](../http-api/endpoints.md#composite-primary-keys)).

When the database generates the primary key (e.g. an identity column), the clients can’t set it on creation: the
value of a request is ignored. When it doesn’t (e.g. a code or a composite key), the value of the request is used,
and an existing one is `409 Conflict`.

## The database context

Magicalizer works with any database context, registered by the base `DbContext` type:

```csharp
builder.Services.AddDbContext<DbContext, AppDbContext>(options => {
  options.UseSqlite(builder.Configuration.GetConnectionString("Default"));
});
```

The services of Magicalizer depend on `DbContext`, so they don’t know the context type of the application. The
relationships, indexes, conversions, and the rest are configured in the context as usual, and Magicalizer uses them:
e.g. the [inclusion](../http-api/inclusion.md) checks the navigation properties of the model of the context, and the
services check the foreign keys to report the references to the objects that don’t exist.

## Navigation properties

The related entities are referred to by the navigation properties (and the foreign key properties), as usual:

[!code-csharp[](../../samples/GettingStarted/Data/Entities/Category.cs)]

The navigation properties are what the clients [include](../http-api/inclusion.md) and
[filter](../http-api/filtering.md) by, and what the [sorting](../http-api/sorting.md) paths go through. They aren’t
loaded unless they are included.

## See also

- [Models](models.md): the domain layer the entities are converted to
- [Packages and layers](../introduction/packages-and-layers.md)
- [`IEntity` in the API reference](xref:Magicalizer.Data.Entities.Abstractions.IEntity)
