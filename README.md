# Magicalizer 5.0.0

![Magicalizer logotype](https://magicalizer.net/magicalizer_github_icon.png)

## Introduction

Magicalizer is a small, super simple, and lightweight library that allows you to create
a fully featured REST API ASP.NET Core web application with minimal routine code.

In most cases, all you need to write are DTO/model/entity classes, filters, and validators for them.
That’s it. Your web application will automatically support complex filtering
(including multiple nested object properties, ranges, etc.), complex sorting
(multiple nested object properties with different directions), pagination, complex inclusions
(nested object properties), flexible and powerful validation at both the DTO and model levels
(using FluentValidation), and policy-based authorization for GET, POST, PUT, PATCH,
and DELETE requests (yes, PATCH requests are supported too).

One important point: While Magicalizer handles the heavy lifting, you can still replace
any default implementation with your own when needed. Once you create
a custom service implementation for any model, it will automatically replace the default one.
The same applies to controllers. It doesn’t matter where you place your DTO/model/entity classes,
validators, services, or controllers—they will be discovered and resolved automatically.
This makes it incredibly easy to reuse code and create decoupled software.

## Quick Start

To start using Magicalizer:

* Create an empty web application.
* Add a dependency on Magicalizer.
* Add a dependency on EntityFramework for your preferred database and configure it.
* Call the `AddMagicalizer` and `UseMagicalizer` methods in the `Program` class.

````csharp
using App;
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
````

Now create the entity classes.

````csharp
public class Category : IEntity<int>
{
  public int Id { get; set; }
  public string? Name { get; set; }

  public virtual ICollection<Product>? Products { get; set; }
}
````

````csharp
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
````

````csharp
public class Photo : IEntity<int>
{
  public int Id { get; set; }
  public int ProductId { get; set; }
  public string? Filename { get; set; }

  public virtual Product? Product { get; set; }
}
````

Configure a `DbContext` as you would normally do:

````csharp
public class AppDbContext : DbContext
{
  DbSet<Category> Categories { get; set; }
  DbSet<Product> Products { get; set; }
  DbSet<Photo> Photos { get; set; }

  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Category>(etb =>
      {
        etb.HasKey(e => e.Id);
        etb.Property(e => e.Id);
        etb.ToTable("Categories");
      }
    );

    modelBuilder.Entity<Product>(etb =>
      {
        etb.HasKey(e => e.Id);
        etb.Property(e => e.Id);
        etb.ToTable("Products");
      }
    );

    modelBuilder.Entity<Photo>(etb =>
      {
        etb.HasKey(e => e.Id);
        etb.ToTable("Photos");
      }
    );

    base.OnModelCreating(modelBuilder);
  }
}
````

Create the models classes:

````csharp
public class Category : IModel<Data.Entities.Category, Filters.CategoryFilter>
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public IEnumerable<Product>? Products { get; set; }

  public Category() { }

  public Category(Data.Entities.Category _category)
  {
    this.Id = _category.Id;
    this.Name = _category.Name;
    this.Products = _category.Products?.Select(p => new Product(p));
  }

  public Data.Entities.Category ToEntity()
  {
    return new Data.Entities.Category()
    {
      Id = this.Id,
      Name = this.Name
    };
  }
}
````

````csharp
public class Product : IModel<Data.Entities.Product, Filters.ProductFilter>
{
  public int Id { get; set; }
  public Category? Category { get; set; }
  public string? Name { get; set; }
  public decimal Price { get; set; }
  public DateTime Created { get; set; }
  public IEnumerable<Photo>? Photos { get; set; }

  public Product() { }

  public Product(Data.Entities.Product _product)
  {
    this.Id = _product.Id;
    this.Category = _product.Category == null ? new Category { Id = _product.CategoryId } : new Category(_product.Category);
    this.Name = _product.Name;
    this.Price = _product.Price;
    this.Created = _product.Created;
    this.Photos = _product.Photos?.Select(p => new Photo(p));
  }

  public Data.Entities.Product ToEntity()
  {
    return new Data.Entities.Product()
    {
      Id = this.Id,
      CategoryId = this.Category == null ? 0 : this.Category.Id,
      Name = this.Name,
      Price = this.Price,
      Created = this.Created == DateTime.MinValue ? DateTime.Now : this.Created
    };
  }
}
````

````csharp
public class Photo : IModel<Data.Entities.Photo, Filters.PhotoFilter>
{
  public Product? Product { get; set; }
  public string? Filename { get; set; }
  public IEnumerable<Product>? Products { get; set; }

  public Photo() { }

  public Photo(Data.Entities.Photo _photo)
  {
    this.Product = _photo.Product == null ? new Product { Id = _photo.ProductId } : new Product(_photo.Product);
    this.Filename = _photo.Filename;
  }

  public Data.Entities.Photo ToEntity()
  {
    return new Data.Entities.Photo()
    {
      ProductId = this.Product?.Id ?? 0,
      Filename = this.Filename
    };
  }
}
````

Create the DTOs classes:

````csharp
[Magicalized("v1/categories")]
public class Category : IDto<Domain.Models.Category>
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public IEnumerable<Product>? Products { get; set; }

  public Category() { }

  public Category(Domain.Models.Category _category) : this(_category, ignoreProducts: false) { }

  public Category(Domain.Models.Category _category, bool ignoreProducts = false)
  {
    this.Id = _category.Id;
    this.Name = _category.Name;

    if (!ignoreProducts)
      this.Products = _category.Products?.Select(p => new Product(p, ignoreCategory: true));
  }

  public Domain.Models.Category ToModel()
  {
    return new Domain.Models.Category()
    {
      Id = this.Id,
      Name = this.Name,
      Products = this.Products?.Select(p => p.ToModel())
    };
  }
}
````

````csharp
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

  public Product(Domain.Models.Product _product) : this(_product, ignoreCategory: false, ignorePhotos: false) { }

  public Product(Domain.Models.Product _product, bool ignoreCategory = false, bool ignorePhotos = false)
  {
    this.Id = _product.Id;

    if (!ignoreCategory)
      this.Category = _product.Category == null ? null : new Category(_product.Category, ignoreProducts: true);

    this.Name = _product.Name;
    this.Price = _product.Price;
    this.Created = _product.Created;

    if (!ignorePhotos)
      this.Photos = _product.Photos?.Select(p => new Photo(p, ignoreProduct: true));
  }

  public Domain.Models.Product ToModel()
  {
    return new Domain.Models.Product()
    {
      Id = this.Id,
      Category = this.Category?.ToModel(),
      Name = this.Name,
      Price = this.Price,
      Created = this.Created
    };
  }
}
````

````csharp
[Magicalized("v1/photos")]
public class Photo : IDto<Domain.Models.Photo>
{
  public Product? Product { get; set; }
  public string? Filename { get; set; }

  public Photo() { }

  public Photo(Domain.Models.Photo _photo) : this(_photo, ignoreProduct: false) { }

  public Photo(Domain.Models.Photo _photo, bool ignoreProduct = false)
  {
    if (!ignoreProduct)
      this.Product = _photo.Product == null ? null : new Product(_photo.Product);

    this.Filename = _photo.Filename;
  }

  public Domain.Models.Photo ToModel()
  {
    return new Domain.Models.Photo()
    {
      Product = this.Product?.ToModel(),
      Filename = this.Filename
    };
  }
}
````

Create the DTO validators classes (you can create the separate validators for models):

````csharp
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
````

````csharp
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
````

````csharp
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
````

Optionally, you can also define an authorization policy to restrict access to specific CRUD methods.

If you need to restrict access to certain data in the database based on user information,
the best approach might be to use query filters in your `DbContext` configuration.

Finally, define the filter classes:

````csharp
public class CategoryFilter : IFilter
{
  public IntegerFilter? Id { get; set; }
  public StringFilter? Name { get; set; }
  public EnumerableFilter<ProductFilter>? Products { get; set; }

  public CategoryFilter() { }

  public CategoryFilter(IntegerFilter? id = null, StringFilter? name = null, EnumerableFilter<ProductFilter>? products = null)
  {
    this.Id = id;
    this.Name = name;
    this.Products = products;
  }
}
````

````csharp
public class ProductFilter : IFilter
{
  public IntegerFilter? Id { get; set; }
  public CategoryFilter? Category { get; set; }
  public StringFilter? Name { get; set; }
  public DecimalFilter? Price { get; set; }
  public DateTimeFilter? Created { get; set; }
  public EnumerableFilter<PhotoFilter>? Photos { get; set; }

  public ProductFilter() { }

  public ProductFilter(IntegerFilter? id = null, CategoryFilter? category = null, StringFilter? name = null, DecimalFilter? price = null, DateTimeFilter? created = null, EnumerableFilter<PhotoFilter>? photos = null)
  {
    this.Id = id;
    this.Category = category;
    this.Name = name;
    this.Price = price;
    this.Created = created;
    this.Photos = photos;
  }
}
````

````csharp
public class PhotoFilter : IFilter
{
  public ProductFilter? Product { get; set; }
  public StringFilter? Filename { get; set; }

  public PhotoFilter() { }

  public PhotoFilter(ProductFilter? product = null, StringFilter? filename = null)
  {
    this.Product = product;
    this.Filename = filename;
  }
}
````

Run the web application and try the following requests (you can use a test database from the sample project).

```console
GET: /v1/categories
```

```console
GET: /v1/categories?name.contains=izza
```

```console
GET: /v1/categories?products.any.photos.any.filename.contains=.jpg&fields=products.photos
```

```console
GET: /v1/categories/1
```

```console
GET: /v1/categories/5?fields=products.photos
```

```console
GET: /v1/products
```

```console
GET: /v1/products?id.in=1&id.in=2
```

```console
GET: /v1/products?category.id=5&sorting=+name&offset=0&limit=5
```

```console
GET: /v1/products?category.id=5&sorting=-name&offset=0&limit=5
```

```console
GET: /v1/products?category.name.equals=Pizza&name.contains=ana&fields=category
```

```console
POST: /v1/categories
```

````json
{"name": "Sushi"}
````

```console
PUT: /v1/categories
```

````json
{"id": 1, "name": "Not sushi"}
````

```console
PATCH: /v1/categories/1
```

````json
[{"op": "replace", "path":"name", "value":"Sushi again o_O"}]
````

```console
DELETE: /v1/categories/1
```

If you need to get a service for working with a model of a specific type, use DI (Dependency Injection).

````csharp
.GetService<IService<int, Data.Entities.Product, Product, ProductFilter>>()
````

If you need to replace the default implementation with your own, simply inherit your class
from the default service class or implement the corresponding interface and place it anywhere in the project.

````csharp
public class ProductService : Service<int, Data.Entities.Product, Product, ProductFilter>
{
  public ProductService(DbContext dbContext, IValidator<Product>? validator) : base(dbContext, validator)
  {
  }
}
````

### Samples

* [Sample Magicalizer-based web application](https://github.com/Magicalizer/Magicalizer-Sample);

## Links

Author: http://sikorsky.pro/
