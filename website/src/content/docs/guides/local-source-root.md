---
title: Develop against a local source tree
description: Point a generated project at a CodeGenerator checkout instead of the published packages.
sidebar:
  order: 3
---

By default a generated project references the CodeGenerator packages from NuGet at pinned
versions. When you are changing the framework and a generator that uses it at the same
time, that round trip through a package feed is in the way.

`--local-source-root` replaces the package references with project references:

```bash
create-code-cli --name MyGenerator --local-source-root ../CodeGenerator/src
```

The path points at the `src` directory of a CodeGenerator checkout.

## What changes in the generated .csproj

Without the flag:

```xml
<ItemGroup>
  <PackageReference Include="QuinntyneBrown.CodeGenerator.Core" Version="1.3.0" />
  <PackageReference Include="QuinntyneBrown.CodeGenerator.DotNet" Version="1.2.0" />
  <!-- ...seven more... -->
</ItemGroup>
```

With it:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\..\CodeGenerator\src\CodeGenerator.Core\CodeGenerator.Core.csproj" />
  <ProjectReference Include="..\..\..\CodeGenerator\src\CodeGenerator.DotNet\CodeGenerator.DotNet.csproj" />
  <!-- ...seven more... -->
</ItemGroup>
```

Nine projects are referenced either way. The paths are computed relative to the generated
project directory, so the result is portable within the layout it was generated for.

## When the path is wrong

The tool does not verify that the referenced projects exist — the failure surfaces on the
first build of the generated solution, naming the unresolved path. If a build fails that
way, check that the path ends in `src` and points at a CodeGenerator checkout:

```bash
ls ../CodeGenerator/src/CodeGenerator.Core/CodeGenerator.Core.csproj
```

## Switching back

Regenerate without the flag, or replace the `ProjectReference` block with the
`PackageReference` block by hand. The versions to use are listed in the
[compatibility reference](/reference/compatibility/).
