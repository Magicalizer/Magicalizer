# A layered solution

[Your first API](your-first-api.md) keeps all the classes in one project, in the folders of their layers. In a larger
application, the layers become projects of their own, and each project explicitly references only its own layer (and
the projects it builds on), so, for example, the models can’t reach HTTP. This page splits the same catalog into such projects. The code is in
`docs/samples/Layered`.

## The projects

| Project | Contents | References |
|---|---|---|
| `App.Data.Entities` | The entities and the `DbContext` | `Magicalizer.Data.Entities.Abstractions`, `Microsoft.EntityFrameworkCore.Relational` |
| `App.Filters` | The filters | `Magicalizer.Filters.Abstractions` |
| `App.Domain.Models` | The models | `Magicalizer.Domain.Models.Abstractions`, `App.Data.Entities`, `App.Filters` |
| `App.Api.Dto` | The DTOs and their validators | `Magicalizer.Api.Dto.Abstractions`, `Magicalizer.Validators.Abstractions`, `FluentValidation`, `App.Domain.Models` |
| `App` | The web application | `Magicalizer`, `Microsoft.EntityFrameworkCore.Sqlite`, `App.Api.Dto` |

```text
Layered/
  App/                     # Program.cs, appsettings.json
  App.Api.Dto/             # Category.cs, Photo.cs, Product.cs, Validators/
  App.Domain.Models/       # Category.cs, Photo.cs, Product.cs
  App.Data.Entities/       # AppDbContext.cs, Category.cs, Photo.cs, Product.cs
  App.Filters/             # CategoryFilter.cs, PhotoFilter.cs, ProductFilter.cs
  Layered.slnx
```

The classes are the ones of the tutorial, unchanged: only the projects they live in are different. Each project
references only the abstractions of its layer (see [Packages and layers](../introduction/packages-and-layers.md)), and
only the web application references `Magicalizer` itself. The web application references `App.Api.Dto` and gets the
other projects through it.

Create them with the .NET CLI, e.g.:

```console
dotnet new classlib -o App.Api.Dto
dotnet add App.Api.Dto package Magicalizer.Api.Dto.Abstractions
dotnet add App.Api.Dto package Magicalizer.Validators.Abstractions
dotnet add App.Api.Dto package FluentValidation
dotnet add App.Api.Dto reference App.Domain.Models
```

The validators of the models (if you have any) go into `App.Domain.Models`, which then references
`Magicalizer.Validators.Abstractions` and `FluentValidation` too. The custom services and the query prefilters may go
into a project of their own, e.g. `App.Domain.Services`, which references `Magicalizer`.

## Loading the assemblies

Magicalizer looks for the classes in the assemblies that are loaded when `AddMagicalizer` is called (see
[Type discovery](../building-blocks/type-discovery.md#which-assemblies)). .NET loads an assembly when its types are
used first, and `Program.cs` of a layered solution uses none of the DTOs, models, or filters. So without more, nothing
is found:

```text
info: Magicalizer[0]
      0 controllers are created for the magicalized DTOs.
```

And all the requests are `404 Not Found`. Load the assemblies of the layers before `AddMagicalizer`:

[!code-csharp[](../../samples/Layered/App/Program.cs#LoadAssemblies)]

`Assembly.Load` takes the names of the assemblies (the names of the projects, unless you changed them), which the web
application has in its output folder through the project references. The validators are found the same way, so the
assembly of the DTO validators (and of the model validators, if they live elsewhere) must be in the list too.

Now the log of the start shows the API:

```text
info: Magicalizer[0]
      The magicalized DTO Category is served by MagicalizedController<Category> at /v1/categories: GET, POST, PUT, PATCH, DELETE.
info: Magicalizer[0]
      The magicalized DTO Photo is served by MagicalizedController<Photo> at /v1/photos: GET, POST, PUT, PATCH, DELETE.
info: Magicalizer[0]
      The magicalized DTO Product is served by MagicalizedController<Product> at /v1/products: GET, POST, PUT, PATCH, DELETE.
info: Magicalizer[0]
      3 controllers are created for the magicalized DTOs.
```

> [!NOTE]
> This is a temporary step: a future version of Magicalizer will find the assemblies of the solution by itself.

## The rest

Everything else is as in the tutorial: the same `Program.cs` (with the step above), the same API, and the same
requests. Run it from the folder of the web application:

```console
dotnet run --project App
```

## See also

- [Packages and layers](../introduction/packages-and-layers.md)
- [Type discovery](../building-blocks/type-discovery.md)
- [HTTP methods](../customization/http-methods.md#the-name-of-httpmethod): the DTO project doesn’t need the alias of
  `HttpMethod`, as it isn’t a web project
