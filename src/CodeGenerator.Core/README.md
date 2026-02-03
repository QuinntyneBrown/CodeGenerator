# Endpoint

The core foundation library for the Endpoint code generation framework. This library provides the essential building blocks, utilities, interfaces, and services that power the entire code generation engine.

## Overview

Endpoint serves as the foundational layer upon which all other Endpoint modules are built. It provides:

- **Artifact Definitions**: Base classes and interfaces for defining code generation artifacts
- **Syntax Support**: Core syntax manipulation and parsing capabilities
- **Service Infrastructure**: Essential services for the generation pipeline
- **Caching Utilities**: Object and string caching for optimized performance
- **Extension Methods**: String manipulation and helper utilities

## Project Structure

```
Endpoint/
├── Artifacts/         # Code generation artifact definitions
├── Internal/          # Internal implementation details
├── Services/          # Core service implementations
├── Syntax/            # Syntax parsing and manipulation
├── ConfigureServices.cs   # DI registration
├── IObjectCache.cs        # Caching interface
├── ObjectCache.cs         # Object cache implementation
├── StringBuilderCache.cs  # StringBuilder pooling
└── StringExtensions.cs    # String helper methods
```

## Key Dependencies

- **Microsoft.CodeAnalysis.CSharp** - Roslyn compiler APIs for C# syntax analysis
- **DotLiquid** - Template rendering engine
- **MediatR** - Mediator pattern implementation
- **AutoMapper** - Object-to-object mapping
- **Humanizer** - String humanization utilities
- **LibGit2Sharp** - Git repository operations
- **Newtonsoft.Json** - JSON serialization
- **System.Reactive** - Reactive extensions support

## Usage

This library is typically not used directly but is referenced by other Endpoint modules. To use Endpoint in your own code generator:

```csharp
using Endpoint;
using Endpoint.Services;
using Endpoint.Artifacts;

// Register services
services.AddEndpointServices();
```

## Target Framework

- .NET 9.0

## License

This project is licensed under the MIT License.
