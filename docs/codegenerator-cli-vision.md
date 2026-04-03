# CodeGenerator.Cli: Path to 10/10 and Beyond

## Part 1: Making CodeGenerator.Cli 10/10

### 1.1 Bulletproof Error Handling

Every file I/O operation, shell command, and generation step should be wrapped in structured
error handling that gives users actionable feedback.

```csharp
// Current: silent failure
commandService.Start($"dotnet new sln -n {name}", directory);

// Target: structured result with recovery guidance
var result = commandService.Start($"dotnet new sln -n {name}", directory);
if (result != 0)
{
    logger.LogError("Failed to create solution '{Name}'. Exit code: {Code}", name, result);
    logger.LogError("Ensure the .NET SDK is installed: dotnet --version");
    return 1;
}
```

**Specific improvements:**
- Wrap `HandleAsync` in try/catch with categorized exceptions (IO, process, validation, template)
- Validate output directory is writable before starting generation
- Validate solution names follow C# identifier rules
- Check for existing output and prompt before overwriting
- Rollback partial generation on failure (delete created files/directories)
- Return meaningful exit codes (0 = success, 1 = validation error, 2 = IO error, 3 = process error)

### 1.2 Input Validation Layer

Add a validation step before any generation begins.

```csharp
public class GenerationOptionsValidator
{
    public ValidationResult Validate(string name, string output, string framework)
    {
        var result = new ValidationResult();

        if (!Regex.IsMatch(name, @"^[A-Za-z_][A-Za-z0-9_.]*$"))
            result.AddError("name", $"'{name}' is not a valid C# identifier");

        if (!Directory.Exists(Path.GetDirectoryName(output)))
            result.AddError("output", $"Parent directory does not exist: {output}");

        if (!framework.StartsWith("net"))
            result.AddError("framework", $"Unrecognized framework: {framework}");

        return result;
    }
}
```

### 1.3 Rich Console Output

Replace bare `ILogger` calls with structured, visually clear output using
[Spectre.Console](https://spectreconsole.net/) or similar.

**Generation progress:**
```
Creating solution: ContosoErp
  [1/6] Solution file ............... done
  [2/6] CLI project ................. done
  [3/6] AppRootCommand .............. done
  [4/6] HelloWorldCommand ........... done
  [5/6] EnterpriseSolutionCommand ... done
  [6/6] install-cli.bat ............. done

  Generated 6 files in ./ContosoErp (12.4 KB)

Next steps:
  cd ContosoErp
  dotnet build
  dotnet run --project src/ContosoErp.Cli -- hello -o ./output
```

**Error output:**
```
ERROR: Solution name 'my-bad-name' contains invalid characters
  Solution names must be valid C# identifiers: letters, digits, underscores, dots
  Example: dotnet run -- -n MyProject -o ./output
```

**Tree view of generated output:**
```
ContosoErp/
  src/
    ContosoErp.Cli/
      Commands/
        AppRootCommand.cs
        HelloWorldCommand.cs
        EnterpriseSolutionCommand.cs
      Program.cs
      ContosoErp.Cli.csproj
  ContosoErp.sln
  install-cli.bat
```

### 1.4 Comprehensive Test Suite

Target: 90%+ code coverage on CLI-specific code.

**Unit tests:**
- Option parsing (all flags, defaults, aliases, required validation)
- `GenerateProgramContent()`, `GenerateCliProjectContent()` and other template methods
  produce valid C# (parse with Roslyn to verify)
- Input validation logic
- Error handling paths

**Integration tests:**
- End-to-end: run command, verify directory structure, verify file contents
- Build the generated project with `dotnet build` to confirm it compiles
- Run the generated CLI's `--help` to verify it executes
- Overwrite scenarios (existing directory)
- Invalid input scenarios

**Test infrastructure:**
- Use `System.IO.Abstractions.TestingHelpers` (already a dependency) for file system mocking
- Use `DryRunCommandService` for command capture
- Use `System.CommandLine.Testing` for CLI invocation testing

### 1.5 Interactive Mode

When invoked without required arguments, enter an interactive questionnaire.

```
$ create-code-cli

  What is the name of your solution? ContosoErp
  Output directory [.]: ./output
  Target framework [net9.0]:
  Solution format: (1) .sln  (2) .slnx [1]:
  Use local CodeGenerator source? [n]:

  Creating solution: ContosoErp ...
```

Use `System.CommandLine`'s middleware or a library like `Spectre.Console` prompts. Fall back to
non-interactive when stdin is not a terminal (piped/CI).

### 1.6 Dry-Run and Preview

Add `--dry-run` and `--what-if` flags that show what would be generated without writing files.

```
$ create-code-cli -n ContosoErp --dry-run

  Would generate 6 files (12.4 KB):
    ContosoErp.sln
    src/ContosoErp.Cli/ContosoErp.Cli.csproj
    src/ContosoErp.Cli/Program.cs
    src/ContosoErp.Cli/Commands/AppRootCommand.cs
    src/ContosoErp.Cli/Commands/HelloWorldCommand.cs
    src/ContosoErp.Cli/Commands/EnterpriseSolutionCommand.cs
    install-cli.bat

  Would run 2 commands:
    dotnet new sln -n ContosoErp
    dotnet sln add src/ContosoErp.Cli/ContosoErp.Cli.csproj
```

This leverages the existing `GenerationContext.DryRun` and `GenerationResult` infrastructure
that already exists in CodeGenerator.Core but is not wired into the CLI.

### 1.7 Configuration File Support

Support a `.codegenerator.json` project-level config that sets defaults.

```json
{
  "defaults": {
    "framework": "net9.0",
    "solutionFormat": "slnx",
    "output": "./generated"
  },
  "templates": {
    "author": "Contoso Team",
    "license": "MIT"
  }
}
```

Resolution order: CLI args > `.codegenerator.json` > environment variables > built-in defaults.

### 1.8 Extract Embedded Templates

Move the 500+ lines of embedded string templates out of `CreateCodeGeneratorCommand.cs` and
`InstallCommand.cs` into proper embedded resource files.

```
Templates/
  Program.cs.liquid
  AppRootCommand.cs.liquid
  HelloWorldCommand.cs.liquid
  EnterpriseSolutionCommand.cs.liquid
  CliProject.csproj.liquid
  install-cli.bat.liquid
  SKILL.md
```

Benefits:
- Templates are syntax-highlighted in editors
- Easier to review and maintain
- Can be overridden by users via a `--templates` directory flag
- Leverages the existing `ITemplateProcessor` / `LiquidTemplateProcessor` pipeline

### 1.9 Version Consistency

The current templates embed inconsistent package versions (1.2.0, 1.2.1, 1.2.2, 1.2.5).
Centralize version numbers.

```csharp
public static class PackageVersions
{
    public const string Core = "1.2.5";
    public const string DotNet = "1.2.5";
    public const string Angular = "1.2.5";
    // ...
}
```

Or better: read the version from the assembly metadata at runtime so templates always reference
the version of CodeGenerator that generated them.

### 1.10 Shell Completion

Register shell completions for bash, zsh, PowerShell, and fish. System.CommandLine supports
this natively via `dotnet-suggest`.

```bash
# After installation:
create-code-cli <TAB>
  --name        --output      --framework   --slnx        install

create-code-cli --framework <TAB>
  net8.0   net9.0   net10.0
```

### 1.11 Post-Generation Verification

After generating a project, optionally verify it compiles.

```
$ create-code-cli -n ContosoErp --verify

  Creating solution: ContosoErp ... done
  Verifying: dotnet build ... success (0 warnings, 0 errors)
  Verifying: dotnet run -- --help ... success

  Solution is ready.
```

Add `--verify` flag that runs `dotnet build` and basic smoke tests on the generated output.

### 1.12 Telemetry and Diagnostics

Add optional `--diagnostics` flag that dumps generation timing and system info for bug reports.

```
$ create-code-cli -n Test --diagnostics

  Environment:
    CodeGenerator.Cli 1.2.0
    .NET SDK 9.0.100
    OS: Windows 11 (10.0.26200)
    Shell: bash

  Timing:
    Solution creation:  120ms
    Project generation: 340ms
    File generation:    180ms
    dotnet sln add:     890ms
    Total:             1530ms
```

### 1.13 Plugin Discovery for Commands

Allow external assemblies to contribute commands via a well-known interface.

```csharp
public interface ICliPlugin
{
    string Name { get; }
    Command CreateCommand(IServiceProvider serviceProvider);
}
```

Discovery via:
- NuGet packages with a `CodeGenerator.Cli.Plugin` convention
- Assemblies in a `plugins/` directory
- `--plugin` flag pointing to a DLL

This lets the community extend the CLI without forking.

---

## Part 2: AGI-Unlocked Features

When AGI-class models arrive, they will be able to reason about entire codebases, understand
intent from minimal descriptions, and orchestrate complex multi-step generation workflows.
The following features become viable and would result in massive token savings.

### 2.1 Natural Language to Model Compiler

**Today:** Agents must construct detailed builder chains or model objects to generate code.
A simple CRUD entity requires 20-40 lines of builder calls.

**With AGI:** A single natural language sentence compiles to a complete model graph.

```
Agent prompt: "Create a blog platform with posts, comments, and tags"

AGI resolves to:
- 3 entities with relationships (Post hasMany Comments, Post manyToMany Tags)
- Full CRUD for each entity
- Authentication and authorization
- API controllers, services, repositories
- React frontend with routing
- Database migrations
```

**Token savings:** ~50 agent tokens replace ~2,000 tokens of explicit builder calls. The AGI
model understands domain conventions (blog posts have titles, bodies, publish dates) without
being told.

**Implementation:** Add an `INaturalLanguageCompiler` service that accepts a description string
and returns a fully populated `FullStackCreateOptions` or `SolutionModel`. The compiler uses
the AGI model's world knowledge to infer entity properties, relationships, validation rules,
and UI patterns.

```csharp
public interface INaturalLanguageCompiler
{
    Task<SolutionModel> CompileAsync(string description, CompilationOptions options);
}
```

### 2.2 Codebase-Aware Incremental Generation

**Today:** CodeGenerator generates from scratch or uses basic `AddFileModel` with
`ConflictBehavior`. It cannot understand existing code well enough to merge intelligently.

**With AGI:** The generator reads an existing codebase, understands its patterns, conventions,
and architecture, then generates code that perfectly fits the existing style.

```
Agent: "Add a notification system to this project"

AGI analyzes:
- Existing project uses repository pattern with Unit of Work
- Controllers follow a specific error handling pattern
- Frontend uses a custom hook pattern for API calls
- Tests follow arrange-act-assert with a specific mock setup
- Database uses Fluent API configuration, not data annotations

AGI generates:
- NotificationRepository matching existing repository style
- NotificationController matching existing error handling and response patterns
- useNotifications hook matching existing hook conventions
- NotificationTests matching existing test setup patterns
- NotificationConfiguration matching existing Fluent API style
```

**Token savings:** Instead of the agent specifying every convention explicitly (1,000+ tokens
of style instructions), the AGI infers all conventions from the existing code (0 extra tokens).

**Implementation:** Extend `IProjectContext` with deep analysis capabilities.

```csharp
public interface IProjectAnalyzer
{
    Task<ProjectConventions> AnalyzeAsync(string projectDirectory);
    Task<StyleGuide> InferStyleAsync(string projectDirectory);
    Task<ArchitectureModel> InferArchitectureAsync(string projectDirectory);
}

public interface IConventionAwareGenerator
{
    Task GenerateAsync(object model, ProjectConventions conventions);
}
```

### 2.3 Intent-Preserving Refactoring

**Today:** Refactoring requires the agent to understand the codebase, plan changes across
multiple files, and generate each change individually. This is extremely token-expensive.

**With AGI:** Express refactoring intent once; the generator applies it across the entire
codebase.

```
Agent: "Convert this monolith to clean architecture"

CodeGenerator with AGI:
1. Analyzes all classes, dependencies, and data flows
2. Identifies bounded contexts from usage patterns
3. Creates Domain, Application, Infrastructure, API layers
4. Moves classes to correct layers
5. Introduces interfaces at layer boundaries
6. Creates DI registrations
7. Updates all references and imports
8. Generates integration tests for new boundaries
```

**Token savings:** A refactoring that would require 50,000+ tokens of agent reasoning and
individual file edits becomes a single model operation (~100 tokens).

**Implementation:**

```csharp
public interface IRefactoringEngine
{
    Task<RefactoringPlan> PlanAsync(string intent, string projectDirectory);
    Task<GenerationResult> ExecuteAsync(RefactoringPlan plan);
}
```

### 2.4 Semantic Diff and Merge

**Today:** `ConflictBehavior` offers Skip, Overwrite, or Error. No intelligent merging.

**With AGI:** The generator understands the semantic meaning of code and can merge generated
code with hand-written code intelligently.

```
Scenario: Developer hand-modified a generated controller to add custom caching logic.
Regeneration needs to add a new endpoint.

AGI-powered merge:
- Identifies hand-written caching logic as intentional customization
- Preserves all custom code in its exact location
- Adds the new endpoint in the correct position
- Updates DI registrations without disturbing existing ones
- If the caching pattern should apply to the new endpoint, applies it
```

**Token savings:** Eliminates the "regenerate and manually re-apply customizations" cycle
that currently costs thousands of agent tokens per iteration.

**Implementation:**

```csharp
public enum MergeBehavior
{
    Skip,
    Overwrite,
    SemanticMerge  // AGI-powered
}

public interface ISemanticMergeService
{
    Task<string> MergeAsync(string existingCode, string generatedCode, MergeContext context);
}
```

### 2.5 Specification-to-Solution Pipeline

**Today:** An agent must decompose a specification into dozens of individual generation calls,
managing the dependency order, cross-references, and consistency manually.

**With AGI:** Feed an entire product specification and get a complete, consistent solution.

```
Input: A 10-page product requirements document (PRD)

AGI-powered CodeGenerator:
1. Parses PRD into domain model (entities, use cases, constraints)
2. Infers architecture (microservices vs monolith, sync vs async)
3. Generates complete solution:
   - Domain models with validation
   - Application layer (CQRS commands/queries/handlers)
   - Infrastructure (repositories, external service clients)
   - API (controllers, middleware, authentication)
   - Frontend (pages, components, state management, routing)
   - Tests (unit, integration, E2E)
   - Infrastructure-as-code (Docker, CI/CD, cloud resources)
   - Documentation (API docs, architecture diagrams, runbooks)
```

**Token savings:** A PRD that would require 100,000+ agent tokens to implement manually
becomes a single pipeline invocation. The AGI handles all the decomposition, consistency
checking, and cross-referencing internally.

**Implementation:**

```csharp
public interface ISpecificationCompiler
{
    Task<SolutionModel> CompileAsync(string specification, ArchitecturePreferences preferences);
    Task<SolutionModel> CompileAsync(Stream document, ArchitecturePreferences preferences);
}
```

### 2.6 Self-Healing Code Generation

**Today:** If generated code has bugs, the agent must diagnose the issue, understand the
generator's output, and manually fix it -- often regenerating entirely.

**With AGI:** The generator validates its own output and iterates until correct.

```
Generation loop:
1. Generate code from model
2. Compile and run tests
3. If errors: AGI analyzes errors, adjusts model/templates, regenerates
4. Repeat until all tests pass and code compiles cleanly

Agent sees: a single call that returns working code, every time.
```

**Token savings:** Eliminates the debug-fix-regenerate cycle. What currently takes 5-10
agent turns (5,000-20,000 tokens) becomes a single call. The iteration happens inside
CodeGenerator, invisible to the agent.

**Implementation:**

```csharp
public interface ISelfHealingGenerator
{
    Task<GenerationResult> GenerateAndVerifyAsync(
        object model,
        VerificationOptions options,
        int maxIterations = 5);
}

public class VerificationOptions
{
    public bool CompileCheck { get; set; } = true;
    public bool RunTests { get; set; } = true;
    public bool LintCheck { get; set; } = true;
    public string[] TestCommands { get; set; }
}
```

### 2.7 Cross-Project Dependency Graph Generation

**Today:** Generating a microservices architecture requires the agent to manually coordinate
shared contracts, API versions, message schemas, and deployment dependencies across services.

**With AGI:** Describe the system topology once; CodeGenerator generates all services with
consistent contracts, versioning, and deployment configuration.

```
Agent: "E-commerce platform with order, inventory, payment, and notification services
        communicating via events"

AGI generates:
- Shared event contracts (CloudEvents/AsyncAPI) with schema versioning
- Order service (publishes OrderPlaced, OrderCancelled)
- Inventory service (subscribes to OrderPlaced, publishes InventoryReserved)
- Payment service (subscribes to InventoryReserved, publishes PaymentProcessed)
- Notification service (subscribes to all events, sends emails/push)
- API gateway with routing and auth
- Docker Compose for local development
- Kubernetes manifests for deployment
- Saga orchestrator for distributed transactions
- Circuit breakers and retry policies
- Distributed tracing correlation
```

**Token savings:** Coordinating 4+ services manually requires 50,000+ tokens. A single
topology description (~200 tokens) replaces it all.

**Implementation:**

```csharp
public interface IDistributedSystemGenerator
{
    Task<DistributedSystemModel> GenerateAsync(
        SystemTopology topology,
        CommunicationPattern pattern,
        DeploymentTarget target);
}

public class SystemTopology
{
    public List<ServiceDefinition> Services { get; set; }
    public List<EventFlow> EventFlows { get; set; }
    public List<SyncDependency> SyncDependencies { get; set; }
}
```

### 2.8 Live Architecture Optimization

**Today:** Architecture decisions are made upfront and baked into the generated code. Changing
architecture requires regeneration.

**With AGI:** CodeGenerator monitors runtime behavior and suggests or applies architectural
improvements.

```
AGI observes (from logs, metrics, traces):
- OrderService.GetOrderHistory() has P99 latency of 2.3s
- The query joins 5 tables and is called 10,000 times/day
- Read/write ratio is 100:1

AGI recommends and generates:
- CQRS read model for order history (denormalized, single table)
- Event handler to maintain the read model
- Migration script
- Updated OrderService query (now 12ms P99)
- Performance test to verify improvement
```

**Token savings:** Performance optimization typically requires extensive profiling,
analysis, and multi-file refactoring (10,000+ agent tokens). Automated generation from
runtime data reduces this to a single confirmation.

### 2.9 Multi-Modal Input (Diagrams, Wireframes, Conversations)

**Today:** CodeGenerator accepts structured models built via code. PlantUML parsing is the
closest thing to visual input.

**With AGI:** Accept any input modality and generate complete implementations.

```
Inputs that become generation sources:
- Whiteboard photo of system architecture -> microservices solution
- Figma export / wireframe screenshot -> React components with exact layout
- Slack conversation about a feature -> implementation with tests
- Database schema screenshot -> entities, repositories, API, and frontend
- API response JSON sample -> TypeScript interfaces, API client, and Zustand store
- Voice description -> complete feature implementation
```

**Token savings:** Converting visual/informal inputs to structured models currently requires
thousands of tokens of agent interpretation. AGI does this conversion internally, so the
agent just passes the raw input.

**Implementation:**

```csharp
public interface IMultiModalCompiler
{
    Task<SolutionModel> CompileAsync(MultiModalInput input);
}

public class MultiModalInput
{
    public List<InputSource> Sources { get; set; }
}

public class InputSource
{
    public InputType Type { get; set; } // Image, Text, Audio, Document, Schema, Api
    public Stream Content { get; set; }
    public string Description { get; set; }
}
```

### 2.10 Autonomous Test Generation with Behavioral Coverage

**Today:** Test generation produces structural tests (method exists, returns correct type).
The agent must specify behavioral test cases manually.

**With AGI:** The generator reasons about the domain and produces tests that cover meaningful
behavioral scenarios, edge cases, and failure modes.

```
For an OrderService, AGI generates:
- Happy path: create order, verify total, confirm inventory reserved
- Concurrency: two orders for last item in stock, one must fail gracefully
- Boundary: order with 0 items, order with 10,000 items, order with negative price
- Integration: payment gateway timeout, retry, and eventual consistency
- Security: user A cannot view user B's orders
- Performance: 1000 concurrent order submissions complete within SLA
- Regression: historical bugs from git history inform edge case tests
```

**Token savings:** Writing comprehensive behavioral tests typically requires 5,000-10,000
tokens per service. AGI generates them from the domain model alone.

### 2.11 Token Budget-Aware Generation

**Today:** CodeGenerator generates the same output regardless of how it's being used. An
agent with 4K remaining tokens and an agent with 100K remaining tokens get the same treatment.

**With AGI:** CodeGenerator adapts its interface based on the calling agent's constraints.

```
Low token budget (< 1K remaining):
  Agent: generate("blog")
  -> AGI infers everything, generates complete app, returns summary

Medium token budget (10K remaining):
  Agent: generate("blog", { entities: ["Post", "Comment"] })
  -> AGI infers properties and relationships, agent reviews key decisions

High token budget (100K remaining):
  Agent: generate(detailedModel)
  -> Traditional generation with full agent control over every detail
```

**Implementation:**

```csharp
public interface IAdaptiveGenerator
{
    Task<GenerationResult> GenerateAsync(
        object modelOrDescription,
        TokenBudget budget,
        AgentCapabilities capabilities);
}

public class TokenBudget
{
    public int RemainingTokens { get; set; }
    public GenerationVerbosity PreferredVerbosity { get; set; } // Minimal, Standard, Detailed
}
```

### 2.12 Continuous Codebase Synchronization

**Today:** Generation is a one-shot operation. After generation, the code diverges from the
model as developers make changes.

**With AGI:** CodeGenerator maintains a bidirectional link between models and code. Changes
in either direction propagate automatically.

```
Developer adds a new field to an entity in code:
  -> Model automatically updated
  -> Migration generated
  -> API endpoint updated
  -> Frontend form updated
  -> Tests updated

Product owner updates a requirement in the spec:
  -> Affected models identified
  -> Code changes generated as a PR
  -> Tests updated and verified
  -> Changelog entry created
```

**Token savings:** Keeping generated code in sync with evolving requirements currently
requires repeated full-context agent conversations (50,000+ tokens each). Continuous
synchronization reduces this to zero -- changes propagate automatically.

**Implementation:**

```csharp
public interface ICodebaseSynchronizer
{
    Task<SyncResult> SyncModelToCodeAsync(SolutionModel model, string projectDirectory);
    Task<SolutionModel> SyncCodeToModelAsync(string projectDirectory);
    FileSystemWatcher Watch(string projectDirectory, Action<SyncEvent> onChange);
}
```

---

## Summary: Token Savings Potential

| Feature | Current Agent Cost | With AGI | Savings |
|---------|-------------------|----------|---------|
| CRUD entity generation | 2,000 tokens | 50 tokens | 40x |
| Full-stack application | 20,000 tokens | 200 tokens | 100x |
| Codebase-aware additions | 5,000 tokens | 100 tokens | 50x |
| Architectural refactoring | 50,000 tokens | 100 tokens | 500x |
| Specification to solution | 100,000+ tokens | 500 tokens | 200x |
| Debug-fix-regenerate cycle | 20,000 tokens | 0 tokens (self-healing) | infinite |
| Multi-service systems | 50,000+ tokens | 200 tokens | 250x |
| Behavioral test suites | 10,000 tokens | 50 tokens | 200x |

The core insight: CodeGenerator already provides the scaffolding and strategy infrastructure.
AGI transforms it from a tool that agents drive step-by-step into a tool that agents point at
a problem and let run. The token savings come not from generating code faster, but from
eliminating the need for the agent to think about how to generate code at all.
