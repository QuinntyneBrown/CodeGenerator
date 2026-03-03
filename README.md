# CodeGenerator

A comprehensive code generation framework for .NET applications. Generate clean architecture solutions, APIs, and full-stack applications from models, PlantUML diagrams, or code analysis.

## Packages

| Package | Version | Description |
|---------|---------|-------------|
| `QuinntyneBrown.CodeGenerator.Core` | 1.2.0 | Core abstractions, syntax models, and shared services |
| `QuinntyneBrown.CodeGenerator.DotNet` | 1.2.0 | .NET code generation with C# syntax, DotLiquid templates, and solution scaffolding |
| `QuinntyneBrown.CodeGenerator.Angular` | 1.2.0 | Angular workspace and project generation with Jest configuration |
| `QuinntyneBrown.CodeGenerator.Cli` | 1.1.0 | CLI tool to scaffold new code generator projects |

## Installation

Add the packages to your project:

```bash
dotnet add package QuinntyneBrown.CodeGenerator.Core
dotnet add package QuinntyneBrown.CodeGenerator.DotNet
dotnet add package QuinntyneBrown.CodeGenerator.Angular
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

var serviceProvider = services.BuildServiceProvider();
var artifactGenerator = serviceProvider.GetRequiredService<IArtifactGenerator>();

await artifactGenerator.GenerateAsync(model);
```

The framework uses a plugin architecture — generation strategies are automatically discovered and registered from the provided assembly via reflection.

## Features

- **Clean Architecture** - Generate complete solution structures with API, Application, Domain, and Infrastructure layers
- **CQRS** - Commands, queries, handlers, and validators following the MediatR pattern
- **Domain-Driven Design** - Entities, aggregates, value objects, domain events, and specifications
- **API Generation** - Controllers, minimal APIs, and route handlers
- **Full-Stack Scaffolding** - End-to-end solution generation including projects, configurations, and dependency injection
- **Angular Integration** - Workspace creation, project scaffolding, Jest configuration
- **React & Lit** - Component generation for React and Lit web components
- **Template Engine** - DotLiquid-based templates with embedded resource support
- **Roslyn Integration** - C# syntax tree generation and code analysis via Microsoft.CodeAnalysis
- **PlantUML** - Parse sequence diagrams and generate solution models from PlantUML
- **SignalR** - Hub and client generation
- **Testing** - Unit test, integration test, and SpecFlow/BDD scaffolding
- **Playwright** - End-to-end test scaffolding
- **Git Integration** - Repository operations via LibGit2Sharp

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
│   ├── CodeGenerator.Core/        # Core abstractions, artifact & syntax engines
│   │   ├── Artifacts/             # IArtifactGenerator, strategies, base classes
│   │   ├── Services/              # Template processing, naming conventions, tense conversion
│   │   └── Syntax/                # ISyntaxGenerator, syntax models
│   ├── CodeGenerator.DotNet/      # .NET-specific generation (depends on Core)
│   │   ├── Artifacts/             # File, project, solution, full-stack strategies
│   │   ├── Syntax/                # C# syntax generators (classes, methods, controllers, DDD, etc.)
│   │   ├── Services/              # Code analysis, formatting, DI, PlantUML, SignalR, etc.
│   │   └── Templates/             # DotLiquid templates (27+ categories)
│   ├── CodeGenerator.Angular/     # Angular generation (depends on Core)
│   └── CodeGenerator.Cli/         # CLI tool (depends on DotNet)
└── CodeGenerator.sln
```

### Key Services

| Service | Description |
|---------|-------------|
| `IArtifactGenerator` | Main code generation engine — dispatches to registered strategies |
| `ISyntaxGenerator` | Generates C# syntax trees from models |
| `IFullStackFactory` | End-to-end application scaffolding |
| `ICodeAnalysisService` | Roslyn-based code analysis |
| `ITemplateProcessor` | DotLiquid template rendering |
| `INamingConventionConverter` | PascalCase, camelCase, snake_case conversion |
| `IPlantUmlParserService` | Parse PlantUML diagrams into solution models |
| `IDependencyInjectionService` | Generate DI configuration |

## Requirements

- .NET 8.0 or .NET 9.0

## License

MIT

## Author

Quinntyne Brown
