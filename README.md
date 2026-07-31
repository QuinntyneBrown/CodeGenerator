# CodeGenerator

A model-driven, extensible code-generation framework for creating source files, projects, solutions, and application scaffolds across multiple technology stacks.

[![Build](https://github.com/QuinntyneBrown/CodeGenerator/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/QuinntyneBrown/CodeGenerator/actions/workflows/ci-cd.yml)
[![NuGet](https://img.shields.io/nuget/v/QuinntyneBrown.CodeGenerator.Core.svg?label=NuGet)](https://www.nuget.org/packages/QuinntyneBrown.CodeGenerator.Core)
[![NuGet downloads](https://img.shields.io/nuget/dt/QuinntyneBrown.CodeGenerator.Core.svg?label=downloads)](https://www.nuget.org/packages/QuinntyneBrown.CodeGenerator.Core)
[![.NET 8 and 9](https://img.shields.io/badge/.NET-8%20%7C%209-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/license-MIT-0078D4.svg)](LICENSE.txt)
[![Contributions welcome](https://img.shields.io/badge/contributions-welcome-107C10.svg)](CONTRIBUTING.md)

[Packages](#packages) · [Get started](#get-started) · [CLI](#command-line-tool) · [Documentation](#documentation) · [Build and test](#build-and-test) · [Contributing](CONTRIBUTING.md)

## Overview

CodeGenerator lets applications and coding agents describe an intended result as a compact object model, then delegates rendering and file creation to target-specific strategies. This keeps callers focused on structure and intent while the framework handles syntax, templates, naming conventions, and project layout.

The repository includes generators for .NET, Python, Angular, React, React Native, Flask, Playwright, and Detox. It can generate individual syntax elements as strings or coordinate larger artifacts such as projects, solutions, clean-architecture applications, and test suites.

Key capabilities include:

- Fluent builders and strongly typed models for concise generation requests
- Strategy discovery and dependency-injection-based extensibility
- C#, Python, and TypeScript-oriented syntax generation
- DotLiquid templates with embedded and file-system template support
- Project and solution scaffolding for backend, frontend, mobile, and test stacks
- Naming conversion, validation, diagnostics, dry-run, and rollback primitives
- YAML-driven scaffolding and a .NET global tool for generator projects

> [!IMPORTANT]
> Generated code is a starting point, not a substitute for engineering review. Validate, test, secure, and license generated output before using it in production.

## Packages

Packages are versioned and published independently. Install only the packages needed by your generator.

| Package | Purpose |
| --- | --- |
| [`QuinntyneBrown.CodeGenerator.Abstractions`](https://www.nuget.org/packages/QuinntyneBrown.CodeGenerator.Abstractions) | Lightweight contracts, base models, validation, results, and shared enums |
| [`QuinntyneBrown.CodeGenerator.Core`](https://www.nuget.org/packages/QuinntyneBrown.CodeGenerator.Core) | Artifact and syntax dispatch, templates, configuration, naming, validation, and shared services |
| [`QuinntyneBrown.CodeGenerator.DotNet`](https://www.nuget.org/packages/QuinntyneBrown.CodeGenerator.DotNet) | C# syntax, Roslyn analysis, .NET projects and solutions, CQRS, DDD, APIs, and PlantUML |
| [`QuinntyneBrown.CodeGenerator.Angular`](https://www.nuget.org/packages/QuinntyneBrown.CodeGenerator.Angular) | Angular workspaces, projects, TypeScript models, and Jest configuration |
| [`QuinntyneBrown.CodeGenerator.React`](https://www.nuget.org/packages/QuinntyneBrown.CodeGenerator.React) | React, TypeScript, Vite, components, hooks, stores, routers, and API clients |
| [`QuinntyneBrown.CodeGenerator.ReactNative`](https://www.nuget.org/packages/QuinntyneBrown.CodeGenerator.ReactNative) | React Native projects, screens, components, navigation, styles, and stores |
| [`QuinntyneBrown.CodeGenerator.Python`](https://www.nuget.org/packages/QuinntyneBrown.CodeGenerator.Python) | Python classes, functions, modules, packages, requirements, and virtual environments |
| [`QuinntyneBrown.CodeGenerator.Flask`](https://www.nuget.org/packages/QuinntyneBrown.CodeGenerator.Flask) | Flask projects, controllers, models, repositories, services, schemas, and middleware |
| [`QuinntyneBrown.CodeGenerator.Playwright`](https://www.nuget.org/packages/QuinntyneBrown.CodeGenerator.Playwright) | Playwright projects, page objects, fixtures, configuration, and test specifications |
| [`QuinntyneBrown.CodeGenerator.Detox`](https://www.nuget.org/packages/QuinntyneBrown.CodeGenerator.Detox) | Detox mobile test projects, page objects, specifications, and configuration |
| [`QuinntyneBrown.CodeGenerator.Cli`](https://www.nuget.org/packages/QuinntyneBrown.CodeGenerator.Cli) | .NET tool for creating generator projects, scaffolding from YAML, and installing agent guidance |

The reusable libraries target .NET 8 and .NET 9. The CLI currently targets .NET 9.

## Get started

### Prerequisites

- [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0) for repository development and the CLI
- A .NET 8 or .NET 9 project for the reusable libraries

### Install the libraries

For C# generation, add the core and .NET packages:

```bash
dotnet add package QuinntyneBrown.CodeGenerator.Core
dotnet add package QuinntyneBrown.CodeGenerator.DotNet
```

Add other target packages in the same way when your generator needs them.

### Generate C# from a model

Register the framework, create a model, and pass it to `ISyntaxGenerator`:

```csharp
using CodeGenerator.Core;
using CodeGenerator.Core.Syntax;
using CodeGenerator.DotNet.Builders;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddLogging();
services.AddCoreServices(typeof(Program).Assembly);
services.AddDotNetServices();

using var serviceProvider = services.BuildServiceProvider();
var syntaxGenerator = serviceProvider.GetRequiredService<ISyntaxGenerator>();

var order = ClassBuilder.For("Order")
    .Public()
    .Sealed()
    .WithProperty("Id", "Guid")
    .WithProperty("CustomerId", "Guid")
    .WithProperty("Total", "decimal")
    .Build();

var source = await syntaxGenerator.GenerateAsync(order);
Console.WriteLine(source);
```

The same pattern applies across targets:

1. Register core services and the target package.
2. Create a syntax or artifact model.
3. Use `ISyntaxGenerator` for source text or `IArtifactGenerator` for files and larger structures.

## Command-line tool

Install the global tool:

```bash
dotnet tool install --global QuinntyneBrown.CodeGenerator.Cli
```

Create a generator project:

```bash
create-code-cli --name Contoso.CodeGeneration --output ./tools
```

Useful commands include:

```bash
# Show all project-creation options
create-code-cli --help

# Inspect YAML-driven project scaffolding
create-code-cli scaffold --help

# Install CodeGenerator guidance for Claude
create-code-cli install --output .
```

Update an existing global installation with:

```bash
dotnet tool update --global QuinntyneBrown.CodeGenerator.Cli
```

## Generation targets

| Area | Supported outputs |
| --- | --- |
| .NET | C# syntax, projects, solutions, clean architecture, CQRS, domain models, APIs, SignalR, SpecFlow, and PlantUML-driven models |
| Python | Classes, functions, decorators, modules, packages, requirements, virtual environments, and project scaffolds |
| Web | Angular workspaces; React and TypeScript projects, components, hooks, state, routing, and API clients |
| Mobile | React Native projects, screens, navigation, styles, components, and state |
| Backend | Flask app factories, blueprints, controllers, services, repositories, SQLAlchemy models, and schemas |
| Testing | Playwright browser tests and Detox mobile tests, including page objects, fixtures, specs, and configuration |

## Architecture

CodeGenerator separates shared orchestration from target-specific models and strategies:

```text
CodeGenerator/
├── src/
│   ├── CodeGenerator.Abstractions/    Shared public contracts and primitives
│   ├── CodeGenerator.Core/            Generation engine and cross-target services
│   ├── CodeGenerator.DotNet/          .NET syntax and artifact strategies
│   ├── CodeGenerator.Angular/         Angular generation
│   ├── CodeGenerator.React/           React generation
│   ├── CodeGenerator.ReactNative/     React Native generation
│   ├── CodeGenerator.Python/          Python generation
│   ├── CodeGenerator.Flask/           Flask generation
│   ├── CodeGenerator.Playwright/      Playwright generation
│   ├── CodeGenerator.Detox/           Detox generation
│   └── CodeGenerator.Cli/             Global tool and scaffolding commands
├── tests/                              Unit and integration test projects
├── eng/                                Engineering and installation scripts
└── CodeGenerator.sln                   Repository solution
```

At runtime, models are dispatched to compatible generation strategies. Target packages register their own strategies and services, allowing a consumer to compose only the generation surfaces it needs.

## Documentation

**[create-code-cli documentation site](https://quinntynebrown.github.io/CodeGenerator/)** — every
command, option, configuration key, `scaffold.yaml` key, exit code, and error code. The
reference sections are generated from the source on every build, so they cannot drift from
the tool.

| Document | Purpose |
| --- | --- |
| [Documentation site](https://quinntynebrown.github.io/CodeGenerator/) | CLI reference, guides, and the `scaffold.yaml` schema |
| [Known limitations](https://quinntynebrown.github.io/CodeGenerator/reference/known-limitations/) | Surfaces that do not behave the way their name implies |
| [High-level requirements](docs/specs/L1.md) | Product scope and system-level capabilities reverse-engineered from the implementation |
| [Detailed requirements](docs/specs/L2.md) | Traceable behavior and acceptance criteria for each high-level requirement |
| [Detailed designs](docs/detailed-designs/) | Feature-by-feature design with C4, class, and sequence diagrams |

## Build and test

Clone and validate the repository:

```bash
git clone https://github.com/QuinntyneBrown/CodeGenerator.git
cd CodeGenerator

dotnet restore CodeGenerator.sln
dotnet build CodeGenerator.sln --configuration Release --no-restore
dotnet test CodeGenerator.sln --configuration Release --no-build
```

During development, run the closest test project first. For example:

```bash
dotnet test tests/CodeGenerator.DotNet.UnitTests/CodeGenerator.DotNet.UnitTests.csproj
```

See [CONTRIBUTING.md](CONTRIBUTING.md) for the complete development workflow and pull-request expectations.

## Community

| Document | Purpose |
| --- | --- |
| [Contributing](CONTRIBUTING.md) | Development setup, quality gates, and pull-request guidance |
| [Code of Conduct](CODE_OF_CONDUCT.md) | Standards for respectful community participation |
| [Security](SECURITY.md) | Supported versions and private vulnerability reporting |
| [Support](SUPPORT.md) | Help channels and information to include with a report |
| [Governance](GOVERNANCE.md) | Project roles, decisions, and maintainer responsibilities |
| [Changelog](CHANGELOG.md) | Notable changes and release history |
| [Contributors](CONTRIBUTORS.md) | Contributor recognition |

Contributions of code, tests, templates, documentation, issue triage, and design feedback are welcome. Please read the [contribution guide](CONTRIBUTING.md) and follow the [Code of Conduct](CODE_OF_CONDUCT.md).

## Security

Do not report suspected vulnerabilities in a public issue. Follow the private reporting process in [SECURITY.md](SECURITY.md).

## License

Copyright © 2026 CodeGenerator contributors. CodeGenerator is released under the [MIT License](LICENSE.txt).
