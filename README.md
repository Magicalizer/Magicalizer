# Magicalizer

![Magicalizer logotype](https://magicalizer.net/magicalizer_github_icon.png)

## Introduction

Magicalizer is a small and lightweight library that allows you to create a fully featured REST API ASP.NET Core web
application with minimal routine code.

In most cases, all you need to write are the DTO, model, and entity classes, the filters, and the validators. That’s it.
Your web application gets the standard REST API for each DTO, with complex filtering, sorting, paging, and inclusion of
related objects; validation at both the DTO and the model levels; authorization; and the OpenAPI description.

While Magicalizer handles the heavy lifting, you can still replace any default implementation with your own. Once you
create a custom service for a model, it replaces the default one, and the same applies to the controllers. It doesn’t
matter where you place your classes: they are discovered automatically.

## Features

* **Standard REST API** for each DTO marked with `[Magicalized]`: GET (a list and by the primary key), POST, PUT,
  PATCH (JSON Patch), and DELETE. The supported HTTP methods can be limited for each DTO.
* **Filtering** by the properties of the object, of the related objects (e.g. `category.name.equals=Pizza`), and of the
  elements of the collections (`photos.any.filename.endswith=.jpg`), with the filters of numbers, strings, booleans,
  dates and times, GUIDs, and enums.
* **Sorting** by several properties, including the ones of the related objects, in both directions.
* **Paging** with the total number of the matching objects in the response headers.
* **Inclusion** of the related objects and collections, including the nested ones (e.g. `products.photos`).
* **Limits** of the number of the objects a request gets and of the depth of the inclusions.
* **Strict query validation**: a misspelled parameter or condition, an invalid value, sorting, or inclusion is a 400 Bad
  Request with all the errors of the request at once.
* **Validation** of the DTOs (in the API) and the models (in the services) by FluentValidation, with the separate rule
  sets for creation and editing.
* **Errors** as problem details: 400 for the invalid requests and the references to the objects that don’t exist, 404
  for the missing objects, 409 for the duplicate keys and the objects that can’t be deleted as others refer to them.
* **Authorization** for each DTO and HTTP method: an authenticated user or the authorization policies.
* **Query prefilters** that restrict the data (e.g. show the users only their own objects) for all the requests,
  including the ones by the primary key, updates, and deletions.
* **OpenAPI** description of the API with the filter conditions as they are passed, and the settings of the API
  description (hiding an API or putting it into a separate document).
* **Customization**: custom services and controllers, derived from the standard ones, replace them automatically.
* **Startup diagnostics**: the served DTOs and the misconfigured types (e.g. a DTO without a model or an authorization
  policy that isn’t registered) are logged when the app starts.

## Packages

The library is split by the layers, so each project depends only on the abstractions of its layer:

| Package | Contents | Used by |
|---|---|---|
| `Magicalizer` | The controllers, services, and the rest of the implementation | The web application |
| `Magicalizer.Api.Dto.Abstractions` | `IDto`, `[Magicalized]`, and the authorization attributes | DTOs |
| `Magicalizer.Domain.Models.Abstractions` | `IModel` | Models |
| `Magicalizer.Domain.Services.Abstractions` | `IService`, `IQueryPrefilter`, the exceptions of the services | Services and their consumers |
| `Magicalizer.Data.Entities.Abstractions` | `IEntity` | Entities |
| `Magicalizer.Filters.Abstractions` | `IFilter` and the filters of the values and collections | Filters |
| `Magicalizer.Validators.Abstractions` | `RuleSetName` | Validators |

## Quick Start

To start using Magicalizer:

* Create an empty web application.
* Add a dependency on Magicalizer.
* Add a dependency on Entity Framework Core for your database and configure it.
* Call `AddMagicalizer` and `UseMagicalizer` in the `Program` class.

```csharp
using App.Data.Entities;
using Magicalizer.Extensions;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DbContext, AppDbContext>(options => {
  options.UseSqlite(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddMagicalizer();

WebApplication webApplication = builder.Build();

webApplication.UseMagicalizer();
webApplication.Run();
```

Create the entities. The type arguments of `IEntity` are the types of the primary key properties, in the order of the
primary key of the database context.

```csharp
public class Category : IEntity<int>
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public virtual ICollection<Product>? Products { get; set; }
}
```

```csharp
public class Product : IEntity<int>
{
  public int Id { get; set; }
  public int CategoryId { get; set; }
  public string? Name { get; set; }
  public decimal Price { get; set; }
  public DateTime Created { get; set; }
  public virtual Category? Category { get; set; }
  public virtual ICollection<Photo>? Photos { get; set; }
}
```

```csharp
public class Photo : IEntity<int>
{
  public int Id { get; set; }
  public int ProductId { get; set; }
  public string? Filename { get; set; }
  public virtual Product? Product { get; set; }
}
```

Configure the database context as you would normally do:

```csharp
public class AppDbContext : DbContext
{
  public DbSet<Category> Categories { get; set; }
  public DbSet<Product> Products { get; set; }
  public DbSet<Photo> Photos { get; set; }

  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Category>(etb => {
      etb.HasKey(e => e.Id);
      etb.ToTable("Categories");
    });

    modelBuilder.Entity<Product>(etb => {
      etb.HasKey(e => e.Id);
      etb.ToTable("Products");
    });

    modelBuilder.Entity<Photo>(etb => {
      etb.HasKey(e => e.Id);
      etb.ToTable("Photos");
    });

    base.OnModelCreating(modelBuilder);
  }
}
```

Create the models. A model is created from its entity by the constructor that takes the entity, and it’s converted to
the entity by `ToEntity`. The related models leave out the references back: the related entities refer to each other
(e.g. the products of an included category refer to it), so an endless conversion would overflow the stack.

```csharp
public class Category : IModel<Data.Entities.Category, Filters.CategoryFilter>
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public IEnumerable<Product>? Products { get; set; }

  public Category() { }

  public Category(Data.Entities.Category categoryEntity) : this(categoryEntity, ignoreProducts: false) { }

  public Category(Data.Entities.Category categoryEntity, bool ignoreProducts)
  {
    this.Id = categoryEntity.Id;
    this.Name = categoryEntity.Name;

    if (!ignoreProducts)
      this.Products = categoryEntity.Products?.Select(p => new Product(p, ignoreCategory: true, ignorePhotos: false)).ToList();
  }

  public Data.Entities.Category ToEntity()
  {
    return new Data.Entities.Category {
      Id = this.Id,
      Name = this.Name
    };
  }
}
```

```csharp
public class Product : IModel<Data.Entities.Product, Filters.ProductFilter>
{
  public int Id { get; set; }
  public Category? Category { get; set; }
  public string? Name { get; set; }
  public decimal Price { get; set; }
  public DateTime Created { get; set; }
  public IEnumerable<Photo>? Photos { get; set; }

  public Product() { }

  public Product(Data.Entities.Product productEntity) : this(productEntity, ignoreCategory: false, ignorePhotos: false) { }

  public Product(Data.Entities.Product productEntity, bool ignoreCategory, bool ignorePhotos)
  {
    this.Id = productEntity.Id;

    if (!ignoreCategory)
      this.Category = productEntity.Category == null ? new Category { Id = productEntity.CategoryId } : new Category(productEntity.Category, ignoreProducts: true);

    this.Name = productEntity.Name;
    this.Price = productEntity.Price;
    this.Created = productEntity.Created;

    if (!ignorePhotos)
      this.Photos = productEntity.Photos?.Select(p => new Photo(p, ignoreProduct: true)).ToList();
  }

  public Data.Entities.Product ToEntity()
  {
    return new Data.Entities.Product {
      Id = this.Id,
      CategoryId = this.Category?.Id ?? 0,
      Name = this.Name,
      Price = this.Price,
      Created = this.Created == default ? DateTime.UtcNow : this.Created
    };
  }
}
```

```csharp
public class Photo : IModel<Data.Entities.Photo, Filters.PhotoFilter>
{
  public int Id { get; set; }
  public Product? Product { get; set; }
  public string? Filename { get; set; }

  public Photo() { }

  public Photo(Data.Entities.Photo photoEntity) : this(photoEntity, ignoreProduct: false) { }

  public Photo(Data.Entities.Photo photoEntity, bool ignoreProduct)
  {
    this.Id = photoEntity.Id;

    if (!ignoreProduct)
      this.Product = photoEntity.Product == null ? new Product { Id = photoEntity.ProductId } : new Product(photoEntity.Product, ignoreCategory: false, ignorePhotos: true);

    this.Filename = photoEntity.Filename;
  }

  public Data.Entities.Photo ToEntity()
  {
    return new Data.Entities.Photo {
      Id = this.Id,
      ProductId = this.Product?.Id ?? 0,
      Filename = this.Filename
    };
  }
}
```

Create the DTOs. `[Magicalized]` sets the route of the API, a DTO is created from its model by the constructor that
takes the model, and it’s converted to the model by `ToModel`. The related DTOs leave out the references back too.

```csharp
[Magicalized("v1/categories")]
public class Category : IDto<Domain.Models.Category>
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public IEnumerable<Product>? Products { get; set; }

  public Category() { }

  public Category(Domain.Models.Category categoryModel) : this(categoryModel, ignoreProducts: false) { }

  public Category(Domain.Models.Category categoryModel, bool ignoreProducts)
  {
    this.Id = categoryModel.Id;
    this.Name = categoryModel.Name;

    if (!ignoreProducts)
      this.Products = categoryModel.Products?.Select(p => new Product(p, ignoreCategory: true, ignorePhotos: false)).ToList();
  }

  public Domain.Models.Category ToModel()
  {
    return new Domain.Models.Category {
      Id = this.Id,
      Name = this.Name
    };
  }
}
```

```csharp
[Magicalized("v1/products")]
public class Product : IDto<Domain.Models.Product>
{
  public int Id { get; set; }
  public Category? Category { get; set; }
  public string? Name { get; set; }
  public decimal Price { get; set; }
  public DateTime Created { get; set; }
  public IEnumerable<Photo>? Photos { get; set; }

  public Product() { }

  public Product(Domain.Models.Product productModel) : this(productModel, ignoreCategory: false, ignorePhotos: false) { }

  public Product(Domain.Models.Product productModel, bool ignoreCategory, bool ignorePhotos)
  {
    this.Id = productModel.Id;

    if (!ignoreCategory)
      this.Category = productModel.Category == null ? null : new Category(productModel.Category, ignoreProducts: true);

    this.Name = productModel.Name;
    this.Price = productModel.Price;
    this.Created = productModel.Created;

    if (!ignorePhotos)
      this.Photos = productModel.Photos?.Select(p => new Photo(p, ignoreProduct: true)).ToList();
  }

  public Domain.Models.Product ToModel()
  {
    return new Domain.Models.Product {
      Id = this.Id,
      Category = this.Category?.ToModel(),
      Name = this.Name,
      Price = this.Price,
      Created = this.Created
    };
  }
}
```

```csharp
[Magicalized("v1/photos")]
public class Photo : IDto<Domain.Models.Photo>
{
  public int Id { get; set; }
  public Product? Product { get; set; }
  public string? Filename { get; set; }

  public Photo() { }

  public Photo(Domain.Models.Photo photoModel) : this(photoModel, ignoreProduct: false) { }

  public Photo(Domain.Models.Photo photoModel, bool ignoreProduct)
  {
    this.Id = photoModel.Id;

    if (!ignoreProduct)
      this.Product = photoModel.Product == null ? null : new Product(photoModel.Product, ignoreCategory: false, ignorePhotos: true);

    this.Filename = photoModel.Filename;
  }

  public Domain.Models.Photo ToModel()
  {
    return new Domain.Models.Photo {
      Id = this.Id,
      Product = this.Product?.ToModel(),
      Filename = this.Filename
    };
  }
}
```

Create the validators of the DTOs (the validators of the models are created the same way and run in the services).
The rules without a rule set are checked always, the ones of `RuleSetName.Create` and `RuleSetName.Edit` are checked on
creation and editing.

```csharp
public class CategoryValidator : AbstractValidator<Category>
{
  public CategoryValidator()
  {
    this.RuleFor(c => c.Name).NotEmpty().MaximumLength(64);
    this.RuleSet(RuleSetName.Edit, () => {
      this.RuleFor(c => c.Id).NotEmpty();
    });
  }
}
```

```csharp
public class ProductValidator : AbstractValidator<Product>
{
  public ProductValidator()
  {
    this.RuleFor(p => p.Category).NotNull().DependentRules(() => {
      this.RuleFor(p => p.Category!.Id).GreaterThan(0);
    });

    this.RuleFor(p => p.Name).NotEmpty().MaximumLength(64);
    this.RuleFor(p => p.Price).GreaterThan(0m);
    this.RuleSet(RuleSetName.Edit, () => {
      this.RuleFor(p => p.Id).NotEmpty();
    });
  }
}
```

```csharp
public class PhotoValidator : AbstractValidator<Photo>
{
  public PhotoValidator()
  {
    this.RuleFor(p => p.Product).NotNull().DependentRules(() => {
      this.RuleFor(p => p.Product!.Id).GreaterThan(0);
    });

    this.RuleFor(p => p.Filename).NotEmpty().MaximumLength(64);
  }
}
```

Finally, create the filters. A property of a filter is a filter of the property of the entity with the same name: a
value filter (e.g. `StringFilter`), the filter of a related entity, or `EnumerableFilter` of a collection.

```csharp
public class CategoryFilter : IFilter
{
  public IntegerFilter? Id { get; set; }
  public StringFilter? Name { get; set; }
  public EnumerableFilter<ProductFilter>? Products { get; set; }
}
```

```csharp
public class ProductFilter : IFilter
{
  public IntegerFilter? Id { get; set; }
  public CategoryFilter? Category { get; set; }
  public StringFilter? Name { get; set; }
  public DecimalFilter? Price { get; set; }
  public DateTimeFilter? Created { get; set; }
  public EnumerableFilter<PhotoFilter>? Photos { get; set; }
}
```

```csharp
public class PhotoFilter : IFilter
{
  public IntegerFilter? Id { get; set; }
  public ProductFilter? Product { get; set; }
  public StringFilter? Filename { get; set; }
}
```

Run the web application and try the requests (you can use the database of the sample project):

```console
GET /v1/categories
GET /v1/categories?name.contains=izz
GET /v1/categories/1?include=products.photos
GET /v1/products?id.in=1&id.in=2
GET /v1/products?category.id.equals=5&sort=-price,name&offset=0&limit=5
GET /v1/products?category.name.equals=Pizza&include=category
GET /v1/products?photos.isempty=true
GET /v1/products?photos.none.filename.endswith=.png
GET /v1/categories?products.any.price.from=100&products.any.name.contains=izz
GET /v1/categories?products.any[0].price.from=100&products.any[1].name.contains=izz
```

The last two requests differ: in the first one, the same product must cost from 100 and have `izz` in its name, in
the second one, these may be different products.

```console
POST /v1/categories
{"name": "Sushi"}
```

```console
PUT /v1/categories
{"id": 1, "name": "Not sushi"}
```

```console
PATCH /v1/categories/1
[{"op": "replace", "path": "/name", "value": "Sushi again"}]
```

```console
DELETE /v1/categories/1
```

## Querying

### Filtering

A filter condition is a query parameter with the property path and the condition (e.g. `name.contains=izz`), the
names are case-insensitive. All the conditions of a request must match.

| Filter | Conditions |
|---|---|
| `ByteFilter`, `ShortFilter`, `IntegerFilter`, `LongFilter`, `FloatFilter`, `DoubleFilter`, `DecimalFilter`, `DateTimeFilter`, `DateTimeOffsetFilter` | `isnull`, `equals`, `notequals`, `from`, `to`, `in`, `notin` |
| `StringFilter` | `isnull`, `equals`, `notequals`, `contains`, `startswith`, `endswith`, `in`, `notin` |
| `GuidFilter`, `EnumFilter<TEnum>` | `isnull`, `equals`, `notequals`, `in`, `notin` |
| `BooleanFilter` | `isnull`, `equals` |
| `EnumerableFilter<TFilter>` | `isempty`, `any`, `none` |

* `isnull=true` matches the null values, `isnull=false` the non-null ones (`isempty` works the same way).
* `from` and `to` include the values themselves.
* The values of `in` and `notin` are passed by repeating the parameter: `id.in=1&id.in=2`.
* The enums are passed by their names or numbers: `diet.equals=vegan`.
* The strings are compared as the database compares them (its collation decides, e.g., whether the case is ignored).
* The conditions of the collection elements are passed in a single form, where one element must match all of them
  (`products.any.price.from=100&products.any.name.contains=pizza`), or in a list form, where each filter is matched by
  any element (`products.any[0].price.from=100&products.any[1].name.contains=pizza`).

### Sorting

`sort` takes comma-separated property paths, a path prefixed with `-` is sorted in the descending order:
`sort=-price,category.name`.

### Paging

`offset` skips the objects and `limit` takes the maximum number of them: `offset=20&limit=10`. The response has the
`Paging-Total-Count` header with the number of all the matching objects, and the `Paging-Offset` and `Paging-Limit`
headers with the applied values.

The number of all the matching objects takes a separate database query, so a client that doesn’t need it skips it with
the `Prefer: count=none` header (RFC 7240). If the count is off by default (see [Options](#options)), a client gets it
with the `Prefer: count=exact` header. The response reports the applied preference in the `Preference-Applied` header
and has the `Vary: Prefer` header, so the caches keep the responses apart. A browser client gets these headers if they
are exposed by CORS (`WithExposedHeaders("Paging-Total-Count", "Paging-Offset", "Paging-Limit", "Preference-Applied")`).

### Inclusion

`include` takes comma-separated property paths of the related objects and collections: `include=category,photos`. The
nested ones are included by the longer paths: `include=products.photos`. A path that leads back by the inverse property
(e.g. `category.products` of a product) isn’t allowed.

## Options

```csharp
builder.Services.AddMagicalizer(options => {
  // The maximum number of objects a request gets, the default limit too.
  options.MaxLimit = 100;

  // The maximum number of properties in a property path of an inclusion.
  options.MaxInclusionDepth = 2;

  // The query parameters something else processes (e.g. the request localization).
  options.IgnoredQueryParameters.Add("culture");

  // Whether the lists are returned with the total count unless a request asks otherwise (on by default).
  options.CountTotalByDefault = false;
});
```

The limits aren’t set by default. They apply to the API only, the services return what they are asked for.

## Customization

### HTTP methods and authorization

```csharp
[Magicalized("v1/notes", HttpMethod.Get, HttpMethod.Post)]
[AuthenticatedOnly(HttpMethod.Get)]
[AuthorizedOnly("Notes.Edit", HttpMethod.Post)]
public class Note : IDto<Domain.Models.Note>
```

`IgnoreApi` hides the API from the OpenAPI description, and `GroupName` puts it into a separate document:
`[Magicalized("v1/notes", GroupName = "internal")]`.

### Services

A service of a model is available by its interface:

```csharp
public ProductsReport(IService<int, Product, ProductFilter> productService)
```

A custom service derived from the standard one replaces it automatically. It’s also available by a custom interface
derived from `IService`, if the service implements one.

```csharp
public class ProductService : Service<int, Data.Entities.Product, Product, ProductFilter>
{
  public ProductService(DbContext dbContext, IEnumerable<IQueryPrefilter<Data.Entities.Product, ProductFilter>>? queryPrefilters = null, IValidator<Product>? validator = null) : base(dbContext, queryPrefilters, validator) { }

  public override Task<Product> CreateAsync(Product model, CancellationToken cancellationToken = default)
  {
    model.Created = DateTime.UtcNow;
    return base.CreateAsync(model, cancellationToken);
  }
}
```

A filter property marked with `[IgnoreFilter]` isn’t applied to the queries, so a custom service can process it (e.g.
convert a search text to the conditions).

### Query prefilters

A query prefilter restricts the data of an entity for all the requests (the lists, the ones by the primary key, updates,
and deletions), e.g. by the current user. It’s registered in the services:
`builder.Services.AddScoped<IQueryPrefilter<Data.Entities.Note, NoteFilter>, OwnNotePrefilter>();` (this one also needs
`builder.Services.AddHttpContextAccessor();`).

```csharp
public class OwnNotePrefilter : IQueryPrefilter<Data.Entities.Note, NoteFilter>
{
  private readonly IHttpContextAccessor httpContextAccessor;

  public OwnNotePrefilter(IHttpContextAccessor httpContextAccessor)
  {
    this.httpContextAccessor = httpContextAccessor;
  }

  public IQueryable<Data.Entities.Note> Apply(IQueryable<Data.Entities.Note> query, NoteFilter? filter)
  {
    // The users only see their own notes.
    string? userId = this.httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    return query.Where(n => n.UserId == userId);
  }
}
```

### Controllers

A custom controller derived from the standard one replaces it automatically and gets the route of the DTO; the standard
actions can be overridden, and the custom ones added:

```csharp
public class PhotosController : MagicalizedController<int, Photo, Api.Dto.Photo, PhotoFilter>
{
  public PhotosController(IService<int, Photo, PhotoFilter> service) : base(service) { }

  [HttpGet("count")]
  public Task<int> CountAsync(CancellationToken cancellationToken = default)
  {
    return this.service.CountAsync(cancellationToken: cancellationToken);
  }
}
```

## Samples

* [Sample Magicalizer-based web application](https://github.com/Magicalizer/Magicalizer-Sample)

## Links

Author: https://sikorsky.pro/

## License

Licensed under the [Apache License 2.0](LICENSE.txt), see also [NOTICE.txt](NOTICE.txt).
