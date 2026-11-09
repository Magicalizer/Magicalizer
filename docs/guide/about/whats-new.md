# What’s new

## 7.0

Magicalizer 7.0 is a big revision of the library: the HTTP API became strict and predictable, the controllers became
ordinary ASP.NET Core controllers with conventions, and the dependencies became lighter. The changes below are
compared with 6.0.2, the last stable version.

### New

- **Options** of the API (`AddMagicalizer(options => ...)`): the maximum limit (also the default one), the maximum
  depth of the inclusions, the ignored query parameters, and whether the lists are counted by default. See
  [Options](../customization/options.md).
- **Skipping the total count** with the `Prefer: count=none` header (and asking for it with `Prefer: count=exact`),
  with `Preference-Applied` and `Vary: Prefer` in the response. See [Paging](../http-api/paging.md).
- **Strict query validation**: an unknown parameter or condition, an invalid value, a sorting, or an inclusion that
  can’t be applied is `400 Bad Request`, with all the errors of the request at once. See
  [Errors](../http-api/errors.md).
- **Errors of the services** as exceptions, `ModelNotFoundException`, `ModelAlreadyExistsException`, and
  `ModelInUseException`, which the API turns into `404 Not Found` and `409 Conflict`. A reference to an object that
  doesn’t exist is a validation error (`400 Bad Request`).
- **New filters and conditions**: `DateTimeOffsetFilter`, `EnumFilter<TEnum>` (by the name or the number),
  `startswith`, `endswith`, and `notin` of the strings, `notin` of the other value filters (except the booleans). See
  [Filters](../building-blocks/filters.md).
- **OpenAPI**: the query parameters are described as they are passed (e.g. `name.contains`,
  `photos.any.filename.endswith`); `IgnoreApi` and `GroupName` of `[Magicalized]` hide an API or put it into another
  document. See [OpenAPI](../customization/openapi.md).
- **Startup diagnostics**: the served DTOs, their routes, methods, and authorization, and the classes that can’t be
  served are logged when the application starts. See [Startup diagnostics](../troubleshooting/startup-diagnostics.md).
- **Cancellation**: all the methods of the services take a `CancellationToken`, and the API passes the one of the
  request.
- `GetKeyValues` of the services, which returns the primary key values of a model.

### Changed

- **The controllers** are `MagicalizedController<...>` (previously `DefaultController<...>`), and their base classes
  have changed. The route, the HTTP methods, the authorization, and the API description are applied by the conventions
  of ASP.NET Core, so they are the usual metadata of the endpoints (the authorization middleware, OpenAPI, and the
  startup log see them), and a [custom controller](../customization/custom-controllers.md) gets them from its DTO. The
  constructor of a custom controller takes only the service.
- **JSON**: System.Text.Json instead of Newtonsoft.Json, including JSON Patch
  (`Microsoft.AspNetCore.JsonPatch.SystemTextJson`).
- **Validation**: FluentValidation 12, run by Magicalizer itself instead of `FluentValidation.AspNetCore`. The
  DataAnnotations of the DTOs are switched off, so the validators are the only source of the rules. See
  [Validators](../building-blocks/validators.md).
- **The filters** share the base classes (`ValueFilterBase<T>`, `EquatableFilterBase<T>`, `ComparableFilterBase<T>`).
  `IsNotNull` and `IsNotEmpty` are removed: use `isnull=false` and `isempty=false`.
- **The services**: the signatures have the `CancellationToken` parameter; `ServiceBase` has the protected
  `GetByKeyValuesAsync` and `DeleteByKeyValuesAsync`.
- `Magicalizer.Validators.Abstractions` no longer depends on FluentValidation, and `System.Linq.Dynamic.Core` is no
  longer used.
- The helpers that were public by accident (`TypeExtensions`, `StringExtensions`, `ExpressionExtensions`, the model
  binders) are internal now.

### Upgrading from 6.0

1. Rename `DefaultController` to `MagicalizedController` in the custom controllers and remove the authorization
   service from their constructors.
2. Add the `CancellationToken` parameter to the overridden methods of the custom services.
3. Replace `isnotnull=true` with `isnull=false` and `isnotempty=true` with `isempty=false` in the clients and in the
   filters built in code.
4. Check the JSON settings: the Newtonsoft.Json options (if you had any) don’t apply anymore.
5. Move the DataAnnotations rules of the DTOs (if any) to the validators.
6. Check the log of the start for the warnings.

## Earlier versions

The earlier versions are on [NuGet](https://www.nuget.org/packages/Magicalizer#versions-body-tab).
