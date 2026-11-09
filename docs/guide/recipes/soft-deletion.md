# Soft deletion

`DELETE` of a soft-deleted object doesn’t remove its row: it marks the object as deleted, and from then on the object
is hidden as if it were removed. It can be restored, audited, or purged later. The notes of `docs/samples/Recipes` are
deleted this way.

Two parts:

1. a global query filter of EF Core hides the deleted objects from every query;
2. a [custom service](../customization/custom-services.md) marks the object instead of removing it.

## Hiding the deleted objects

The entity has the flag:

[!code-csharp[](../../samples/Recipes/Data/Entities/Note.cs)]

And the `DbContext` hides the flagged ones with a named global query filter (the names are new in EF Core 10, they let
a query switch off one filter and keep the others):

[!code-csharp[](../../samples/Recipes/Data/Entities/AppDbContext.cs#SoftDeletion)]

The flag isn’t in the model, the DTO, or the filter: the clients don’t see it and can’t ask for the deleted notes.

Why a global query filter and not a [query prefilter](../customization/query-prefilters.md)? The global filter is
applied by EF Core to every query of the entity, including the [inclusions](../http-api/inclusion.md) and the
conditions of the [related objects](../http-api/filtering.md#related-objects), while a prefilter restricts only the
queries of its own service. With a prefilter, `GET /v1/notebooks/1?include=notes` would still return the deleted notes.

## Marking instead of removing

The service overrides `DeleteAsync`:

[!code-csharp[](../../samples/Recipes/Domain/Services/NoteService.cs#DeleteAsync)]

It first gets the note the usual way, so a note that is already deleted, or isn’t available through the prefilters
(e.g. it’s [another user’s](own-data.md)), is `404 Not Found`, as for the default deletion. Then `ExecuteUpdateAsync`
sets the flag with one `UPDATE`, without loading the entity.

The API is unchanged:

```http
DELETE /v1/notes/2
```

```http
HTTP/1.1 204 No Content
```

```http
GET /v1/notebooks/1?include=notes
```

```json
{
  "id": 1,
  "name": "Ideas",
  "notes": [
    { "id": 1, "notebook": null, "title": "Buy milk", "text": null }
  ]
}
```

A second `DELETE /v1/notes/2` is `404 Not Found`, and `GET /v1/notes/2`, `PUT`, and `PATCH` of it are too: the global
query filter applies to the checks of the service as well.

## Restoring and purging

Your own code reaches the deleted objects by switching off the filter by its name, e.g. in a method of the service:

```csharp
public async Task RestoreAsync(int id, CancellationToken cancellationToken = default)
{
  await this.dbContext.Set<Data.Entities.Note>()
    .IgnoreQueryFilters([AppDbContext.SoftDeletionFilter])
    .Where(n => n.Id == id && n.IsDeleted)
    .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsDeleted, false), cancellationToken);
}
```

The other filters (e.g. the owner of a multi-tenant application) stay in force. A background job purges the old
deleted rows the same way, with `ExecuteDeleteAsync`.

## Things to keep in mind

- The deleted rows are still in the table: the unique indexes and the foreign keys see them. E.g. a unique name can’t
  be reused while the deleted object with it exists, and deleting a notebook removes its notes, the deleted ones
  included (the relationship is required, so EF Core cascades the deletion).
- If some users (e.g. the administrators) must see the deleted objects, a prefilter that depends on the user may fit
  better than a global filter, as in [Query prefilters](../customization/query-prefilters.md), with its limits on the
  related objects.
- Index the flag (or use a filtered index) if the table is large and the deleted rows are many.

## See also

- [Custom services](../customization/custom-services.md)
- [Global query filters in EF Core](https://learn.microsoft.com/ef/core/querying/filters)
- [ExecuteUpdate and ExecuteDelete](https://learn.microsoft.com/ef/core/saving/execute-insert-update-delete)
