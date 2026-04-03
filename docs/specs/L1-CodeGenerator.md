# L1 Requirements — CodeGenerator Framework

**Status:** Reverse-engineered from source code
**Date:** 2026-04-03
**Author:** Reverse-engineered by Copilot

---

## 1. Overview

CodeGenerator is a comprehensive, plugin-based code generation framework for .NET that produces clean architecture solutions, APIs, full-stack applications, mobile apps, and end-to-end tests across multiple technology stacks. It consumes models, PlantUML diagrams, or code analysis results and emits source files, project scaffolds, and complete solution structures.

## 2. Goals

- Provide a single extensible framework for generating code across .NET, Python, Angular, React, React Native, Flask, Playwright, and Detox.
- Support plugin-based strategy architecture with automatic discovery so new generators require zero configuration.
- Enable model-driven code generation at three levels: syntax (code strings), files (templated artifacts), and projects/solutions (full scaffolding).
- Support diagram-to-code workflows via PlantUML parsing.
- Provide a CLI tool for bootstrapping new code generator projects.

## 3. Functional Requirements

### FR-01: Core Generation Engine

The framework shall provide a core engine with strategy-based dispatch for both artifact generation (files, projects, solutions) and syntax generation (code strings). Strategies are automatically discovered via assembly scanning and selected at runtime by model type and priority.

### FR-02: .NET Code Generation

The framework shall generate C# code including classes, interfaces, records, controllers, minimal API route handlers, CQRS commands/queries/handlers, DDD entities/aggregates, EF Core DbContext, FluentValidation validators, SpecFlow BDD tests, and unit tests.

### FR-03: Solution Scaffolding

The framework shall scaffold complete .NET solutions in multiple architectural patterns: Minimal API, Clean Architecture Microservice (Domain/Application/Infrastructure/API layers), and DDD-based solutions, including project files, dependencies, and DI configuration.

### FR-04: Python Code Generation

The framework shall generate Python syntax including classes, functions, methods, decorators, type hints, imports, and complete modules. It shall scaffold Python projects with virtual environments, requirements files, and package installation.

### FR-05: Flask Backend Generation

The framework shall generate Flask applications with app factories, configuration classes, Blueprint controllers, SQLAlchemy models, repositories, services, Marshmallow schemas, and middleware decorators. It shall scaffold complete Flask project structures with optional features (auth, CORS, Celery, migrations).

### FR-06: Angular Generation

The framework shall generate Angular workspaces with Jest testing (replacing Karma), standalone components, TypeScript types and functions, barrel index files, and build configurations. It shall support both application and library project types.

### FR-07: React Generation

The framework shall generate React + TypeScript + Vite workspaces with functional components (including forwardRef and "use client"), custom hooks, Zustand stores, Axios-based API clients, TypeScript interfaces with inheritance, and barrel index files.

### FR-08: React Native Generation

The framework shall generate React Native projects with screens (SafeAreaView), components, React Navigation (Stack/Tab/Drawer), Zustand stores, StyleSheet generation, and testID attributes for testing.

### FR-09: Playwright Test Generation

The framework shall generate Playwright test projects with page object models (supporting GetByTestId, GetByRole, GetByLabel, and CSS locator strategies), test specs with Arrange-Act-Assert pattern, custom fixtures, multi-browser configuration, and a base page class.

### FR-10: Detox Test Generation

The framework shall generate Detox mobile test projects with page object models using testID selectors, Jest test specs, `.detoxrc.js` configuration for iOS/Android, Jest configuration with ts-jest, and a base page class with mobile interaction helpers.

### FR-11: PlantUML Parsing

The framework shall parse PlantUML diagrams (class diagrams, sequence diagrams) into solution models, supporting classes with visibility, properties, methods, enums, relationships (inheritance, composition, aggregation, association), packages, and components. It shall transform parsed diagrams into code generation models.

### FR-12: Roslyn Code Analysis

The framework shall integrate with Roslyn/Microsoft.CodeAnalysis to open and analyze .csproj files, inspect solution structures, and detect installed NuGet/npm packages.

### FR-13: Template Engine

The framework shall provide a DotLiquid-based template processing engine with token substitution, supporting automatic naming convention variants (PascalCase, camelCase, snake_case, kebab-case, TitleCase, AllCaps) with pluralization for each token.

### FR-14: Naming Convention Conversion

The framework shall convert identifiers between PascalCase, camelCase, snake_case, kebab-case, TitleCase, and AllCaps conventions, with automatic convention detection and Humanizer-based pluralization/singularization.

### FR-15: CLI Tool

The framework shall provide a global .NET CLI tool (`create-code-cli`) that scaffolds new code generator projects with a DI-configured program, sample commands, install scripts, and optional Claude skill documentation. It shall support target framework selection and .slnx solution format.

### FR-16: Git Integration

The framework shall integrate with Git via LibGit2Sharp and GitHub via Octokit for automated pull request creation, merging, and branch management.

### FR-17: Dependency Injection Automation

The framework shall automatically generate and update `ConfigureServices.cs` files with service registrations (Singleton, Transient, Scoped) and hosted service registrations, parsing existing files to prevent duplicates.

### FR-18: Command Execution

The framework shall execute shell commands cross-platform (Windows/Unix) for operations like `dotnet new`, `npm init`, `pip install`, and other CLI tools used during project scaffolding.

### FR-19: YAML-Driven Codebase Scaffolding

The CLI shall provide a command that scaffolds any codebase from a comprehensive YAML configuration file. The YAML schema shall be technology-agnostic, supporting .NET, Python, Angular, React, SPA (with Playwright Page Object Model testing), Flask, and any other technology stack. The command shall generate the complete initial project structure—directories, files, project configurations, dependency manifests, and boilerplate code—without business logic. The intent is that a user defines requirements, creates a detailed software design, scaffolds the initial codebase via this command, and then fills in specific logic, minimizing token usage when working with AI assistants. The configuration schema shall be comprehensive enough to describe complex multi-project codebases such as those found in production repositories.

## 4. Non-Functional Requirements

### NFR-01: Extensibility

New generation strategies must be addable by implementing `IArtifactGenerationStrategy<T>` or `ISyntaxGenerationStrategy<T>` and placing the implementation in a scanned assembly. No manual registration required.

### NFR-02: Performance

The framework shall use thread-safe caching (ConcurrentDictionary for strategy wrappers, ThreadStatic StringBuilderCache) to minimize allocations and reflection overhead during generation.

### NFR-03: Modularity

Each technology stack (DotNet, Python, Angular, React, ReactNative, Flask, Playwright, Detox) shall be packaged as an independent NuGet package with its own service registration extension method.

### NFR-04: Target Frameworks

The framework shall target .NET 8.0 and .NET 9.0.

## 5. Package Structure

| Package | Scope |
|---------|-------|
| `CodeGenerator.Core` | Core abstractions, artifact/syntax engines, template processing, naming conventions, services |
| `CodeGenerator.DotNet` | C# syntax, .NET solutions, CQRS, DDD, PlantUML, Roslyn, Git, SignalR |
| `CodeGenerator.Python` | Python syntax, virtual environments, project scaffolding |
| `CodeGenerator.Angular` | Angular workspaces, projects, Jest configuration |
| `CodeGenerator.React` | React + Vite + TypeScript, components, hooks, stores, API clients |
| `CodeGenerator.ReactNative` | React Native screens, components, navigation, styles |
| `CodeGenerator.Flask` | Flask app factories, controllers, models, repositories, schemas |
| `CodeGenerator.Playwright` | Playwright page objects, test specs, fixtures, configuration |
| `CodeGenerator.Detox` | Detox page objects, test specs, configuration |
| `CodeGenerator.Cli` | CLI tool for project bootstrapping |
