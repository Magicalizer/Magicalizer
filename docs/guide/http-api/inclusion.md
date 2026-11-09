# Inclusion

The related objects and collections aren’t loaded by default: a product comes with only the primary key of its
category, and without its photos. The `include` query parameter loads them:

```http
GET /v1/products/1?include=category,photos
```

```json
{
  "id": 1,
  "category": {"id": 1, "name": "Pizza", "products": null},
  "name": "Margherita",
  "price": 9.5,
  "created": "2026-11-06T13:29:04.7960396",
  "photos": [{"id": 1, "product": null, "filename": "margherita.jpg"}]
}
```

`include` takes the comma-separated property paths (or the parameter is repeated). It works with both the lists and
the gets by the primary key. The names are case-insensitive.

## Nested objects

A longer path includes the nested objects, together with the ones on the way:

```http
# A category with its products and their photos
GET /v1/categories/1?include=products.photos
```

## What can be included

The paths are the ones of the **entity**, and each property on the way must be a navigation property (a related
object or a collection). The rest is `400 Bad Request`, with all the wrong paths reported at once:

| Request | Error of `include` |
|---|---|
| `GET /v1/products?include=colour` | `Unable to include 'colour': the type 'Product' doesn't have the property 'colour'.` |
| `GET /v1/products?include=name` | `Unable to include 'name': the property 'Name' isn't a navigation property.` |
| `GET /v1/products?include=category.products` | `Unable to include 'category.products': the property 'Products' is the inverse of the property 'Category', so the path makes a cycle.` |
| `GET /v1/categories?include=products.photos.product` (with `MaxInclusionDepth = 2`) | `Unable to include 'products.photos.product': the property path has more than 2 properties.` |

A path can’t go back by the inverse property: the category of a product is already the one its products refer to.
And an application may limit the length of the paths (the `MaxInclusionDepth`
[option](../customization/options.md)); there is no limit by default.

## Inclusion and the DTOs

Whether an included object appears in the response depends on how the DTO is converted from its model: the DTOs
usually leave out the references back (the products of an included category come without the category, see
[Models](../building-blocks/models.md)). That’s why `"products": null` and `"product": null` in the example above.

## See also

- [Filtering](filtering.md), [Sorting](sorting.md), [Paging](paging.md)
- [Performance](../recipes/performance.md): the cost of the inclusions
