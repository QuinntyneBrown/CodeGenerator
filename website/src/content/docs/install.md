---
title: Installation
description: Install, update, and uninstall the create-code-cli global tool.
sidebar:
  order: 2
---

## Requirements

- .NET SDK 8.0 or 9.0

The tool targets `net9.0`. Generated projects can target either.

## Install

```bash
dotnet tool install -g QuinntyneBrown.CodeGenerator.Cli
```

Confirm the install:

```bash
create-code-cli --version
```

## Update

```bash
dotnet tool update -g QuinntyneBrown.CodeGenerator.Cli
```

## Uninstall

```bash
dotnet tool uninstall -g QuinntyneBrown.CodeGenerator.Cli
```

## Install into a directory instead of globally

Useful in CI, where a machine-wide install is undesirable:

```bash
dotnet tool install QuinntyneBrown.CodeGenerator.Cli --tool-path ./.tools
./.tools/create-code-cli --version
```

## The libraries

The CLI is packaged separately from the generation libraries. An application that builds
models in C# references the packages directly instead:

```bash
dotnet add package QuinntyneBrown.CodeGenerator.Core
dotnet add package QuinntyneBrown.CodeGenerator.DotNet
```

The remaining target packages follow the same naming: `.Angular`, `.React`,
`.ReactNative`, `.Python`, `.Flask`, `.Playwright`, and `.Detox`.

## Next

- [Your first generator project](/start/first-project/) — create and run a generator
- [Scaffold a workspace from YAML](/start/first-scaffold/) — the declarative path
