---
title: Variables and post-scaffold commands
description: Values shared across projects, and commands run after files are written.
sidebar:
  order: 13
---

## Variables

`globalVariables` holds values available to every project; a project's own `variables`
holds values scoped to it.

```yaml
globalVariables:
  author: Your Name
  company: Example Ltd

projects:
  - name: Shop.Api
    type: dotnet-webapi
    path: src/Shop.Api
    variables:
      port: "5001"
```

Both are string-to-string maps.

## Post-scaffold commands

`postScaffoldCommands` runs shell commands in the resolved output root after every file has
been written.

```yaml
postScaffoldCommands:
  - dotnet restore
  - dotnet build
  - npm install --prefix src/shop-web
```

Three properties are worth knowing:

- **They do not run during a dry run.** [`--dry-run`](/guides/dry-run/) skips them entirely.
- **A failure does not abort the run.** Each command's exit code is recorded; a non-zero
  code is reported as a warning naming the command and its code, and the remaining commands
  still run.
- **They run in the output root**, which is `<output>/<outputPath>/<name>` — not the
  directory you invoked the tool from. See [root configuration](/scaffold/root/).

### They run through the system shell

Commands are executed with `cmd.exe /C` on Windows and `bash -c` elsewhere. A
`scaffold.yaml` is therefore executable content: running `scaffold` against a file you did
not write runs whatever that file's author put in this list.

Read the `postScaffoldCommands` of any configuration you did not author before running it.

## Git initialization

:::note[Not yet implemented]
**`gitInit` has no effect.** The key is accepted by the schema and validated. No repository is created.

Run `git init` in the output directory yourself, or add it to `postScaffoldCommands`.

_Tracked as [KL-004](/reference/known-limitations/#kl-004) · Applies to CLI 1.2.1_
:::

```yaml
postScaffoldCommands:
  - git init
  - git add -A
```
