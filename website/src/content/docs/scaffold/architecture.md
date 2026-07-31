---
title: Architecture patterns
description: How a named architecture expands into layers, references, and member assignment.
sidebar:
  order: 5.5
---

A project can name an architecture instead of listing its layers. The resolver expands the
name into concrete projects, wires the references between them, and distributes the
project's entities, services, and endpoints to the right layer.

## clean-architecture

```yaml
projects:
  - name: Shop
    type: dotnet-webapi
    path: src/Shop
    architecture: clean-architecture
```

Expands into four layers under `src/`:

| Layer | Type | References | Receives |
|---|---|---|---|
| `Shop.Domain` | class library | — | the project's `entities` |
| `Shop.Application` | class library | `Shop.Domain` | — |
| `Shop.Infrastructure` | class library | `Shop.Application` | the project's `services` |
| `Shop.Api` | web API | `Shop.Application`, `Shop.Infrastructure` | the project's `endpoints` |

The reference chain runs one way: Application sees Domain, Infrastructure sees Application,
and the API sees both Application and Infrastructure. Nothing references the API.

## vertical-slices

```yaml
projects:
  - name: Shop
    type: dotnet-webapi
    path: src/Shop
    architecture: vertical-slices
```

Expands into a single web API project at `src/Shop` holding the entities, services, and
endpoints together.

## Custom layers

Declaring [`layers`](/scaffold/layers/) without an `architecture` produces exactly those
layers:

```yaml
projects:
  - name: Shop
    type: dotnet-webapi
    path: src/Shop
    layers:
      - name: Shop.Core
        type: dotnet-classlib
      - name: Shop.Api
        type: dotnet-webapi
        references:
          - Shop.Core
```

## No pattern

A project with neither `architecture` nor `layers` is created as a single project of its
declared [type](/scaffold/project-types/).

## Unrecognized values

Matching is case-insensitive. A value that is neither `clean-architecture` nor
`vertical-slices` is **not** an error: the project falls back to custom layers when
`layers` is non-empty, and to no pattern otherwise. A typo in the architecture name
therefore produces a single project rather than a failure, so check the output when the
layer set is not what you expected.
