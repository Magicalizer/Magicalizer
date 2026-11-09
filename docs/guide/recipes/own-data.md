# Users see only their own data

A notes application: each user has notebooks with notes, and sees, edits, and deletes only their own. The client never
says whose a notebook is, and can’t reach the notebooks of others even by their identifiers. The code is in
`docs/samples/Recipes`.

The idea:

1. the entity stores its owner;
2. [query prefilters](../customization/query-prefilters.md) restrict every query to the current user;
3. a [custom service](../customization/custom-services.md) sets the owner of the new and the edited objects;
4. a model validator checks that the objects a new one refers to are the user’s too.

## The owner

The notebook stores the identifier of its user. The model has it too, so the service can set it, but the DTO and the
filter don’t: the clients neither see nor send it, and can’t filter by it.

[!code-csharp[](../../samples/Recipes/Data/Entities/Notebook.cs)]

[!code-csharp[](../../samples/Recipes/Filters/NotebookFilter.cs)]

A note doesn’t store its owner: it belongs to the user of its notebook.

## The current user

The domain layer doesn’t depend on HTTP, so it gets the user through an interface of its own:

[!code-csharp[](../../samples/Recipes/Domain/ICurrentUser.cs)]

The web application implements it with the user of the request:

[!code-csharp[](../../samples/Recipes/Infrastructure/HttpCurrentUser.cs)]

A background job or a test registers its own implementation (e.g. a fixed user, or none).

## The prefilters

One prefilter for each entity the clients reach directly:

[!code-csharp[](../../samples/Recipes/Data/Prefilters/OwnNotebookPrefilter.cs)]

[!code-csharp[](../../samples/Recipes/Data/Prefilters/OwnNotePrefilter.cs)]

[!code-csharp[](../../samples/Recipes/Program.cs#OwnData)]

Now a notebook of another user behaves as if it doesn’t exist: it isn’t in the lists, and getting, replacing,
patching, or deleting it is `404 Not Found`. The DTOs are marked with `[AuthenticatedOnly]`, so an anonymous request is
`401 Unauthorized` before it reaches the prefilters.

## Setting the owner

The prefilters restrict the existing objects, but a new one gets its owner from the service:

[!code-csharp[](../../samples/Recipes/Domain/Services/NotebookService.cs)]

`EditAsync` sets the owner too: the DTO has no owner, so the model that comes from `PUT` or `PATCH` has none, and the
entity would lose it. Setting it to the current user is safe, as the prefilter has already checked that the notebook is
the user’s (otherwise it’s `404 Not Found`).

## The references

A note refers to a notebook, and the client sends its identifier. Without a check, a user could put a note into the
notebook of another user. The model validator checks it through the notebook service, which is prefiltered too:

[!code-csharp[](../../samples/Recipes/Domain/Models/Validators/NoteValidator.cs)]

```http
POST /v1/notes
X-User: bob
Content-Type: application/json

{"notebook": {"id": 1}, "title": "Spam"}
```

```json
{
  "status": 400,
  "errors": {
    "Notebook.Id": ["The notebook doesn't exist."]
  }
}
```

The validator runs in the service, for the creation and the editing alike, so a note can’t be moved to a foreign
notebook either.

## The related objects

> [!WARNING]
> A prefilter restricts the queries of its own entity: the lists, the counts, and the objects by the primary key. It
> doesn’t restrict the related objects of another entity: the [included](../http-api/inclusion.md) ones and the
> conditions of the [related objects](../http-api/filtering.md#related-objects).

Here it’s enough: each entity the clients reach is prefiltered, and everything related to an own notebook or note is
the user’s too. It isn’t enough when a shared entity refers to the private ones, e.g. a public `Tag` with the notes of
all the users: `GET /v1/tags?include=notes` would return the notes of everyone. Then:

- don’t give the shared entity the navigation property to the private ones (or don’t allow including it); or
- restrict the private entity in EF Core itself, with a global query filter, which applies to every query, including
  the inclusions:

```csharp
public class AppDbContext : DbContext
{
  private readonly ICurrentUser currentUser;

  public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser) : base(options)
  {
    this.currentUser = currentUser;
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Note>(etb => {
      // The filter reads the user of this context instance, so each request gets its own.
      etb.HasQueryFilter("Owner", e => e.Notebook!.UserId == this.currentUser.Id);
    });
  }
}
```

The [soft deletion](soft-deletion.md) recipe uses a global query filter the same way.

## Several tenants

A multi-tenant application is the same recipe one level up: the entities store the tenant instead of (or besides) the
user, `ICurrentUser` becomes `ICurrentTenant` (from a claim, a header, or the host name), and the prefilters or the
global query filters compare the tenant. When most entities belong to a tenant, the global query filters are usually
simpler, as one convention in the `DbContext` covers all of them, the related objects included.

## Tests

The `Recipes.Tests` project checks the recipe through HTTP (see [Testing](testing.md)):

[!code-csharp[](../../samples/Recipes.Tests/NotebookTests.cs#L26-L43)]

## See also

- [Query prefilters](../customization/query-prefilters.md)
- [Authorization](../customization/authorization.md)
- [Global query filters in EF Core](https://learn.microsoft.com/ef/core/querying/filters)
