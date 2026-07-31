---
title: Use .slnx solutions
description: Request the XML-based solution format, and what the default depends on.
sidebar:
  order: 2
---

The .NET SDK supports two solution formats: the original `.sln`, and `.slnx`, an XML format
that is easier to read and to merge.

## Requesting .slnx explicitly

```bash
create-code-cli --name MyGenerator --slnx
```

With the flag, the tool runs `dotnet new slnx` and the solution is always `MyGenerator.slnx`.

## Without the flag

The tool runs `dotnet new sln`, and the extension is whatever the installed .NET SDK
produces for that command. Older SDKs produce `.sln`; newer ones produce `.slnx` by
default. If a specific extension matters — a build script that globs `*.sln`, for example —
set it explicitly rather than relying on the SDK's default.

## Setting it per repository

`--slnx` is resolved through the [configuration tiers](/config/precedence/). In
`.codegenerator.json` the key is `defaults.solutionFormat`, and it takes the format name
rather than a boolean:

```json
{
  "defaults": {
    "solutionFormat": "slnx"
  }
}
```

`slnx` selects the XML format; any other value selects `sln`.

The `CODEGEN_SLNX` [environment variable](/config/environment/) sets the same setting but
takes `true` or `false`:

```bash
CODEGEN_SLNX=true create-code-cli --name MyGenerator
```

## In a scaffold.yaml

A solution declares its own format, independent of the CLI option:

```yaml
solutions:
  - name: MyProject
    format: slnx
    projects:
      - MyProject.Api
```

See the [solutions reference](/scaffold/solutions/).
