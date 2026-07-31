---
title: Your first generator project
description: Create, build, and run a code generator CLI in about five minutes.
sidebar:
  order: 3
---

This walkthrough creates a code generator CLI, builds it, and runs one of its example
commands. It takes about five minutes and leaves you with a project you can start adding
your own generation commands to.

## 1. Create the project

```bash
create-code-cli --name MyGenerator
```

The solution is created in `./MyGenerator`. Pass `--output` to put it elsewhere:

```bash
create-code-cli --name MyGenerator --output ./sandbox
```

## 2. Look at what was created

```
MyGenerator/
├── MyGenerator.sln
├── eng/
│   └── scripts/
│       └── install-cli.bat
└── src/
    └── MyGenerator.Cli/
        ├── MyGenerator.Cli.csproj
        ├── Program.cs
        └── Commands/
            ├── AppRootCommand.cs
            ├── HelloWorldCommand.cs
            └── EnterpriseSolutionCommand.cs
```

`Program.cs` already registers every CodeGenerator target package, so a new command can
generate .NET, Python, Flask, Angular, React, React Native, Playwright, or Detox artifacts
without further setup. The `.csproj` is marked `PackAsTool`, with a tool command name of
`mygenerator-cli`.

The solution file extension follows the .NET SDK. Older SDKs produce `MyGenerator.sln`;
newer ones produce `MyGenerator.slnx`. Pass [`--slnx`](/guides/slnx/) to request the
XML-based format explicitly.

## 3. Build it

```bash
cd MyGenerator
dotnet build
```

## 4. Run the example command

```bash
dotnet run --project src/MyGenerator.Cli -- hello --output ./output
```

This writes `./output/HelloWorld.txt` through the generation engine — a minimal example of
the model-to-file path your own commands will use.

The second example command generates a full-stack solution:

```bash
dotnet run --project src/MyGenerator.Cli -- enterprise-solution --name SampleEnterprise --output ./output
```

## 5. Install it as a global tool

The generated script packs the project and installs it machine-wide:

```bash
eng\scripts\install-cli.bat
```

After it completes, `mygenerator-cli` is on your path.

## Developing against a CodeGenerator checkout

By default the generated `.csproj` references the CodeGenerator packages from NuGet. To
work against a local clone instead — useful when changing the framework and the generator
together — pass the path to its `src` directory:

```bash
create-code-cli --name MyGenerator --local-source-root ../CodeGenerator/src
```

The generated project then carries `ProjectReference` entries computed relative to itself.
See [developing against a local source tree](/guides/local-source-root/).

## Next

- [Scaffold a workspace from YAML](/start/first-scaffold/) — generate many projects at once
- [`create-code-cli` reference](/cli/create-code-cli/) — every option
