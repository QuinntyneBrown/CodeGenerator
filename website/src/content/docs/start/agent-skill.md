---
title: Install the agent skill
description: Give a coding agent the CodeGenerator APIs so it can generate code instead of writing it line by line.
sidebar:
  order: 5
---

CodeGenerator was built for coding agents as much as for people. An agent that describes an
artifact as a model spends a fraction of the tokens it would spend writing the artifact
out, and the generator handles the boilerplate.

The `install` command writes a skill file describing the CodeGenerator APIs into a project,
so an agent working in that repository knows how to use them.

```bash
create-code-cli install
```

This creates:

```
.claude/skills/code-generator/SKILL.md
```

Target another directory with `--output`:

```bash
create-code-cli install --output ./MyProject
```

Intermediate directories are created as needed, and the written path is logged.

## What the skill contains

The file documents the package references to add, the `services.Add*Services()`
registration calls, the core services (`IArtifactGenerator`, `ISyntaxGenerator`,
`ITemplateProcessor`, `ICommandService`), and the model catalog for each supported
framework.

## Why it saves tokens

A fluent model describes an artifact far more compactly than the artifact itself:

```csharp
var controller = ControllerBuilder
    .For("Order")
    .WithCrud("Order")
    .WithUrlPrefix("/api/orders")
    .Build();
```

Four lines describe a Flask controller with five CRUD routes, its Blueprint registration,
and its service wiring. The generated file is many times longer.

## Next

- [`install` reference](/cli/install/) — the command's options
