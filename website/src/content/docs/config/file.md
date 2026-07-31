---
title: .codegenerator.json
description: The configuration file, where it is found, and every key it accepts.
sidebar:
  order: 2
---

A repository can set defaults for every invocation in a `.codegenerator.json` file.

## Where it is found

The tool starts in the current working directory and walks **upward** to the filesystem
root, using the first `.codegenerator.json` it finds. A file at the root of a repository
therefore applies to every directory beneath it, without being repeated.

If no file is found anywhere in the ancestor chain, no error is raised — the file tier is
simply empty.

## Shape

```json
{
  "defaults": {
    "framework": "net9.0",
    "output": "./generated",
    "solutionFormat": "slnx"
  },
  "templates": {
    "author": "Your Name",
    "license": "MIT",
    "templatesDirectory": "./templates"
  }
}
```

Property names are matched case-insensitively.

## Keys

| JSON path | Sets | Notes |
|---|---|---|
| `defaults.framework` | `framework` | Target framework moniker, for example `net9.0`. |
| `defaults.output` | `output` | Default output directory. |
| `defaults.solutionFormat` | `slnx` | `slnx` selects the XML solution format; any other value selects `sln`. |
| `templates.author` | `templates.author` | Available to templates. |
| `templates.license` | `templates.license` | Available to templates. |
| `templates.templatesDirectory` | `templates.directory` | Available to templates. No environment variable sets this key. |

## Precedence

The file outranks the built-in defaults and is outranked by environment variables and
command-line arguments. See [precedence](/config/precedence/).

## A note on solutionFormat

The key takes a format name — `sln` or `slnx` — while the equivalent
[environment variable](/config/environment/) `CODEGEN_SLNX` takes `true` or `false`. Both
set the same underlying setting.
