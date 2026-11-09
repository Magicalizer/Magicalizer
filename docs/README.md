# Magicalizer documentation

The source of https://docs.magicalizer.net, built by [DocFX](https://dotnet.github.io/docfx/) with its modern
template.

| Path | Contents |
|---|---|
| `index.md` | The home page. |
| `guide/` | The articles, by section (`guide/toc.yml` is the navigation). |
| `samples/` | The projects with the code of the articles; they are built, so the code stays correct. |
| `template/` | The additions to the modern template (the brand color, the GitHub link). |
| `images/` | The images. |
| `api/` | The API reference, generated from the XML comments of the projects in `src` (not in the repository). |

## Building

```
dotnet tool update -g docfx
docfx docfx.json --serve
```

The site is generated into `_site` and served at http://localhost:8080. The samples are built and tested separately:

```
dotnet build samples/GettingStarted
dotnet build samples/Customization
dotnet build samples/Layered/Layered.slnx
dotnet test samples/Recipes.Tests
```

## Writing

- The articles are in English, one Markdown file per page, the title is the first heading.
- The code is included from the samples by regions or lines, e.g.
  `[!code-csharp[](../../samples/GettingStarted/Program.cs)]`, rather than copied into the articles.
- The HTTP requests are in `http` code blocks, the JSON in `json` ones.
