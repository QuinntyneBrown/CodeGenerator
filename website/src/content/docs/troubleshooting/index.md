---
title: Common errors
description: What each failure means and how to resolve it.
sidebar:
  order: 1
---

Every failure carries an [exit code](/reference/exit-codes/) identifying its class, and most
carry a stable [error code](/reference/error-codes/) as well.

## Validation failed: Name is not a valid C# identifier

```
Validation failed:
  - Name: Solution name is not a valid C# identifier. Must start with a letter or
    underscore, followed by letters, digits, underscores, or dots.
```

The solution name becomes a namespace and an assembly name. `My-Generator` and
`2Fast` are rejected; `MyGenerator` and `My.Generator` are accepted.

Exit code `1`.

## Validation failed: Parent directory does not exist

The directory that would contain the output does not exist. Create it first, or point
`--output` somewhere that exists:

```bash
mkdir -p ./sandbox
create-code-cli --name Demo --output ./sandbox
```

Exit code `1`.

## Required option '--name' was not provided

Standard input is not a terminal, so the tool did not prompt. Supply every required option
on the command line. See [running in CI](/guides/ci/).

Exit code `1`.

## No configuration file specified and no scaffold.yaml found

`scaffold` looked for `scaffold.yaml` in the output directory and found none. Either pass
one explicitly or create a starter:

```bash
create-code-cli scaffold --config ./path/to/scaffold.yaml
create-code-cli scaffold --init
```

Exit code `5`.

## Validation error on a scaffold configuration

Every problem in the document is reported at once. Common causes:

| Message | Cause |
|---|---|
| `Invalid semver format` | `version` must be `major.minor.patch`, optionally with a prerelease or build suffix. |
| `Duplicate project name` | Project names are compared case-insensitively. |
| `Referenced project '<name>' not found` | A `references` or solution `projects` entry names an undeclared project. |
| `Path traversal detected` | A `path` contains `..`. |
| `File must specify exactly one of: content, template, or source` | A file declared more than one content source. |

Exit code `1`.

## ERROR [INTERNAL_UNEXPECTED]

A failure the tool did not anticipate. Re-run with `--verbose` for the stack trace, and
please open an issue with it.

Exit code `99`.

## Operation cancelled

The run was interrupted with <kbd>Ctrl</kbd>+<kbd>C</kbd>. Files already written are left
in place — see [rollback](/troubleshooting/rollback/).

Exit code `8`.

## Something generated, but not what I expected

Check the [known limitations](/reference/known-limitations/) register first. Several
surfaces are accepted and validated but do not act — `--force`, `gitInit`, `file.template`,
`file.encoding`, and a project's `dependencies`, `devDependencies`, and `features`.
