---
title: Scaffold a workspace from YAML
description: Describe a multi-project workspace in one file and generate it in a single run.
sidebar:
  order: 4
---

The `scaffold` command reads one YAML file describing a whole workspace and produces it in
a single run. This walkthrough starts from the generated example, validates it, previews
it, and runs it.

## 1. Create a starter file

```bash
mkdir demo && cd demo
create-code-cli scaffold --init
```

That writes `scaffold.yaml`:

```yaml
name: my-project
version: 1.0.0
description: A new project scaffolded with CodeGenerator
outputPath: .
gitInit: true

globalVariables:
  author: Your Name

projects:
  - name: MyProject.Api
    type: dotnet-webapi
    path: src/MyProject.Api
    framework: net9.0
    entities:
      - name: Item
        properties:
          - name: id
            type: uuid
            required: true
          - name: name
            type: string
            required: true
          - name: description
            type: string

solutions:
  - name: MyProject
    projects:
      - MyProject.Api

postScaffoldCommands:
  - dotnet build
```

:::note[Not yet implemented]
**`gitInit` has no effect.** The key is accepted by the schema and validated. No repository is created.

Run `git init` in the output directory yourself, or add it to `postScaffoldCommands`.

_Tracked as [KL-004](/reference/known-limitations/#kl-004) · Applies to CLI 1.2.1_
:::

## 2. Validate it

```bash
create-code-cli scaffold --validate
```

Validation is total: every problem in the document is reported, not just the first. A valid
file prints `Configuration is valid.` and exits `0`; an invalid one lists each error and
exits `1`.

## 3. Preview it

```bash
create-code-cli scaffold --dry-run
```

Nothing is written and no post-scaffold command runs.

:::caution[Known limitation]
**`--dry-run` does not list individual files.** It lists one path per project and one per solution. A real run creates substantially more files than the preview reports.

Use `--dry-run` to confirm where output lands, not what is produced. To see the full file set, scaffold into an empty temporary directory and list it.

_Tracked as [KL-002](/reference/known-limitations/#kl-002) · Applies to CLI 1.2.1_
:::

## 4. Run it

```bash
create-code-cli scaffold
```

Output lands in `./my-project` — the workspace directory is named after the `name` key, not
after the directory you run in. See [output paths](/scaffold/root/) for how `--output`,
`outputPath`, and `name` combine.

:::danger[Destructive behavior]
**`--force` has no effect.** Scaffolding overwrites any existing file at a target path whether or not `--force` is passed. The flag is accepted, validated, and never read.

Commit or back up your work before scaffolding into a directory that already contains files. Run with `--dry-run` first to see which paths are targeted.

_Tracked as [KL-001](/reference/known-limitations/#kl-001) · Applies to CLI 1.2.1_
:::

## 5. Grow the file

From here, the same document can describe much more:

- a [layered architecture](/scaffold/architecture/) expanded from one `architecture` key
- [entities and DTOs](/scaffold/entities/) generated into the right layer
- [endpoints](/scaffold/endpoints/) on the API project
- a React or Angular front end alongside the API ([project types](/scaffold/project-types/))
- [Playwright or Detox](/scaffold/testing/) test projects with page objects and specs

## Next

- [`scaffold` reference](/cli/scaffold/) — every option
- [`scaffold.yaml` reference](/scaffold/) — every key
