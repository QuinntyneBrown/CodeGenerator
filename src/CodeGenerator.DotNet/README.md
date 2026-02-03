# Endpoint.DotNet

The .NET-specific code generation module for the Endpoint framework. This library provides comprehensive templates and generators for creating C# code, ASP.NET Core applications, and .NET solution structures.

## Overview

Endpoint.DotNet is the primary module for generating .NET code. It includes:

- **Code Generation Engine**: Application builder pattern for flexible code generation
- **Extensive Template Library**: 27+ template categories for various .NET patterns
- **Syntax Manipulation**: C# syntax tree generation and transformation
- **Solution/Project Scaffolding**: Complete .NET solution and project generation

## Project Structure

```
Endpoint.DotNet/
├── Artifacts/          # .NET-specific artifact generators
├── Events/             # Generation event definitions
├── Exceptions/         # Custom exception types
├── Extensions/         # Extension methods
├── Options/            # Configuration options
├── Services/           # Core generation services
├── Syntax/             # C# syntax generators (27 categories)
├── Templates/          # DotLiquid templates
│   ├── Angular/        # Angular-related templates
│   ├── Api/            # API templates
│   ├── Application/    # Application layer templates
│   ├── BuildingBlocks/ # Shared building blocks
│   ├── Console/        # Console app templates
│   ├── DddApp/         # DDD application templates
│   ├── Domain/         # Domain layer templates
│   ├── Global/         # Global configuration templates
│   ├── Infrastructure/ # Infrastructure templates
│   ├── IntegrationTests/ # Integration test templates
│   ├── Lit/            # Lit component templates
│   ├── SharedKernel/   # Shared kernel templates
│   ├── SpecFlow/       # SpecFlow test templates
│   └── ...             # Additional templates
└── CodeGeneratorApplication.cs  # Main application builder
```

## Key Features

### Code Generator Application

```csharp
using Endpoint.DotNet;

var app = CodeGeneratorApplication.CreateBuilder()
    .ConfigureServices(services =>
    {
        services.AddDotNetServices();
        // Add additional services
    })
    .Build();

await app.RunAsync();
```

### Template Categories

- **API**: Controllers, endpoints, middleware
- **Application**: Commands, queries, handlers (CQRS)
- **Domain**: Entities, aggregates, value objects
- **Infrastructure**: DbContext, repositories, configurations
- **Testing**: Unit tests, integration tests, SpecFlow

### Syntax Generation

The module includes extensive C# syntax generators for:

- Classes, interfaces, records
- Methods, properties, fields
- Attributes, namespaces
- Lambda expressions
- Async/await patterns

## Dependencies

- **Endpoint** (Core) - Foundation library
- **Microsoft.CodeAnalysis.CSharp** - Roslyn APIs
- **DotLiquid** - Template engine
- **MediatR** - Request/response handling

## NuGet Package

- **Package ID**: `Quinntyne.Endpoint.Core`
- **Version**: 1.0.12

## Target Framework

- .NET 9.0

## License

This project is licensed under the MIT License.
