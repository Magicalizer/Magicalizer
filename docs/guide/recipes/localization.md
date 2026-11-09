# Localization

A Magicalizer API is localized as any ASP.NET Core application: the request localization middleware sets the culture
of the request, and the validation messages follow it. The only thing specific to Magicalizer is the
[strict query validation](../http-api/errors.md): the API rejects the query parameters it doesn’t know, so the ones of
the localization must be allowed.

## The culture of a request

`UseRequestLocalization` takes the culture from the query string (`culture` and `ui-culture`), a cookie, or the
`Accept-Language` header, in this order:

[!code-csharp[](../../samples/Recipes/Program.cs#Localization)]

It goes before `UseMagicalizer`, so the controllers run in the culture of the request.

## Allowing the parameters

Without more, `GET /v1/notes?culture=uk` is `400 Bad Request`:

```json
{
  "status": 400,
  "errors": {
    "culture": ["The query parameter isn't supported."]
  }
}
```

Add the parameters of the localization to `IgnoredQueryParameters` (the names are case-insensitive):

```csharp
builder.Services.AddMagicalizer(options => {
  options.IgnoredQueryParameters.Add("culture");
  options.IgnoredQueryParameters.Add("ui-culture");
});
```

The `Accept-Language` header and the cookie need nothing: they aren’t query parameters.

## The validation messages

FluentValidation has the translations of its standard messages and picks them by the UI culture of the request, so
the [validators](../building-blocks/validators.md) of the DTOs and the models need no changes:

```http
POST /v1/notes?culture=uk
Content-Type: application/json

{"notebook": {"id": 1}, "title": ""}
```

```json
{
  "status": 400,
  "errors": {
    "Title": ["'Title' не може бути порожнім."]
  }
}
```

The same comes with `Accept-Language: uk`. To localize the rest:

- your own messages (`WithMessage`) and the property names in the messages (`WithName`) take the localized strings
  from `IStringLocalizer`, as described in the
  [FluentValidation documentation](https://docs.fluentvalidation.net/en/latest/localization.html);
- the messages of ASP.NET Core model binding (e.g. `The value 'abc' is not valid for From.`) are set by
  `ModelBindingMessageProvider` of the MVC options;
- the messages of Magicalizer itself (e.g. `The query parameter isn't supported.`, `Unable to sort by ...`) are in
  English only. The clients tell them apart by the key of the error (the parameter), not by the text.

## The data

Magicalizer doesn’t translate the data: a localized name of a product is a matter of the data model, e.g. a related
entity with the translations of a product (`ProductTranslation` with the culture and the name), which a client
[includes](../http-api/inclusion.md) and [filters](../http-api/filtering.md#collections)
(`translations.any.culture.equals=uk`), or a [custom service](../customization/custom-services.md) that picks the
translation of the current culture.

## See also

- [Options](../customization/options.md)
- [Globalization and localization in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/localization)
