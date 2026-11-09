# Authorization

The API of a DTO is open by default. Two attributes, set next to `[Magicalized]`, require an authenticated user or an
authorization policy, for all the HTTP methods or for some of them:

| Attribute | Requires |
|---|---|
| `[AuthenticatedOnly]` | An authenticated user |
| `[AuthorizedOnly("PolicyName")]` | A user that satisfies the authorization policy |

Both take an `HttpMethod` (any by default) and can be repeated. For example, everyone reads the categories, but only
the editors change them:

[!code-csharp[](../../samples/Customization/Api/Dto/Category.cs#L1-L11)]

And only the authenticated users work with the photos:

```csharp
[Magicalized("v1/photos", HttpMethod.Get, HttpMethod.Post, HttpMethod.Delete, GroupName = "internal")]
[AuthenticatedOnly]
public class Photo : IDto<Domain.Models.Photo>
```

The attributes are a part of the DTOs package, so the DTOs don’t depend on ASP.NET Core. Magicalizer turns them into
the usual `[Authorize]` of the actions.

## Authentication and policies

The authentication schemes and the policies are configured as in any ASP.NET Core application:

[!code-csharp[](../../samples/Customization/Program.cs#Authorization)]

```csharp
webApplication.UseAuthentication();
webApplication.UseAuthorization();
webApplication.UseMagicalizer();
```

> [!NOTE]
> The sample authenticates anyone who sends the `X-User` header, with the roles of the `X-Roles` header, so the
> requests are easy to try. A real application uses a real scheme, e.g. JWT bearer tokens or cookies.

## Responses

| Request | Response |
|---|---|
| Without an authenticated user | `401 Unauthorized` |
| With a user that doesn’t satisfy the policy | `403 Forbidden` |
| With a user that does | As usual |

```http
POST /v1/categories
X-User: bob
Content-Type: application/json

{"name": "Pizza"}
```

```http
HTTP/1.1 403 Forbidden
```

## Startup diagnostics

The log of the start shows the rules of each method:

```text
info: Magicalizer[0]
      The magicalized DTO Category is served by MagicalizedController<Category> at /v1/categories: GET, POST (policy Catalog.Edit), PUT (policy Catalog.Edit), PATCH (policy Catalog.Edit), DELETE (policy Catalog.Edit).
```

A policy that isn’t registered is a warning, as all the requests to its methods would fail:

```text
warn: Magicalizer[0]
      The magicalized DTO Category requires the policy Catalog.Edit for POST, but it isn't registered, so the requests will fail.
```

## Restricting the data

The attributes decide who calls a method, not which objects they see. To show the users only their own objects (or
to hide the archived ones), use a [query prefilter](query-prefilters.md).

## See also

- [HTTP methods](http-methods.md)
- [Query prefilters](query-prefilters.md)
- [Authorization in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/authorization/introduction)
