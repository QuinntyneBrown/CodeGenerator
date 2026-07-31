---
title: Precedence and layering
description: The four configuration tiers and the order in which they are applied.
sidebar:
  order: 1
---

A setting can come from four places. They are applied in order, and a later tier replaces
an earlier one:

1. **Built-in defaults** — compiled into the tool. See [defaults](/config/defaults/).
2. **Configuration file** — the nearest `.codegenerator.json`. See [the file](/config/file/).
3. **Environment variables** — the `CODEGEN_` variables. See [environment](/config/environment/).
4. **Command-line arguments** — whatever you type.

A tier that does not set a value leaves the earlier one standing; only a value that is
actually present overrides.

## Worked example

Given a built-in default of `net9.0`, a repository file setting `net8.0`, and no
environment variable:

| Invocation | Effective framework |
|---|---|
| `create-code-cli -n Demo` | `net8.0` — from the file |
| `CODEGEN_FRAMEWORK=net10.0 create-code-cli -n Demo` | `net10.0` — the variable outranks the file |
| `create-code-cli -n Demo -f net9.0` | `net9.0` — the argument outranks everything |

## Key names are case-insensitive

`framework`, `Framework`, and `FRAMEWORK` name the same setting throughout.

## Grouped keys

Keys under a prefix form a section. `templates.author` and `templates.license` belong to
the `templates` section, and a consumer can read the whole section at once.

## Which settings the CLI reads

The commands read three settings: `output`, `framework`, and `slnx`. The `templates.*`
settings are resolved and made available to consumers of the configuration, but no command
in the CLI reads them today.
