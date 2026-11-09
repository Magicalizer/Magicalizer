# Filtering

A list is filtered by the query parameters of the form `property.condition=value`:

```http
GET /v1/products?name.contains=pizza&price.from=10
```

All the conditions of a request must match (they are combined with AND). The names of the properties and the
conditions are case-insensitive: `Name.Contains` is the same as `name.contains`.

What can be filtered is decided by the [filter](../building-blocks/filters.md) of the DTO: a property that isn’t in
the filter, or a condition that its type doesn’t have, is rejected with `400 Bad Request` (see [Errors](errors.md)).
The OpenAPI description of the API lists all the parameters the filter allows.

## Conditions

| Type of the property | Filter | Conditions |
|---|---|---|
| Numbers, dates, and times | `ByteFilter`, `ShortFilter`, `IntegerFilter`, `LongFilter`, `FloatFilter`, `DoubleFilter`, `DecimalFilter`, `DateTimeFilter`, `DateTimeOffsetFilter` | `isnull`, `equals`, `notequals`, `from`, `to`, `in`, `notin` |
| Strings | `StringFilter` | `isnull`, `equals`, `notequals`, `contains`, `startswith`, `endswith`, `in`, `notin` |
| GUIDs and enums | `GuidFilter`, `EnumFilter<TEnum>` | `isnull`, `equals`, `notequals`, `in`, `notin` |
| Booleans | `BooleanFilter` | `isnull`, `equals` |
| Collections | `EnumerableFilter<TFilter>` | `isempty`, `any`, `none` |

| Condition | Matches | Example |
|---|---|---|
| `isnull` | `true`: the null values, `false`: the non-null ones | `name.isnull=false` |
| `equals`, `notequals` | The values that are (not) equal to the value | `id.equals=5` |
| `from`, `to` | The values from or to the value, **including** it | `price.from=10&price.to=20` |
| `in`, `notin` | The values that are (not) among the values; the parameter is repeated for each one | `id.in=1&id.in=2` |
| `contains`, `startswith`, `endswith` | The strings that contain, start, or end with the value | `name.startswith=Quattro` |
| `isempty` | `true`: the empty collections, `false`: the non-empty ones | `photos.isempty=true` |
| `any`, `none` | The collections in which any or no element matches the conditions after it | `photos.any.filename.endswith=.png` |

## Values

The values are passed as text, in the invariant format:

| Type | Format | Example |
|---|---|---|
| Numbers | Digits with a dot as the decimal separator | `price.from=9.5` |
| Dates and times | ISO 8601 | `created.from=2026-01-31`, `created.from=2026-01-31T10:00:00Z` |
| GUIDs | The usual format | `id.equals=6f9619ff-8b86-d011-b42d-00cf4fc964ff` |
| Enums | The name or the number | `diet.equals=vegan`, `diet.equals=2` |
| Booleans | `true` or `false` | `available.equals=true` |

A value that can’t be converted is `400 Bad Request`, e.g. `price.from=abc`:

```json
{
  "status": 400,
  "errors": {
    "Price.From": ["The value 'abc' is not valid for From."]
  }
}
```

### Strings and collations

The strings are compared by the database, as it compares them in its other queries: its collation and its Entity
Framework Core provider decide, e.g., whether the case is ignored. With the default settings:

- SQL Server usually ignores the case everywhere (its default collations are case-insensitive);
- PostgreSQL never ignores it;
- SQLite ignores the case of the ASCII letters in `startswith` and `endswith`, but not in `equals`, `in`, and
  `contains`, as its provider translates them differently.

So `name.contains=marg` finds `Margherita` on SQL Server, but not on PostgreSQL or SQLite. See
[PostgreSQL and SQL Server](../recipes/postgresql-and-sql-server.md) for the ways to change that.

## Related objects

The properties of a related object are reached by its path:

```http
GET /v1/products?category.name.equals=Pizza
```

The path can be as long as the filters go: `photos.any.product.category.name.equals=Pizza` is a valid one too.

## Collections

The conditions of a collection apply to its elements:

```http
# The products without photos
GET /v1/products?photos.isempty=true

# The products with at least one PNG photo
GET /v1/products?photos.any.filename.endswith=.png

# The products with no PNG photos (including the ones without photos)
GET /v1/products?photos.none.filename.endswith=.png

# The categories with a product that has photos
GET /v1/categories?products.any.photos.isempty=false
```

### One element or several elements

When there are several conditions on the elements of the same collection, they are passed in one of two forms.

In the **single form**, the conditions are matched by the same element:

```http
# A category with a product that costs from 12 AND has "rita" in its name
GET /v1/categories?products.any.price.from=12&products.any.name.contains=rita
```

In the **list form**, each indexed group of conditions is matched by any element, so they may be matched by different
elements:

```http
# A category with a product that costs from 12, AND with a product that has "rita" in its name
GET /v1/categories?products.any[0].price.from=12&products.any[1].name.contains=rita
```

In the catalog of the tutorial, the first request returns nothing (no single pizza costs from 12 and is Margherita),
and the second one returns Pizza (Quattro Formaggi costs 12, and Margherita has “rita” in its name).

The forms can’t be combined for the same collection, and the indexes start from 0 and go without gaps:

| Request | Error of `products.any` |
|---|---|
| `products.any.price.from=11&products.any[0].name.contains=a` | `The single and list forms of the collection condition can't be combined.` |
| `products.any[1].price.from=11` | `The indexes of the collection condition must start from 0 and go without gaps.` |

The same applies to `none`: `products.none[0]...&products.none[1]...` matches the collections in which no element
matches the first group and no element matches the second one.

## See also

- [Sorting](sorting.md), [Paging](paging.md), [Inclusion](inclusion.md)
- [Filters](../building-blocks/filters.md): how the filters are defined
- [Errors](errors.md)
