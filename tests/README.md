# Tests

The tests of Magicalizer: the unit tests of the code without the database and the integration tests of the API on
a real database. Each test class covers one area, and the test names say what is checked.

| Project | What it tests |
|---|---|
| `Magicalizer.UnitTests` | The filters, the sortings, the inclusions and the paging applied to queries in memory, the parsing of the query values and the helpers. |
| `Magicalizer.IntegrationTests` | The API through HTTP: the standard actions (also with the composite keys), the filters (both forms of the collection conditions), the sortings, the inclusions, the paging, the validation of the query string, the security, the custom services and controllers, the OpenAPI documents and the startup diagnostics. |
| `Magicalizer.TestHost` | The host the integration tests start: an app on SQLite with the test entities (`Category`, `Product`, `Photo`, `Tag`, `ProductTag`, `Stock`, `Note`), a custom service, a custom controller, a query prefilter, the test authentication and the misconfigured types the startup diagnostics report (`Diagnostics`). |

## Running

```
dotnet test Magicalizer.slnx
```

Each test class of the integration tests starts its own host with its own SQLite database in the temporary folder, so
the classes run in parallel and leave nothing behind. The test users are set by the headers (see
`TestAuthenticationHandler`).

### Other databases

The integration tests run on SQLite by default. To run them on PostgreSQL or SQL Server, set the connection string of
a server (each test class creates its own database on it and deletes it afterwards):

```
MAGICALIZER_TESTS_POSTGRESQL="Host=localhost;Username=postgres;Password=..." dotnet test Magicalizer.slnx
MAGICALIZER_TESTS_SQLSERVER="Server=localhost;User Id=sa;Password=...;TrustServerCertificate=true" dotnet test Magicalizer.slnx
```

### Coverage

```
dotnet test Magicalizer.slnx --collect:"XPlat Code Coverage"
```

Each test project writes `coverage.cobertura.xml` to its `TestResults` folder (any report tool can merge them, e.g.
ReportGenerator).
