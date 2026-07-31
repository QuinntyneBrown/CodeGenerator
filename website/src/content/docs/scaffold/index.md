---
title: scaffold.yaml overview
description: The structure of a scaffold configuration and how a run is staged.
sidebar:
  order: 1
---

A `scaffold.yaml` describes a whole workspace: what to create, where, and what to run
afterwards. `create-code-cli scaffold` reads it and produces the result in one run.

## A complete example

```yaml
name: shop
version: 1.0.0
description: Storefront API with a React front end
outputPath: .

globalVariables:
  author: Your Name

projects:
  - name: Shop
    type: dotnet-webapi
    path: src/Shop
    framework: net9.0
    architecture: clean-architecture
    entities:
      - name: Order
        properties:
          - name: id
            type: uuid
            required: true
          - name: total
            type: float
    endpoints:
      - name: GetOrder
        method: GET
        route: /orders/{id}
        responseType: Order

  - name: Shop.Web
    type: react-app
    path: src/shop-web

solutions:
  - name: Shop
    format: slnx
    projects:
      - Shop

postScaffoldCommands:
  - dotnet build
```

## How a run is staged

Nothing is written until the whole document is understood.

1. **Parse.** Malformed YAML is reported as a validation error on the `yaml` property, with
   error code `SCAFFOLD_PARSE_FAILED`. Nothing is written.
2. **Validate.** Every problem is collected, not just the first. See
   [root](/scaffold/root/) and [projects](/scaffold/projects/) for the rules.
3. **Resolve the output root.** See [root configuration](/scaffold/root/).
4. **Orchestrate.** Solutions, projects, layers, entities, and files are created.
5. **Post-scaffold commands.** Only when the run is not a dry run.

## Sections

| Page | Covers |
|---|---|
| [Root configuration](/scaffold/root/) | `name`, `version`, `outputPath`, `globalVariables`, `postScaffoldCommands` |
| [solutions[]](/scaffold/solutions/) | Solution files and their member projects |
| [projects[]](/scaffold/projects/) | Every key a project accepts |
| [Project types](/scaffold/project-types/) | The 15 values `type` accepts, and what each creates |
| [Architecture patterns](/scaffold/architecture/) | `clean-architecture`, `vertical-slices`, custom layers |
| [Layers](/scaffold/layers/) | Custom layer definitions |
| [Entities](/scaffold/entities/) | Entities and their properties |
| [DTOs](/scaffold/dtos/) | Objects derived from an entity |
| [Endpoints](/scaffold/endpoints/) | HTTP endpoints |
| [Files and directories](/scaffold/files/) | Explicit files to create |
| [Page objects, specs, fixtures](/scaffold/testing/) | Test project artifacts |
| [Variables](/scaffold/variables/) | Global and per-project values |
| [JSON Schema](/scaffold/json-schema/) | The schema `--export-schema` emits |

## Editor support

Export the schema and point your editor at it for completion and validation while
authoring:

```bash
create-code-cli scaffold --export-schema > scaffold.schema.json
```

## The Effect column

Every key table on these pages carries an **Effect** column with one of three values:

- **Implemented** — consumed and behaves as named.
- **Partial** — consumed, but incompletely. Cites a [known limitation](/reference/known-limitations/).
- **No effect** — parsed and validated, then ignored. Cites a known limitation.

Every key the schema emits has a row, so a key that does nothing cannot quietly look as
though it does.
