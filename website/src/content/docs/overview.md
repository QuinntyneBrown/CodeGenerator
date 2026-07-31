---
title: What is create-code-cli?
description: The purpose of the tool, the two ways to drive it, and where it fits alongside the CodeGenerator libraries.
sidebar:
  order: 1
---

`create-code-cli` is the command line front end of CodeGenerator, a model-driven code
generation framework. It creates source files, projects, and whole solutions from a
description rather than from a template a developer copies and edits by hand.

There are two ways to drive it, and they suit different situations.

## From the command line

The root command creates a new code generator CLI project: a solution, a console project
referencing every CodeGenerator target package, three example commands, and a script that
packs and installs the result as a global tool of its own.

```bash
create-code-cli --name MyGenerator
```

This is the bootstrapping path. What it produces is not an application, but the scaffolding
a team needs to start writing its own generation commands.

## From a YAML file

The `scaffold` command reads a single `scaffold.yaml` describing a whole workspace —
solutions, projects, layers, entities, endpoints — and produces it in one run.

```bash
create-code-cli scaffold --config ./scaffold.yaml
```

This is the declarative path. It suits a repeatable workspace shape: a clean-architecture
API with four layers, a React front end, and a Playwright test project, described once and
generated on demand.

## Where the libraries fit

The CLI is one consumer of the CodeGenerator libraries. An application can reference
`QuinntyneBrown.CodeGenerator.Core` and the target packages directly, build models in C#,
and call `IArtifactGenerator.GenerateAsync` without involving the CLI at all. The
[project documentation](/project/requirements-l1/) describes that surface.

## What this site covers

Everything here describes what the tool does when you run it. Where a surface behaves
differently from what its name implies, the gap is recorded in
[known limitations](/reference/known-limitations/) and cited wherever that surface is
documented — the reference pages never quietly describe intent as though it were behaviour.

The command and option tables, the configuration tables, the `scaffold.yaml` schema, and
the exit and error code references are generated from the source at build time, and the
build fails if they fall out of step with the code.
