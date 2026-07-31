---
title: Choose a target framework
description: How --framework is resolved and which values are accepted.
sidebar:
  order: 1
---

`--framework` sets the target framework moniker stamped into generated .NET projects.

```bash
create-code-cli --name MyGenerator --framework net8.0
```

## Accepted values

The value must begin with `net` followed by a digit. `net8.0`, `net9.0`, and `net10.0` are
the values the tool suggests; any well-formed moniker is accepted, so a newer one works
without a tool update.

An invalid value fails validation before anything is written:

```
Validation failed:
  - Framework: Invalid target framework. Must start with 'net' (e.g., 'net8.0', 'net9.0').
```

The process exits `1`. See [exit codes](/reference/exit-codes/).

## Where the value comes from

`--framework` is resolved through the four [configuration tiers](/config/precedence/). In
order of increasing precedence:

1. the built-in default, `net9.0`
2. `defaults.framework` in [`.codegenerator.json`](/config/file/)
3. the `CODEGEN_FRAMEWORK` [environment variable](/config/environment/)
4. `--framework` on the command line

Setting it once per repository is usually the right move:

```json
{
  "defaults": {
    "framework": "net8.0"
  }
}
```

## Per-project frameworks in a scaffold

In a `scaffold.yaml`, each project sets its own framework, and `--framework` does not apply:

```yaml
projects:
  - name: MyProject.Api
    type: dotnet-webapi
    path: src/MyProject.Api
    framework: net8.0
```

A project that omits `framework` gets `net9.0`. See the
[projects reference](/scaffold/projects/).
