# CodeGenerator

A comprehensive code generation framework for .NET applications. Generate clean architecture solutions, APIs, and full-stack applications with ease.

## Packages

| Package | Description |
|---------|-------------|
| `QuinntyneBrown.CodeGenerator.Core` | Core abstractions, syntax models, and shared services |
| `QuinntyneBrown.CodeGenerator.DotNet` | .NET code generation with C# syntax, templates, and solution scaffolding |
| `QuinntyneBrown.CodeGenerator.Angular` | Angular workspace and project generation with Jest configuration |
| `QuinntyneBrown.CodeGenerator.Cli` | CLI tool to scaffold new code generator projects |

## CLI Tool

Install the CLI tool globally:

```bash
dotnet tool install -g QuinntyneBrown.CodeGenerator.Cli
```

Update to the latest version:

```bash
dotnet tool update -g QuinntyneBrown.CodeGenerator.Cli
```

Create a new code generator project:

```bash
create-code-cli -n MyCodeGenerator -o ./output
```

This generates a starter CLI project with the following structure:

```
MyCodeGenerator/
├── eng/
│   └── scripts/
│       └── install-cli.bat    # Script to install your CLI as a global tool
├── src/
│   └── MyCodeGenerator.Cli/
│       ├── Commands/
│       │   ├── AppRootCommand.cs
│       │   └── HelloWorldCommand.cs
│       ├── MyCodeGenerator.Cli.csproj
│       └── Program.cs
└── MyCodeGenerator.sln
```

### Getting Started with Your Generated CLI

```bash
cd MyCodeGenerator
dotnet build
dotnet run --project src/MyCodeGenerator.Cli -- hello -o ./output
```

### Installing Your CLI as a Global Tool

Run the included install script to install your CLI as a global dotnet tool:

```bash
eng\scripts\install-cli.bat
```

This will build, pack, and install your CLI globally, allowing you to run it from anywhere using `mycodegenerator-cli`.

### Installing the Claude Skill

Install a Claude skill file into your project so Claude knows how to use the CodeGenerator APIs:

```bash
create-code-cli install
```

Or specify a target directory:

```bash
create-code-cli install -o ./MyProject
```

This creates `.claude/skills/code-generator.md` in the target directory with comprehensive documentation on all CodeGenerator models, factories, and generation patterns.

## Installation

```bash
dotnet add package QuinntyneBrown.CodeGenerator.Core
dotnet add package QuinntyneBrown.CodeGenerator.DotNet
dotnet add package QuinntyneBrown.CodeGenerator.Angular
```

## Usage

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddCoreServices(typeof(Program).Assembly);
services.AddDotNetServices();

var serviceProvider = services.BuildServiceProvider();
var artifactGenerator = serviceProvider.GetRequiredService<IArtifactGenerator>();

await artifactGenerator.GenerateAsync(model);
```

## Features

- **Clean Architecture** - Generate complete solution structures following clean architecture patterns
- **CQRS Support** - Commands, queries, and handlers generation
- **Domain-Driven Design** - Entities, aggregates, value objects, and domain events
- **API Generation** - Controllers, minimal APIs, route handlers
- **Angular Integration** - Workspace creation, project scaffolding, Jest configuration
- **Template Engine** - DotLiquid-based templates for customizable code generation
- **Roslyn Integration** - C# syntax tree generation and manipulation

## Requirements

- .NET 9.0

## License

MIT

## Author

Quinntyne Brown
