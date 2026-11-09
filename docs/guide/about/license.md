# License

Magicalizer is licensed under the [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0).

Copyright © 2020-2026 Dmitry Sikorsky.

## What it means for you

In short (the license text is what counts):

- you may use Magicalizer in any application, commercial or not, open or closed, without paying or asking;
- you may modify it and distribute your modifications, under your own license for your own code;
- if you distribute Magicalizer itself or a modified copy of it, keep the license and the `NOTICE.txt` file, and mark
  the files you changed;
- the license grants you the patents of the contributors that their contributions use, and it ends for those who sue
  over such patents;
- Magicalizer comes without any warranty.

Using the NuGet packages in your application is the first case: nothing else is required.

## The files

| File | Contents |
|---|---|
| [`LICENSE.txt`](https://github.com/Magicalizer/Magicalizer/blob/HEAD/LICENSE.txt) | The text of the license |
| [`NOTICE.txt`](https://github.com/Magicalizer/Magicalizer/blob/HEAD/NOTICE.txt) | The copyright notice, included in the packages |
| `REUSE.toml` and the SPDX headers | The license and the copyright of each file, by the [REUSE](https://reuse.software/) specification |

The packages carry the license expression `Apache-2.0`, so NuGet and the license scanners recognize it.

## The dependencies

Magicalizer depends on ASP.NET Core and Entity Framework Core (MIT), and on FluentValidation (Apache 2.0). Their
licenses apply to them.
