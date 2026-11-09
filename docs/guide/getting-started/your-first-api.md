# Your first API

In this tutorial, you build the REST API of a small catalog: categories, their products, and the photos of the products.
You write the entities, models, DTOs, filters, and validators, and Magicalizer does the rest. It takes about 15 minutes.

The complete code is in the [GettingStarted](https://github.com/Magicalizer/Magicalizer/tree/HEAD/docs/samples/GettingStarted)
sample.

## Create the project

Create an empty web application and add the packages (see [Installation](installation.md)):

```console
dotnet new web -o App
cd App
dotnet add package Magicalizer
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.AspNetCore.OpenApi
```

The classes go to the folders of their layers: `Data/Entities`, `Domain/Models`, `Api/Dto`, `Api/Dto/Validators`, and
`Filters`. The folders and namespaces are only a convention: Magicalizer finds the classes wherever they are.

## 1. Entities

The entities are the classes of Entity Framework Core. To be used by Magicalizer, an entity implements `IEntity`, whose
type arguments are the types of its primary key properties (in the order of the primary key).

# [Category](#tab/category)

[!code-csharp[](../../samples/GettingStarted/Data/Entities/Category.cs)]

# [Product](#tab/product)

[!code-csharp[](../../samples/GettingStarted/Data/Entities/Product.cs)]

# [Photo](#tab/photo)

[!code-csharp[](../../samples/GettingStarted/Data/Entities/Photo.cs)]

---

The database context is configured as usual:

[!code-csharp[](../../samples/GettingStarted/Data/Entities/AppDbContext.cs)]

## 2. Program

Register the database context as `DbContext`, add Magicalizer and the OpenAPI description:

[!code-csharp[](../../samples/GettingStarted/Program.cs)]

Add the connection string to `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=app.db"
  }
}
```

> [!TIP]
> `EnsureCreated` creates the database on the first run, which is handy for a tutorial. A real application uses the
> [migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/) of Entity Framework Core instead.

## 3. Models

The models are the domain layer, the one the services work with. A model implements `IModel`, whose type arguments
are its entity and its filter (you create the filters in step 5). It’s created from its entity by the constructor that
takes the entity, and it’s converted back by `ToEntity`.

# [Category](#tab/category)

[!code-csharp[](../../samples/GettingStarted/Domain/Models/Category.cs)]

# [Product](#tab/product)

[!code-csharp[](../../samples/GettingStarted/Domain/Models/Product.cs)]

# [Photo](#tab/photo)

[!code-csharp[](../../samples/GettingStarted/Domain/Models/Photo.cs)]

---

Note the `ignore...` parameters. The related entities refer to each other: the products of an included category refer
to it, and it refers to them. So the related models leave out the references back (a product of a category is
created with `ignoreCategory: true`), otherwise the conversion would never end.

When a related entity isn’t loaded, the model still keeps its primary key (`new Category { Id = ... }`), so the
reference isn’t lost on an update.

## 4. DTOs

The DTOs are the contract of the API: what the clients send and get. A DTO implements `IDto`, whose type argument is
its model, and it’s marked with `[Magicalized]`, which sets the route of its API. Like a model, it’s created from its
model by the constructor and converted back by `ToModel`.

# [Category](#tab/category)

[!code-csharp[](../../samples/GettingStarted/Api/Dto/Category.cs)]

# [Product](#tab/product)

[!code-csharp[](../../samples/GettingStarted/Api/Dto/Product.cs)]

# [Photo](#tab/photo)

[!code-csharp[](../../samples/GettingStarted/Api/Dto/Photo.cs)]

---

Each `[Magicalized]` DTO gets a controller with the list, get by the primary key, create, update, JSON Patch, and delete
actions.

## 5. Filters

A filter describes how the objects can be filtered. Each of its properties filters the entity property with the same
name: a value filter (e.g. `StringFilter`), the filter of a related entity, or `EnumerableFilter` of a collection.

# [Category](#tab/category)

[!code-csharp[](../../samples/GettingStarted/Filters/CategoryFilter.cs)]

# [Product](#tab/product)

[!code-csharp[](../../samples/GettingStarted/Filters/ProductFilter.cs)]

# [Photo](#tab/photo)

[!code-csharp[](../../samples/GettingStarted/Filters/PhotoFilter.cs)]

---

A property that isn’t in the filter can’t be filtered by: the request is rejected (see
[Errors](../http-api/errors.md)).

## 6. Validators

The validators are the usual FluentValidation ones. The validators of the DTOs run in the API, the validators of the
models (none here) run in the services. The rules without a rule set are checked always, the ones of
`RuleSetName.Create` and `RuleSetName.Edit` only on creation and editing.

# [Category](#tab/category)

[!code-csharp[](../../samples/GettingStarted/Api/Dto/Validators/CategoryValidator.cs)]

# [Product](#tab/product)

[!code-csharp[](../../samples/GettingStarted/Api/Dto/Validators/ProductValidator.cs)]

# [Photo](#tab/photo)

[!code-csharp[](../../samples/GettingStarted/Api/Dto/Validators/PhotoValidator.cs)]

---

## 7. Run

Run the application:

```console
dotnet run
```

Magicalizer logs the APIs it has created:

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

If a DTO is missing from the list, see [Startup diagnostics](../troubleshooting/startup-diagnostics.md).

## 8. Try the API

Send the requests with any HTTP client. The sample has them in
[GettingStarted.http](https://github.com/Magicalizer/Magicalizer/blob/HEAD/docs/samples/GettingStarted/GettingStarted.http),
which Visual Studio, Rider, and VS Code (with the REST Client extension) run right from the editor.

### Create

```http
POST /v1/categories
Content-Type: application/json

{"name": "Pizza"}
```

```http
HTTP/1.1 201 Created
Location: /v1/categories/1

{"id": 1, "name": "Pizza", "products": null}
```

Create a second category, `Sushi`, a few products, and a photo the same way (the `.http` file has all of them):

```http
POST /v1/products
Content-Type: application/json

{"category": {"id": 1}, "name": "Margherita", "price": 9.5}
```

A product refers to its category by the primary key. A category that doesn’t exist (`{"id": 99}`) is reported as an
error of the request, not of the server:

```http
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Category": ["The referenced model with the primary key (99) doesn't exist."]
  }
}
```

### Validate

The validators run before anything is saved, and all the errors come at once:

```http
POST /v1/products
Content-Type: application/json

{"category": {"id": 1}, "name": "", "price": 0}
```

```http
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["'Name' must not be empty."],
    "Price": ["'Price' must be greater than '0'."]
  }
}
```

### Query

Filter, sort, and page the lists with the query parameters:

```http
GET /v1/products?category.name.equals=Pizza&sort=-price
```

```json
[
  {"id": 2, "category": {"id": 1, "name": null, "products": null}, "name": "Quattro Formaggi", "price": 12.0, ...},
  {"id": 1, "category": {"id": 1, "name": null, "products": null}, "name": "Margherita", "price": 9.5, ...}
]
```

The related objects aren’t loaded unless they are included (the category has only its primary key above). Include
them, the nested ones too:

```http
GET /v1/categories/1?include=products.photos
```

```json
{
  "id": 1,
  "name": "Pizza",
  "products": [
    {"id": 1, "name": "Margherita", "price": 9.5, "photos": [{"id": 1, "filename": "margherita.jpg", ...}], ...},
    {"id": 2, "name": "Quattro Formaggi", "price": 12.0, "photos": [], ...}
  ]
}
```

The lists are paged by `offset` and `limit`, and the total count comes in the headers:

```http
GET /v1/products?sort=name&offset=0&limit=2
```

```http
HTTP/1.1 200 OK
Paging-Total-Count: 3
Paging-Offset: 0
Paging-Limit: 2
```

A mistake in a query is never ignored silently: all of them are reported.

```http
GET /v1/products?naem.contains=pep&sort=colour
```

```json
{
  "status": 400,
  "errors": {
    "sort": ["Unable to sort by 'colour': the type 'Product' doesn't have the property 'colour'."],
    "naem.contains": ["The query parameter isn't supported."]
  }
}
```

### Update and delete

`PUT` replaces an object, `PATCH` changes a part of it with a [JSON Patch](https://jsonpatch.com/) (mind its content
type), and `DELETE` deletes it. All three return `204 No Content`, or `404 Not Found` when the object doesn’t exist.

```http
PUT /v1/categories
Content-Type: application/json

{"id": 2, "name": "Sushi and rolls"}
```

```http
PATCH /v1/categories/2
Content-Type: application/json-patch+json

[{"op": "replace", "path": "/name", "value": "Rolls"}]
```

```http
DELETE /v1/photos/1
```

### See the description

The OpenAPI description of the API, with all the filter conditions, is at `/openapi/v1.json`. Open it in any OpenAPI
viewer (e.g. add [Scalar](https://github.com/scalar/scalar) or Swagger UI to the application), or generate a client
from it.

## What you’ve built

With 16 small classes and no controllers, services, or query code, the application has:

- 3 REST APIs with 6 actions each;
- filtering by the properties, the related objects, and the collections, as well as sorting and paging;
- the inclusion of the related objects;
- the validation and the errors as problem details;
- the OpenAPI description.

## Next steps

- [A layered solution](layered-solution.md): split the classes into the projects of their layers.
- [HTTP API](../http-api/endpoints.md): everything the clients can do with the API.
- [Building blocks](../building-blocks/entities.md): the entities, models, DTOs, filters, and validators in depth.
- [Customization](../customization/custom-services.md): your own services, controllers, and prefilters.
