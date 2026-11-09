# Testing

The best tests of a Magicalizer API go through HTTP: they check what the clients get, including the parts you didn’t
write (the routes, the binding, the validation, the errors). ASP.NET Core runs the application in memory for that,
with `WebApplicationFactory`, and each test class gets its own database. The tests of `docs/samples/Recipes` are in
`docs/samples/Recipes.Tests`, and Magicalizer itself is tested the same way.

## The project

A test project (xUnit v3 here) that references the web application:

[!code-xml[](../../samples/Recipes.Tests/Recipes.Tests.csproj)]

The `Program` class of a web application with the top-level statements is public in .NET 10, so the tests reach it
without `public partial class Program`.

## The factory

The factory starts the application with its own SQLite file, so the test classes don’t share the data and can run in
parallel, and deletes the file afterwards:

[!code-csharp[](../../samples/Recipes.Tests/AppFactory.cs)]

- `UseSetting` overrides the connection string of `appsettings.json` before the application reads it.
- `Pooling=false` closes the connections, so the file can be deleted.
- The sample creates the database with `EnsureCreated` at the start. An application with migrations calls
  `Database.Migrate()` instead, or the factory does it.
- `CreateClient(user)` signs the client in by the header of the sample authentication. With a real scheme, replace it in
  the factory by a test one (`ConfigureTestServices` and `AddAuthentication`).

## The tests

Each test class gets one factory (`IClassFixture`), and its tests share the application and the database. The tests
create the data they need, so they don’t depend on each other:

[!code-csharp[](../../samples/Recipes.Tests/NotebookTests.cs)]

The DTOs of the application are the request and response types of the tests, so the tests break at the compile time
when the contract changes.

## Running

```console
dotnet test
```

```text
Passed!  - Failed:     0, Passed:     3, Skipped:     0, Total:     3, Duration: 1 s - Recipes.Tests.dll (net10.0)
```

## Other databases

SQLite is fast and needs no server, but it isn’t your production database: the [comparison of the
strings](postgresql-and-sql-server.md#the-case-of-the-strings), the dates, and the errors of the constraints differ. If
production is PostgreSQL or SQL Server, run the tests on it too (e.g. in CI, with the server in a container): the
factory sets a connection string of a new database for each test class and deletes the database afterwards.

## What else to test

- The startup: a test that the application starts and serves the expected DTOs catches a DTO that is skipped (e.g.
  without a model). The [startup diagnostics](../troubleshooting/startup-diagnostics.md) log the reason.
- The custom services: through the API as above, or directly from a scope of the factory
  (`factory.Services.CreateScope()`), with the current user and the other services replaced by
  `ConfigureTestServices`.
- The validators: as plain classes, with `TestValidate` of FluentValidation.

## See also

- [Integration tests in ASP.NET Core](https://learn.microsoft.com/aspnet/core/test/integration-tests)
- [Users see only their own data](own-data.md), [Soft deletion](soft-deletion.md)
