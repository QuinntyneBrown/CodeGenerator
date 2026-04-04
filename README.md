# CodeGenerator

[![CI/CD](https://github.com/QuinntyneBrown/CodeGenerator/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/QuinntyneBrown/CodeGenerator/actions/workflows/ci-cd.yml)
[![NuGet](https://img.shields.io/nuget/v/QuinntyneBrown.CodeGenerator.Core.svg?label=CodeGenerator.Core)](https://www.nuget.org/packages/QuinntyneBrown.CodeGenerator.Core)
[![NuGet](https://img.shields.io/nuget/v/QuinntyneBrown.CodeGenerator.DotNet.svg?label=CodeGenerator.DotNet)](https://www.nuget.org/packages/QuinntyneBrown.CodeGenerator.DotNet)
[![NuGet](https://img.shields.io/nuget/v/QuinntyneBrown.CodeGenerator.Cli.svg?label=CodeGenerator.Cli)](https://www.nuget.org/packages/QuinntyneBrown.CodeGenerator.Cli)
[![NuGet Downloads](https://img.shields.io/nuget/dt/QuinntyneBrown.CodeGenerator.Core.svg?label=downloads)](https://www.nuget.org/packages/QuinntyneBrown.CodeGenerator.Core)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A model-driven code generation framework that dramatically reduces tokens for coding agents. Instead of writing code line-by-line, agents build lightweight object models and pass them to the generator — producing complete files, projects, solutions, and full-stack applications.

Supports .NET, Python, React, React Native, Angular, Flask, Playwright, and Detox. Generate clean architecture solutions, APIs, full-stack applications, mobile apps, and end-to-end tests from models, PlantUML diagrams, or code analysis.

## Packages

| Package | Version | Description |
|---------|---------|-------------|
| `QuinntyneBrown.CodeGenerator.Core` | 1.3.1 | Core abstractions, syntax models, and shared services |
| `QuinntyneBrown.CodeGenerator.DotNet` | 1.2.1 | .NET code generation with C# syntax, DotLiquid templates, and solution scaffolding |
| `QuinntyneBrown.CodeGenerator.Angular` | 1.2.1 | Angular workspace and project generation with Jest configuration |
| `QuinntyneBrown.CodeGenerator.Python` | 1.2.2 | Python code generation with classes, functions, modules, decorators, and type hints |
| `QuinntyneBrown.CodeGenerator.React` | 1.2.6 | React workspace and project generation with TypeScript, Vite, Zustand, and TanStack Query |
| `QuinntyneBrown.CodeGenerator.ReactNative` | 1.2.2 | React Native project generation with screens, components, navigation, and stores |
| `QuinntyneBrown.CodeGenerator.Flask` | 1.2.7 | Flask project generation with controllers, SQLAlchemy models, repositories, services, and Marshmallow schemas |
| `QuinntyneBrown.CodeGenerator.Playwright` | 1.2.6 | Playwright test project generation with page object models, specs, and fixtures |
| `QuinntyneBrown.CodeGenerator.Detox` | 1.2.2 | Detox mobile test project generation with page object models, specs, and configuration |
| `QuinntyneBrown.CodeGenerator.Cli` | 1.2.1 | CLI tool to scaffold new code generator projects |

## Installation

Add the packages to your project:

```bash
dotnet add package QuinntyneBrown.CodeGenerator.Core
dotnet add package QuinntyneBrown.CodeGenerator.DotNet
dotnet add package QuinntyneBrown.CodeGenerator.Angular
dotnet add package QuinntyneBrown.CodeGenerator.Python
dotnet add package QuinntyneBrown.CodeGenerator.React
dotnet add package QuinntyneBrown.CodeGenerator.ReactNative
dotnet add package QuinntyneBrown.CodeGenerator.Flask
dotnet add package QuinntyneBrown.CodeGenerator.Playwright
dotnet add package QuinntyneBrown.CodeGenerator.Detox
```

## How It Works

Agents build compact models using fluent builders. The generator handles all the file creation, project configuration, and boilerplate:

```csharp
// ~10 tokens of model code produces a complete C# class file
var model = ClassBuilder.For("Order")
    .Public()
    .WithProperty("Id", "Guid")
    .WithProperty("CustomerId", "Guid")
    .WithProperty("Total", "decimal")
    .WithProperty("Status", "OrderStatus")
    .Build();

var code = await syntaxGenerator.GenerateAsync(model);
```

```csharp
// ~15 tokens produces a full Flask controller with 5 CRUD routes
var controller = ControllerBuilder
    .For("Order")
    .WithCrud("Order")
    .WithUrlPrefix("/api/orders")
    .WithService("order_service", "OrderService")
    .Build();
```

```csharp
// ~10 tokens produces a React component with props, hooks, and imports
var component = ComponentBuilder.For("OrderCard")
    .WithProp("order", "Order")
    .WithHook("useEffect")
    .WithImport("./types", "Order")
    .Build();
```

## Quick Start

Register services and generate code:

```csharp
using CodeGenerator.Core;
using CodeGenerator.Core.Artifacts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddCoreServices(typeof(Program).Assembly);
services.AddDotNetServices();
services.AddAngularServices();
services.AddPythonServices();
services.AddReactServices();
services.AddReactNativeServices();
services.AddFlaskServices();
services.AddPlaywrightServices();
services.AddDetoxServices();

var serviceProvider = services.BuildServiceProvider();
var artifactGenerator = serviceProvider.GetRequiredService<IArtifactGenerator>();

await artifactGenerator.GenerateAsync(model);
```

The framework uses a plugin architecture — generation strategies are automatically discovered and registered from the provided assembly via reflection.

## Features

### .NET
- **Clean Architecture** - Generate complete solution structures with API, Application, Domain, and Infrastructure layers
- **CQRS** - Commands, queries, handlers, and validators following the MediatR pattern
- **Domain-Driven Design** - Entities, aggregates, value objects, domain events, and specifications
- **API Generation** - Controllers, minimal APIs, and route handlers
- **Full-Stack Scaffolding** - End-to-end solution generation including projects, configurations, and dependency injection
- **Roslyn Integration** - C# syntax tree generation and code analysis via Microsoft.CodeAnalysis
- **PlantUML** - Parse sequence diagrams and generate solution models from PlantUML
- **SignalR** - Hub and client generation
- **Testing** - Unit test, integration test, and SpecFlow/BDD scaffolding
- **Git Integration** - Repository operations via LibGit2Sharp

### Python
- **Python Syntax** - Classes, functions, modules, decorators, and type hints
- **Virtual Environments** - Automatic venv creation and pip package installation
- **Project Scaffolding** - Flask, Django, and standalone Python package generation

### Frontend
- **Angular** - Workspace creation, project scaffolding, Jest configuration, standalone components
- **React** - Vite + React + TypeScript workspace generation, components (forwardRef, "use client"), custom hooks, Zustand stores, Axios API clients
- **React Native** - Project scaffolding with React Navigation, screens, components with testID, StyleSheet generation, Zustand stores

### Backend
- **Flask** - App factory pattern, Blueprint controllers, SQLAlchemy models, repositories, services, Marshmallow schemas, middleware decorators, config classes

### Testing
- **Playwright** - Page Object Models with locators/actions/queries, test specs, custom fixtures, multi-browser config
- **Detox** - Mobile Page Object Models with testID selectors, Jest test specs, .detoxrc.js and jest.config.js generation

### Shared
- **Template Engine** - DotLiquid-based templates with embedded resource support
- **Naming Conventions** - PascalCase, camelCase, snake_case, kebab-case conversion
- **React & Lit** - Component generation for React and Lit web components

## CLI Tool

Install the CLI tool globally:

```bash
dotnet tool install -g QuinntyneBrown.CodeGenerator.Cli
```

Update to the latest version:

```bash
dotnet tool update -g QuinntyneBrown.CodeGenerator.Cli
```

### Create a New Code Generator Project

```bash
create-code-cli -n MyCodeGenerator -o ./output
```

This generates a starter CLI project:

```
MyCodeGenerator/
├── eng/
│   └── scripts/
│       └── install-cli.bat
├── src/
│   └── MyCodeGenerator.Cli/
│       ├── Commands/
│       │   ├── AppRootCommand.cs
│       │   └── HelloWorldCommand.cs
│       ├── MyCodeGenerator.Cli.csproj
│       └── Program.cs
└── MyCodeGenerator.sln
```

Build and run:

```bash
cd MyCodeGenerator
dotnet build
dotnet run --project src/MyCodeGenerator.Cli -- hello -o ./output
```

### Install as a Global Tool

Run the included install script to pack and install your CLI globally:

```bash
eng\scripts\install-cli.bat
```

### Install the Claude Skill

Install a Claude skill file so Claude can use the CodeGenerator APIs in your project:

```bash
create-code-cli install
create-code-cli install -o ./MyProject
```

This creates `.claude/skills/code-generator.md` with documentation on all CodeGenerator models, factories, and generation patterns.

## Architecture

```
CodeGenerator/
├── src/
│   ├── CodeGenerator.Core/           # Core abstractions, artifact & syntax engines
│   │   ├── Artifacts/                # IArtifactGenerator, strategies, base classes
│   │   ├── Services/                 # Template processing, naming conventions, tense conversion
│   │   └── Syntax/                   # ISyntaxGenerator, syntax models
│   ├── CodeGenerator.DotNet/         # .NET code generation (C#, solutions, CQRS, PlantUML)
│   │   ├── Artifacts/                # File, project, solution, full-stack strategies
│   │   ├── Syntax/                   # C# syntax generators (classes, methods, controllers, DDD)
│   │   ├── Services/                 # Code analysis, formatting, DI, PlantUML, SignalR
│   │   └── Templates/               # DotLiquid templates (27+ categories)
│   ├── CodeGenerator.Python/         # Python code generation (classes, functions, modules, decorators)
│   │   ├── Artifacts/                # Project, virtualenv, requirements, package strategies
│   │   └── Syntax/                   # Python syntax generators (classes, functions, imports, type hints)
│   ├── CodeGenerator.Angular/        # Angular workspace and project generation
│   ├── CodeGenerator.React/          # React + TypeScript + Vite generation
│   │   ├── Artifacts/                # Workspace, project, barrel file strategies
│   │   └── Syntax/                   # Components, hooks, stores, API clients, TypeScript types
│   ├── CodeGenerator.ReactNative/    # React Native project generation
│   │   ├── Artifacts/                # Project scaffolding with React Navigation
│   │   └── Syntax/                   # Screens, components, navigation, styles, stores
│   ├── CodeGenerator.Flask/          # Flask backend generation
│   │   ├── Artifacts/                # Project scaffolding, blueprint strategies
│   │   └── Syntax/                   # Controllers, models, repositories, services, schemas
│   ├── CodeGenerator.Playwright/     # Playwright test generation
│   │   ├── Artifacts/                # Test project scaffolding
│   │   └── Syntax/                   # Page objects, test specs, fixtures, config
│   ├── CodeGenerator.Detox/          # Detox mobile test generation
│   │   ├── Artifacts/                # Test project scaffolding
│   │   └── Syntax/                   # Page objects, test specs, Detox/Jest config
│   └── CodeGenerator.Cli/            # CLI tool (depends on DotNet)
└── CodeGenerator.sln
```

### Key Services

| Service | Description |
|---------|-------------|
| `IArtifactGenerator` | Main code generation engine — dispatches to registered strategies |
| `ISyntaxGenerator` | Generates syntax strings from models (C#, Python, TypeScript) |
| `IFullStackFactory` | End-to-end application scaffolding |
| `ICodeAnalysisService` | Roslyn-based code analysis |
| `ITemplateProcessor` | DotLiquid template rendering |
| `INamingConventionConverter` | PascalCase, camelCase, snake_case, kebab-case conversion |
| `IPlantUmlParserService` | Parse PlantUML diagrams into solution models |
| `IDependencyInjectionService` | Generate DI configuration |
| `ICommandService` | Execute shell commands (npm, dotnet, python, pip) |

## Documentation

| Document | Description |
|----------|-------------|
| [Architecture Audit](docs/architecture-audit.md) | Comprehensive review of the codebase architecture, strengths, and improvement areas |
| [CLI Vision](docs/codegenerator-cli-vision.md) | Roadmap to a 10/10 CLI and AGI-unlocked features for massive token savings |
| [xregistry/codegen Patterns](docs/xregistry-codegen-patterns.md) | Implementation patterns from [xregistry/codegen](https://github.com/xregistry/codegen) that CodeGenerator could adopt |
| [Detailed Designs](docs/detailed-designs/) | Feature-by-feature design documentation |
| [Specifications](docs/specs/) | Requirements specifications |

## Requirements

- .NET 8.0 or .NET 9.0

## License

MIT

## Author

Quinntyne Brown
