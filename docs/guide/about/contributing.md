# Contributing

Magicalizer is developed on [GitHub](https://github.com/Magicalizer/Magicalizer). Bug reports, questions, ideas, and
pull requests are welcome.

## Reporting a problem

Open an [issue](https://github.com/Magicalizer/Magicalizer/issues) with:

- the version of Magicalizer, .NET and the EF Core provider;
- the log of the start (the `Magicalizer` category, see [Startup diagnostics](../troubleshooting/startup-diagnostics.md));
- the request, the response and, for a `500`, the exception from the log;
- the smallest classes that reproduce it (an entity, a model, a DTO, and a filter are usually enough).

Check the [FAQ](../troubleshooting/faq.md) first: many problems are the setup.

## The repository

| Path | Contents |
|---|---|
| `src/` | The libraries: `Magicalizer` and the `Magicalizer.*.Abstractions` packages |
| `tests/` | The unit tests, the integration tests, and the host they start |
| `docs/` | This documentation and its sample projects |
| `CONVENTIONS.md` | The code style and organization |

## Building and testing

You need the .NET 10 SDK.

```console
dotnet build Magicalizer.slnx
dotnet test Magicalizer.slnx
```

The integration tests run on SQLite by default, each test class with its own database in the temporary folder. To run
them on PostgreSQL or SQL Server too, set the connection string of a server (each test class creates its own database
on it and deletes it afterwards):

```console
MAGICALIZER_TESTS_POSTGRESQL="Host=localhost;Username=postgres;Password=..." dotnet test Magicalizer.slnx
MAGICALIZER_TESTS_SQLSERVER="Server=localhost;User Id=sa;Password=...;TrustServerCertificate=true" dotnet test Magicalizer.slnx
```

The coverage:

```console
dotnet test Magicalizer.slnx --collect:"XPlat Code Coverage"
```

## The conventions

The code follows [CONVENTIONS.md](https://github.com/Magicalizer/Magicalizer/blob/HEAD/CONVENTIONS.md). The main
points:

- the formatting rules are in `.editorconfig`; the build checks the code style (`EnforceCodeStyleInBuild`), and the
  Release build treats the warnings as errors;
- the files are UTF-8 with BOM, CRLF, two spaces for indentation;
- each `.cs` file of the libraries and the tests starts with the SPDX license header, and the repository follows the
  [REUSE](https://reuse.software/) specification (`reuse lint` checks it);
- one type per file, the namespace matches the folder;
- the public types and members have XML documentation (the [API reference](xref:Magicalizer.Extensions) is built from
  it);
- the tests are named after the scenario (`Method_Condition_Result`).

Run `dotnet format` before a commit to apply the formatting rules.

## Pull requests

- Discuss a large change in an issue first, so the design is agreed before the code is written.
- Add or update the tests: the integration tests for the behavior of the API, the unit tests for the logic that
  doesn’t need a database.
- Update the documentation (`docs/`) if the behavior or the API changes, and `README.md` if the features do.
- Keep a pull request to one change.

## The documentation

The documentation is built by [DocFX](https://dotnet.github.io/docfx/):

```console
dotnet tool update -g docfx
cd docs
docfx docfx.json --serve
```

The articles are in `docs/guide`, the code they show is in the projects of `docs/samples`, which are built, so the code
stays correct. See `docs/README.md` for the details.

## See also

- [License](license.md)
