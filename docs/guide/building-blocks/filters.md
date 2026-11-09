# Filters

A filter describes how the objects of a model can be filtered: which properties, by which conditions. The clients
pass the conditions in the query (see [Filtering](../http-api/filtering.md)), and the services apply them to the
entities. A filter is the second type argument of the model: `IModel<Data.Entities.Product, ProductFilter>`.

## IFilter

A filter implements `IFilter` of the `Magicalizer.Filters.Abstractions` package. Each of its properties filters the
entity property with the same name:

[!code-csharp[](../../samples/GettingStarted/Filters/ProductFilter.cs)]

A property of a filter is one of these:

| Property | Filters | Query |
|---|---|---|
| A value filter, e.g. `StringFilter` | A scalar property of the entity | `name.contains=pizza` |
| The filter of a related entity, e.g. `CategoryFilter` | A navigation property | `category.name.equals=Pizza` |
| `EnumerableFilter<TFilter>` | A collection navigation property | `photos.any.filename.endswith=.png` |

The properties are nullable: a filter is created from the query, and only the conditions that are set are applied.
The entity property that isn’t in the filter can’t be filtered by, and the query that tries is rejected.

## Value filters

| Filter | For the properties of type | Conditions |
|---|---|---|
| `ByteFilter`, `ShortFilter`, `IntegerFilter`, `LongFilter` | `byte`, `short`, `int`, `long` | `IsNull`, `Equals`, `NotEquals`, `From`, `To`, `In`, `NotIn` |
| `FloatFilter`, `DoubleFilter`, `DecimalFilter` | `float`, `double`, `decimal` | the same |
| `DateTimeFilter`, `DateTimeOffsetFilter` | `DateTime`, `DateTimeOffset` | the same |
| `StringFilter` | `string` | `IsNull`, `Equals`, `NotEquals`, `Contains`, `StartsWith`, `EndsWith`, `In`, `NotIn` |
| `GuidFilter` | `Guid` | `IsNull`, `Equals`, `NotEquals`, `In`, `NotIn` |
| `EnumFilter<TEnum>` | an enum | the same |
| `BooleanFilter` | `bool` | `IsNull`, `Equals` |

The filters work with the nullable properties too (`int?`, `DateTime?`, and so on), and `IsNull` is how the nulls are
found.

## Nested filters

The filters of the related entities are nested, so the clients reach the related objects by the paths:
`ProductFilter.Category` is a `CategoryFilter`, so `category.name.equals=Pizza` works. A filter may refer back to the
filter that refers to it (`CategoryFilter.Products` is an `EnumerableFilter<ProductFilter>`): the paths are taken
from the query, so there are no cycles to worry about.

## Filters in code

The filters are plain classes, so your code creates them to call the services:

```csharp
IEnumerable<Product> pizzas = await productService.GetAllAsync(
  new ProductFilter { Category = new CategoryFilter { Name = new StringFilter { Equals = "Pizza" } } }
);
```

## Properties that aren’t applied

A filter may have the properties that Magicalizer doesn’t apply to the queries, but your code does: e.g. a search
text that a [custom service](../customization/custom-services.md) or a [query prefilter](../customization/query-prefilters.md)
turns into its own conditions.

- A property of a type that isn’t a filter (e.g. `string? Search`) is taken from the query (`search=pizza`), but
  Magicalizer doesn’t apply it.
- A property of a filter type that is marked with `[IgnoreFilter]` isn’t applied either.

## See also

- [Filtering](../http-api/filtering.md): the query parameters the filters allow
- [Models](models.md), [Query prefilters](../customization/query-prefilters.md)
- [`Magicalizer.Filters.Abstractions` in the API reference](xref:Magicalizer.Filters.Abstractions)
