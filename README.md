# Magicalizer 2.0.0

![Magicalizer logotype](https://magicalizer.net/magicalizer_github_icon.png)

## Introduction

Magicalizer allows you to get a fully featured REST API ASP.NET Core web application almost without writing any routine code.
In most of the cases all you need to write are DTO/model/entity classes and validators for them. That’s all.
Your web application will automatically support complex filtering (multiple nested objects properties, ranges etc.),
complex sorting (multiple nested objects properties, different directions), paging, complex inclusions (multiple nested objects properties too),
flexible and powerful validation on the DTO and model levels (using FluentValidation), policy-based authorization for GET, POST, PUT, PATCH,
and DELETE requests independently (and yes, you will have PATCH requests support too).

Important thing. While Magicalizer does all the work, you can still replace any default implementation with your own when needed.
Once you create and register a concrete service implementation for some of the models, it will be used instead of the default one automatically.
The same thing with the controllers and repositories. Moreover, Magicalizer is built on top of the [ExtCore framework](https://github.com/ExtCore/ExtCore),
so it is modular and extendable by default. It is not important where you put your DTO/model/entity classes, your validators, services,
or controllers. They will be found and resolved automatically (even from the DLLs at runtime). It makes it amazingly simple to reuse the code
and create decoupled software.

Some of the code parts might look ugly so far, but I think it is OK for such an early stage. I wanted to release the first version ASAP
to get feedback from you, other developers, and my colleagues to improve the project.

## Quick Start

To start using Magicalizer:

* add dependency on Magicalizer.Api, Magicalizer.Data.Repositories.EntityFramework, and Magicalizer.Domain.Services.Defaults packages to your web application project;
* add dependency on ExtCore.Data.EntityFramework.Sqlite to your web application project too (in order to use SQLite; Microsoft SQL Server and PostgreSQL are supported too);
* call AddMagicalizer and UseMagicalizer inside your web application's Startup class;
* provide a database connection string using the `services.Configure<StorageContextOptions>(...)` method call.

Web application is ready.

Now create the entities classes:

````csharp
public class Category : IEntity<int>
{
  public int Id { get; set; }
  public string Name { get; set; }

  public virtual ICollection<Product> Products { get; set; }
}
````

````csharp
public class Product : IEntity<int>
{
  public int Id { get; set; }
  public int CategoryId { get; set; }
  public string Name { get; set; }
  public decimal Price { get; set; }
  public DateTime Created { get; set; }

  public virtual Category Category { get; set; }
}
````

Register them inside the storage context:

````csharp
public class EntityRegistrar : IEntityRegistrar
{
  public void RegisterEntities(ModelBuilder modelBuilder)
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
  }
}
````

Create the models classes:

````csharp
public class Category : IModel<Data.Entities.Category, Filters.CategoryFilter>
{
  public int Id { get; set; }
  public string Name { get; set; }
  public IEnumerable<Product> Products { get; set; }

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
  public Category Category { get; set; }
  public string Name { get; set; }
  public decimal Price { get; set; }
  public DateTime Created { get; set; }

  public Product() { }

  public Product(Data.Entities.Product _product)
  {
    this.Id = _product.Id;
    this.Category = _product.Category == null ? null : new Category(_product.Category);
    this.Name = _product.Name;
    this.Price = _product.Price;
    this.Created = _product.Created;
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

Create the model validators classes:

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
      this.RuleFor(p => p.Category.Id).GreaterThan(0);
    });

    this.RuleFor(p => p.Name).NotEmpty().MaximumLength(64);
    this.RuleFor(p => p.Price).GreaterThan(0m);
    this.RuleSet(RuleSetName.Edit, () => {
      this.RuleFor(p => p.Id).NotEmpty();
    });
  }
}
````

Create the DTOs classes:

````csharp
[Magicalized("v1/categories")]
public class Category : IDto<Domain.Models.Category>
{
  public int Id { get; set; }
  public string Name { get; set; }
  public IEnumerable<Product> Products { get; set; }

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
  public Category Category { get; set; }
  public string Name { get; set; }
  public decimal Price { get; set; }
  public DateTime Created { get; set; }

  public Product() { }

  public Product(Domain.Models.Product _product) : this(_product, ignoreCategory: false) { }

  public Product(Domain.Models.Product _product, bool ignoreCategory = false)
  {
    this.Id = _product.Id;

    if (!ignoreCategory)
      this.Category = _product.Category == null ? null : new Category(_product.Category, ignoreProducts: true);

    this.Name = _product.Name;
    this.Price = _product.Price;
    this.Created = _product.Created;
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

Create the DTO validators classes:

````csharp
public class IngredientValidator : AbstractValidator<Category>
{
  public IngredientValidator()
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
      this.RuleFor(p => p.Category.Id).GreaterThan(0);
    });

    this.RuleFor(p => p.Name).NotEmpty().MaximumLength(64);
    this.RuleFor(p => p.Price).GreaterThan(0m);
    this.RuleSet(RuleSetName.Edit, () => {
      this.RuleFor(p => p.Id).NotEmpty();
    });
  }
}
````

Run the web application and try the following requests (you can use a test database from the sample project).

GET: /v1/categories

GET: /v1/categories?name.contains=izza

GET: /v1/categories/1

GET: /v1/categories/5?fields=products

GET: /v1/products

GET: /v1/products?category.id=5&sorting=+name&offset=0&limit=5

GET: /v1/products?category.id=5&sorting=-name&offset=0&limit=5

GET: /v1/products?category.name.equals=Pizza&name.contains=ana&fields=category

POST: /v1/categories

````json
{"name": "Sushi"}
````

PUT: /v1/categories

````json
{"id": 1, "name": "Not sushi"}
````

PATCH: /v1/categories/1

````json
[{"op": "replace", "path":"name", "value":"Sushi again o_O"}]
````

DELETE: /v1/categories/1

### Samples

* [Sample Magicalizer-based web application](https://github.com/Magicalizer/Magicalizer-Sample);

## Links

Author: http://sikorsky.pro/
