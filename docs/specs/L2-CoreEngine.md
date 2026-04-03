# L2 Requirements — CodeGenerator Core Engine

**Parent:** [L1-CodeGenerator.md](L1-CodeGenerator.md) — FR-01, FR-13, FR-14, FR-18, NFR-01, NFR-02
**Status:** Reverse-engineered from source code
**Date:** 2026-04-03

---

## FR-01: Core Generation Engine

### FR-01.1: Artifact Generation Dispatch

The `IArtifactGenerator` shall accept any object model and dispatch to the appropriate `IArtifactGenerationStrategy<T>` implementation.

**Acceptance Criteria:**
- GIVEN a model of type T passed to `GenerateAsync(object model)`, WHEN one or more `IArtifactGenerationStrategy<T>` implementations are registered, THEN the strategy with the highest `GetPriority()` value whose `CanHandle()` returns true is selected and invoked.
- GIVEN a model type that has been dispatched before, WHEN the same type is dispatched again, THEN the cached strategy wrapper is reused from the `ConcurrentDictionary<Type, WrapperBase>`.
- GIVEN no registered strategy can handle the model, WHEN `GenerateAsync` is called, THEN an appropriate exception is thrown.

### FR-01.2: Syntax Generation Dispatch

The `ISyntaxGenerator` shall accept a typed model and return a generated code string via the appropriate `ISyntaxGenerationStrategy<T>`.

**Acceptance Criteria:**
- GIVEN a model of type T passed to `GenerateAsync<T>(T model)`, WHEN one or more `ISyntaxGenerationStrategy<T>` implementations are registered, THEN the highest-priority strategy whose `CanHandle()` returns true is invoked and its string result returned.
- GIVEN a model type dispatched previously, WHEN the same type is dispatched again, THEN the cached wrapper is reused.

### FR-01.3: Strategy Auto-Discovery

The framework shall automatically discover and register all `IArtifactGenerationStrategy<T>` and `ISyntaxGenerationStrategy<T>` implementations from a provided assembly.

**Acceptance Criteria:**
- GIVEN an assembly containing non-abstract types implementing `IArtifactGenerationStrategy<T>`, WHEN `AddCoreServices(assembly)` is called, THEN each implementation is registered as a singleton for its generic interface.
- GIVEN an assembly containing non-abstract types implementing `ISyntaxGenerationStrategy<T>`, WHEN `AddCoreServices(assembly)` is called, THEN each implementation is registered as a singleton for its generic interface.
- GIVEN a type implementing multiple generic interfaces (e.g., `IArtifactGenerationStrategy<A>` and `IArtifactGenerationStrategy<B>`), WHEN registered, THEN it is registered for each interface separately.

### FR-01.4: Priority-Based Strategy Selection

When multiple strategies can handle the same model type, the strategy with the highest priority value shall be selected.

**Acceptance Criteria:**
- GIVEN two strategies A (priority 1) and B (priority 2) both handling type T, WHEN dispatching type T, THEN strategy B is selected.
- GIVEN a strategy with default priority (1), WHEN no higher-priority strategy exists for the type, THEN the default-priority strategy is selected.

### FR-01.5: Artifact Model Hierarchy

Artifact models shall support parent-child hierarchies with recursive descendant traversal.

**Acceptance Criteria:**
- GIVEN a `FileModel` with Name, Directory, Extension, and Body, WHEN `Path` is accessed, THEN it returns the combined directory + name + extension path.
- GIVEN a `ContentFileModel`, WHEN created with content, THEN the content is available as init-only property.
- GIVEN a `TemplatedFileModel`, WHEN created with template and tokens, THEN both are available as init-only properties.
- GIVEN an `ArtifactModel` tree, WHEN `GetDescendants()` is called, THEN all recursive children are collected.

---

## FR-13: Template Engine

### FR-13.1: DotLiquid Template Rendering

The `ITemplateProcessor` shall render DotLiquid templates with token substitution.

**Acceptance Criteria:**
- GIVEN a template string with `{{ variable }}` placeholders and a token dictionary, WHEN `Process(template, tokens)` is called, THEN all placeholders are replaced with corresponding token values.
- GIVEN a dynamic model object, WHEN `Process(template, model)` is called, THEN the model's properties are converted to a dictionary via reflection and used for rendering.
- GIVEN `ignoreTokens` array is provided, WHEN processing, THEN the specified tokens are excluded from the dictionary before rendering.

### FR-13.2: Token Building with Naming Variants

The `TokensBuilder` shall generate multiple naming convention variants for each token.

**Acceptance Criteria:**
- GIVEN a property name and token value, WHEN `ToTokens()` is called, THEN at minimum the following variants are produced: PascalCase, PascalCasePlural, CamelCase, CamelCasePlural, SnakeCase, SnakeCasePlural, TitleCase.
- GIVEN a List-type property in a dynamic model, WHEN converting to dictionary, THEN each list item is recursively converted.

### FR-13.3: Embedded Resource Template Location

The `EmbeddedResourceTemplateLocatorBase<T>` shall locate templates embedded in assemblies.

**Acceptance Criteria:**
- GIVEN a filename, WHEN `Get(filename)` is called, THEN the matching embedded resource content is returned as a string.
- GIVEN a filename matching a resource in the assembly of type T, WHEN located, THEN the resource stream is read and returned.

---

## FR-14: Naming Convention Conversion

### FR-14.1: Convention Conversion

The `INamingConventionConverter` shall convert identifiers between all supported naming conventions.

**Acceptance Criteria:**
- GIVEN "MyClassName" (PascalCase), WHEN converting to CamelCase, THEN "myClassName" is returned.
- GIVEN "MyClassName" (PascalCase), WHEN converting to snake_case, THEN "my-class-name" is returned.
- GIVEN "MyClassName" (PascalCase), WHEN converting to kebab-case, THEN "my-class-name" is returned.
- GIVEN "MyClassName" (PascalCase), WHEN converting to TitleCase, THEN "My Class Name" is returned.
- GIVEN "my_class_name" (snake_case), WHEN converting to PascalCase, THEN "MyClassName" is returned.

### FR-14.2: Convention Auto-Detection

The converter shall automatically detect the naming convention of an input string.

**Acceptance Criteria:**
- GIVEN "myClassName" (starts lowercase, no separators), WHEN `GetNamingConvention` is called, THEN CamelCase is returned.
- GIVEN "MyClassName" (starts uppercase, no separators), WHEN detected, THEN PascalCase is returned.
- GIVEN "My Class Name" (has spaces, starts uppercase), WHEN detected, THEN TitleCase is returned.
- GIVEN "my_class_name" (no spaces/dashes, no uppercase), WHEN detected, THEN SnakeCase is returned.

### FR-14.3: Pluralization

The converter shall pluralize identifiers using the Humanizer library.

**Acceptance Criteria:**
- GIVEN "User" and `pluralize=true`, WHEN converting, THEN "Users" is returned (in the target convention).
- GIVEN "Category" and `pluralize=true`, WHEN converting, THEN "Categories" is returned.

---

## FR-18: Command Execution

### FR-18.1: Cross-Platform Command Execution

The `ICommandService` shall execute shell commands on both Windows and Unix.

**Acceptance Criteria:**
- GIVEN a command string on Windows, WHEN `Start(command, workingDirectory)` is called, THEN the command is executed via `cmd.exe /c`.
- GIVEN a command string on Unix, WHEN `Start(command, workingDirectory)` is called, THEN the command is executed via `/bin/bash -c`.
- GIVEN `waitForExit=true`, WHEN the command is executed, THEN the method blocks until the process exits and returns the exit code.
- GIVEN a `NoOpCommandService`, WHEN any command is executed, THEN no process is started (for testing).
