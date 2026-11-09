# Browser clients

A single-page application (React, Vue, Angular, Blazor WebAssembly) on another origin than the API needs CORS: the
browser asks the API whether the origin may call it, and hides the response headers the API doesn’t expose. For a
Magicalizer API, the headers matter: the paging and the `Location` of the created objects are in them.

## The policy

[!code-csharp[](../../samples/Recipes/Program.cs#Cors)]

```json
{
  "Cors": {
    "Origins": ["http://localhost:5173"]
  }
}
```

And the middleware goes before the authentication and `UseMagicalizer`:

```csharp
webApplication.UseCors();
webApplication.UseAuthentication();
webApplication.UseAuthorization();
webApplication.UseMagicalizer();
```

What each part is for:

| Part | Why |
|---|---|
| `WithOrigins` | The origins of your clients, from the configuration, so each environment has its own |
| `AllowAnyMethod` | `PUT`, `PATCH`, and `DELETE` aren’t allowed by default |
| `AllowAnyHeader` | `Content-Type: application/json`, `Authorization`, `Prefer` aren’t simple headers, so the browser asks first |
| `WithExposedHeaders` | Without it, the script sees none of the headers below |

The exposed headers:

| Header | Response |
|---|---|
| `Paging-Total-Count`, `Paging-Offset`, `Paging-Limit` | The lists (see [Paging](../http-api/paging.md)) |
| `Preference-Applied` | The lists with the `Prefer` header |
| `Location` | `201 Created` of `POST` |

## Checking it

The preflight request of the browser, and the answer:

```http
OPTIONS /v1/notes
Origin: http://localhost:5173
Access-Control-Request-Method: POST
Access-Control-Request-Headers: content-type,prefer
```

```http
HTTP/1.1 204 No Content
Access-Control-Allow-Headers: content-type,prefer
Access-Control-Allow-Methods: POST
Access-Control-Allow-Origin: http://localhost:5173
```

And the actual request:

```http
HTTP/1.1 200 OK
Access-Control-Allow-Origin: http://localhost:5173
Access-Control-Expose-Headers: Location,Paging-Total-Count,Paging-Offset,Paging-Limit,Preference-Applied
Paging-Total-Count: 1
```

An origin that isn’t in the list gets no `Access-Control-Allow-Origin`, and the browser blocks the response.

## In the client

```js
const response = await fetch("https://api.example.com/v1/notes?offset=20&limit=10&sort=title", {
  headers: { Authorization: `Bearer ${token}` }
});

const notes = await response.json();
const total = Number(response.headers.get("Paging-Total-Count"));
const pages = Math.ceil(total / 10);
```

A partial update goes as JSON Patch, with its own content type:

```js
await fetch("https://api.example.com/v1/notes/1", {
  method: "PATCH",
  headers: { "Content-Type": "application/json-patch+json", Authorization: `Bearer ${token}` },
  body: JSON.stringify([{ op: "replace", path: "/title", value: "Buy oat milk" }])
});
```

## Cookies

With the cookie authentication instead of tokens, the policy also needs `AllowCredentials()`, the client sends
`credentials: "include"`, and the origins must be listed explicitly (`AllowAnyOrigin` can’t be combined with the
credentials).

## The same origin

If the client is served from the same origin as the API (e.g. the API at `/api` behind the same host, or a reverse
proxy in front of both), there is no CORS at all, and the headers are visible to the scripts as they are.

## See also

- [Paging](../http-api/paging.md#browser-clients)
- [Enable CORS in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/cors)
