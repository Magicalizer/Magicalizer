# Errors

The errors come as the [problem details](https://www.rfc-editor.org/rfc/rfc9457) of ASP.NET Core, with the
`application/problem+json` content type, e.g. for `DELETE /v1/products/5`:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "The model with the primary key (5) doesn't exist.",
  "traceId": "00-4c9c495c466751b8b8af52b348d34a02-6b699b739a37c5fa-00"
}
```

The validation errors (`400 Bad Request`) list all the problems of the request at once in `errors`, by the query
parameter or the property they concern:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["'Name' must not be empty."],
    "Price": ["'Price' must be greater than '0'."]
  },
  "traceId": "00-d6cde8af54fd03462aaed933f64aa865-02abd714f2c80a6c-00"
}
```

## Status codes

| Status | When |
|---|---|
| `400 Bad Request` | The query, the body, or the object isn’t valid (see below). |
| `401 Unauthorized`, `403 Forbidden` | The endpoint requires an authenticated user or a policy (see [Authorization](../customization/authorization.md)). |
| `404 Not Found` | The object with the primary key doesn’t exist (for a get, `PUT`, `PATCH`, or `DELETE`). `PUT` and `DELETE` explain it in `detail`, a get and `PATCH` come without it. |
| `405 Method Not Allowed` | The application doesn’t serve the HTTP method for the DTO (see [HTTP methods](../customization/http-methods.md)). |
| `409 Conflict` | An object with the same primary key already exists (on creation, when the database doesn’t generate the key), or the object can’t be deleted, as the database restricts it (e.g. other objects refer to it). |
| `415 Unsupported Media Type` | The content type of the body isn’t the expected one (e.g. `application/json` for `PATCH`). |

The other failures (e.g. the database is down) are the usual `500 Internal Server Error` of ASP.NET Core.

## Validation errors

### Query

The query is checked strictly: a parameter the endpoint doesn’t support is an error, not something to ignore. So a
typo never turns into a silently missing filter.

```http
GET /v1/products?naem.contains=pep&sort=colour&limit=500
```

```json
{
  "status": 400,
  "errors": {
    "naem.contains": ["The query parameter isn't supported."],
    "sort": ["Unable to sort by 'colour': the type 'Product' doesn't have the property 'colour'."],
    "limit": ["The limit can't be greater than 100."]
  }
}
```

| Key | Message | Details |
|---|---|---|
| The parameter | `The query parameter isn't supported.` | [Filtering](filtering.md) |
| The filter property (e.g. `Price.From`) | `The value 'abc' is not valid for From.` | [Filtering](filtering.md#values) |
| The collection (e.g. `products.any`) | `The single and list forms of the collection condition can't be combined.` / `The indexes of the collection condition must start from 0 and go without gaps.` | [Filtering](filtering.md#one-element-or-several-elements) |
| `sort` | `Unable to sort by '...': ...` | [Sorting](sorting.md) |
| `include` | `Unable to include '...': ...` | [Inclusion](inclusion.md) |
| `offset`, `limit` | `The field offset must be between 0 and 2147483647.` / `The limit can't be greater than ...` | [Paging](paging.md) |

The parameters the application processes itself (e.g. `culture` of the request localization) are allowed by the
`IgnoredQueryParameters` [option](../customization/options.md).

### Body

| Key | Message | When |
|---|---|---|
| The JSON path (e.g. `$.name`) | The JSON error | The body isn’t valid JSON or has a wrong type of a value. |
| The property (e.g. `Name`) | The message of the validator | The DTO or the model breaks a rule of its validator. |
| The reference (e.g. `Category`) | `The referenced model with the primary key (99) doesn't exist.` | The object refers to an object that doesn’t exist. |
| The JSON Patch path (e.g. `/colour`) | `The target location specified by path segment 'colour' was not found.` | A `PATCH` operation can’t be applied. |
| Empty | `The primary key can't be changed.` | A `PATCH` changes the primary key. |

The DTO is validated first, in the API; the model is validated then, in the service, so the rules that need the
domain (e.g. the uniqueness of a name) live in the validators of the models and come back the same way.

## See also

- [Endpoints](endpoints.md)
- [Validators](../building-blocks/validators.md): the rules and their rule sets
- [Startup diagnostics](../troubleshooting/startup-diagnostics.md): the errors of the configuration
