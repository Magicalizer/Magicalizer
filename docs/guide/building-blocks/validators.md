# Validators

The DTOs and the models are validated by the usual [FluentValidation](https://docs.fluentvalidation.net/) validators.
Magicalizer registers all of them (the classes derived from `AbstractValidator<T>`) and runs them at the right time:
you only write them.

## Two levels

| Validator of | Runs | Rejects with |
|---|---|---|
| A DTO | In the API, before the DTO is converted to the model | `400 Bad Request` with the errors by the DTO properties |
| A model | In the service, before the model is saved | `400 Bad Request` (in the API) or `ValidationException` (in your code) |

The **DTO validators** check what the clients send: the required fields, the lengths, the formats. They work with the
properties of the DTO, so their errors are named as the clients know them.

The **model validators** check the domain rules, which hold whoever saves the model: the API, a background job, or an
import. They are created by the dependency injection, so they may take what they need (e.g. the `DbContext` to check
that a name is unique). They can’t take the service of their own model, as it takes them: that would be a circular
dependency.

Both are optional: a DTO or a model without a validator isn’t validated.

## Rule sets

The rules without a rule set are checked always. The rules of the `Create` and `Edit` rule sets
(`RuleSetName` of the `Magicalizer.Validators.Abstractions` package) are checked only on creation and on editing:

[!code-csharp[](../../samples/GettingStarted/Api/Dto/Validators/ProductValidator.cs)]

| Operation | Rule sets |
|---|---|
| `POST`, `CreateAsync` | The rules without a rule set and `Create` |
| `PUT`, `PATCH`, `EditAsync` | The rules without a rule set and `Edit` |

So the primary key is required on editing (`RuleSetName.Edit`), but not on creation, where the database generates it.

`RuleSetName` also has the combined names, `DefaultCreate` (`"Default,Create"`) and `DefaultEdit`
(`"Default,Edit"`), for your own code that validates the same way, e.g.
`validator.ValidateAsync(dto, o => o.IncludeRuleSets(RuleSetName.Default, RuleSetName.Create))`.

## Errors

All the failures of a request come at once, as the problem details with the `errors` by the property:

```json
{
  "status": 400,
  "errors": {
    "Name": ["'Name' must not be empty."],
    "Price": ["'Price' must be greater than '0'."]
  }
}
```

The messages and the property names are the ones of FluentValidation, so they are configured as usual: `WithMessage`,
`WithName`, the global `ValidatorOptions`, or the localization of FluentValidation.

## DataAnnotations

The DataAnnotations attributes (e.g. `[Required]`, `[StringLength]`) and the implicit `[Required]` that ASP.NET Core
adds for the non-nullable reference types aren’t applied to the DTOs: the validators are the only source of the
rules, so the clients get the same kind of errors everywhere. The other models of the application (e.g. the
parameters of your own controllers) keep them.

## See also

- [DTOs](dtos.md), [Models](models.md)
- [Errors](../http-api/errors.md)
- [`RuleSetName` in the API reference](xref:Magicalizer.Validators.Abstractions.RuleSetName)
