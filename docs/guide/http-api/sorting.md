# Sorting

A list is sorted by the `sort` query parameter: the comma-separated property paths, each one prefixed with `-` to
sort in the descending order.

```http
# The most expensive products first, the ones of the same price by name
GET /v1/products?sort=-price,name
```

The first path sorts the list, the next ones sort the objects that are equal by the previous ones. The paths may also
be passed by repeating the parameter: `sort=-price&sort=name` is the same as `sort=-price,name`. The names are
case-insensitive.

## Related objects

A path may go through the related objects:

```http
# By the name of the category, then the most expensive first
GET /v1/products?sort=category.name,-price
```

## What can be sorted by

The paths are the ones of the **entity** (not of the DTO or the filter), and each one must lead to a scalar value: a
number, a string, a date, and so on. The rest is `400 Bad Request`, with all the wrong paths reported at once:

| Request | Error of `sort` |
|---|---|
| `sort=colour` | `Unable to sort by 'colour': the type 'Product' doesn't have the property 'colour'.` |
| `sort=photos` | `Unable to sort by 'photos': the property 'Photos' is a collection.` |
| `sort=category` | `Unable to sort by 'category': the property 'Category' isn't a scalar value.` |

## Without sorting

Without `sort`, the objects come in the order the database returns them, which isn’t defined: it may differ from
request to request. When the lists are [paged](paging.md), always sort them, and sort by something unique in the end
(e.g. `sort=-price,id`), so that the pages don’t overlap.

## See also

- [Filtering](filtering.md), [Paging](paging.md), [Inclusion](inclusion.md)
- [Errors](errors.md)
