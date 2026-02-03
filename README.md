# CodeGenerator

A comprehensive code generation framework for .NET applications. Generate clean architecture solutions, APIs, and full-stack applications with ease.

## Packages

| Package | Description |
|---------|-------------|
| `QuinntyneBrown.CodeGenerator.Core` | Core abstractions, syntax models, and shared services |
| `QuinntyneBrown.CodeGenerator.DotNet` | .NET code generation with C# syntax, templates, and solution scaffolding |
| `QuinntyneBrown.CodeGenerator.Angular` | Angular workspace and project generation with Jest configuration |

## Features

- **Clean Architecture** - Generate complete solution structures following clean architecture patterns
- **CQRS Support** - Commands, queries, and handlers generation
- **Domain-Driven Design** - Entities, aggregates, value objects, and domain events
- **API Generation** - Controllers, minimal APIs, route handlers
- **Angular Integration** - Workspace creation, project scaffolding, Jest configuration
- **Template Engine** - DotLiquid-based templates for customizable code generation
- **Roslyn Integration** - C# syntax tree generation and manipulation

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

## Requirements

- .NET 9.0

## License

MIT

## Author

Quinntyne Brown
