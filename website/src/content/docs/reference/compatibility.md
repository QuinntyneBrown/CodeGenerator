---
title: Package and framework compatibility
description: Which packages exist, what the CLI requires, and what generated projects target.
sidebar:
  order: 4
---

## Requirements

| | |
|---|---|
| To run the CLI | .NET SDK 8.0 or 9.0 |
| The CLI targets | `net9.0` |
| Generated .NET projects target | `net9.0` by default; any `net*` moniker via [`--framework`](/guides/frameworks/) |

## Packages

The CLI ships separately from the generation libraries. An application that builds models
in C# references the libraries directly.

| Package | Contents |
|---|---|
| `QuinntyneBrown.CodeGenerator.Cli` | The `create-code-cli` global tool |
| `QuinntyneBrown.CodeGenerator.Core` | Generation engine, templates, validation, configuration |
| `QuinntyneBrown.CodeGenerator.DotNet` | C# syntax, solutions, projects, CQRS, DDD, PlantUML |
| `QuinntyneBrown.CodeGenerator.Angular` | Angular workspaces and projects |
| `QuinntyneBrown.CodeGenerator.React` | React with TypeScript and Vite |
| `QuinntyneBrown.CodeGenerator.ReactNative` | React Native projects and screens |
| `QuinntyneBrown.CodeGenerator.Python` | Python classes, modules, and packages |
| `QuinntyneBrown.CodeGenerator.Flask` | Flask apps, blueprints, models, repositories |
| `QuinntyneBrown.CodeGenerator.Playwright` | Playwright page objects, specs, fixtures |
| `QuinntyneBrown.CodeGenerator.Detox` | Detox page objects, specs, configuration |

## Versions referenced by a generated project

A project created by the root command references the packages at versions pinned into the
tool at build time. To see the exact versions your installed tool will stamp, generate a
project and read its `.csproj`:

```bash
create-code-cli --name Probe --output ./probe
cat ./probe/Probe/src/Probe.Cli/Probe.Cli.csproj
```

To reference a local checkout instead of the published packages, see
[developing against a local source tree](/guides/local-source-root/).

## Solution formats

`.sln` and `.slnx` are both supported. Which one you get without `--slnx` depends on the
installed .NET SDK — see [using .slnx solutions](/guides/slnx/).
