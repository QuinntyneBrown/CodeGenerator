# Production-Grade Error Handling Plan

## 1. Current State Assessment

### What Exists Today

| Layer | Mechanism | Status |
|-------|-----------|--------|
| **Custom Exception Hierarchy** | `CliException` base with `CliValidationException`, `CliIOException`, `CliProcessException`, `CliTemplateException` | Solid foundation |
| **Exit Codes** | `CliExitCodes` static class (0-4, 99) | Covers main categories |
| **Validation Framework** | `ValidationResult` / `ValidationError` / `IValidatable` / `ValidationSeverity` | Well-designed |
| **Result Objects** | `ScaffoldResult`, `PostCommandResult`, `PlantUmlValidationResult` | Good pattern, inconsistently applied |
| **Rollback** | `IGenerationRollbackService` for file generation | Exists but limited scope |
| **Logging** | `Microsoft.Extensions.Logging` with console provider | Minimal configuration |
| **Input Validation** | `JsonSchemaInputValidator`, `GenerationOptionsValidator` | Present in key areas |

### Gaps Identified

1. **No global exception handler** at the CLI entry point catching unhandled exceptions and mapping them to structured output and exit codes.
2. **No `Result<T>` monad** for service-layer operations; many methods return `void` or raw types and throw on failure.
3. **Inconsistent error propagation** across generation strategies; some throw, some return nulls, some log and continue silently.
4. **No structured logging** (JSON, correlation IDs) for tracing multi-step generation pipelines.
5. **No retry/circuit-breaker patterns** for I/O-bound operations (file system, process execution).
6. **Rollback service** only covers file creation, not directory creation, config mutations, or partial scaffold state.
7. **No error aggregation** for batch operations (e.g., generating 20 artifacts where 3 fail).
8. **No user-facing error formatting** layer separating internal diagnostics from CLI output.
9. **Missing cancellation support** (`CancellationToken`) through async pipelines.
10. **No telemetry/metrics hooks** for error rates or failure categorization.

---

## 2. Design Principles

| Principle | Description |
|-----------|-------------|
| **Fail fast, fail clearly** | Detect errors at the earliest possible point. Never swallow exceptions silently. |
| **Errors are values, not control flow** | Use `Result<T>` for expected failures. Reserve exceptions for truly exceptional conditions. |
| **Aggregate, don't abort** | In batch operations, collect all errors and report them together rather than stopping at the first. |
| **Separation of concerns** | Internal error representations are distinct from user-facing messages. |
| **Idempotent recovery** | Rollback and retry operations must be safe to execute multiple times. |
| **Observable by default** | Every error pathway produces structured, traceable output. |
| **Cancellation-aware** | All async operations respect `CancellationToken`. |

---

## 3. Implementation Plan

### Phase 1: Foundation (Core Error Primitives)

#### 3.1 Introduce `Result<T>` and `Result` Types

**Location:** `CodeGenerator.Abstractions/Results/`

```
Result<T>
├── Success(T value)
├── Failure(ErrorInfo error)
├── IsSuccess : bool
├── IsFailure : bool
├── Value : T              (throws if Failure)
├── Error : ErrorInfo      (throws if Success)
├── Map<U>(Func<T, U>)     → Result<U>
├── Bind<U>(Func<T, Result<U>>) → Result<U>
├── Match(onSuccess, onFailure) → U
└── implicit operator from T / ErrorInfo
```

**`ErrorInfo` value object:**

```
ErrorInfo
├── Code : string           (machine-readable, e.g., "SCAFFOLD_PARSE_FAILED")
├── Message : string        (human-readable)
├── Category : ErrorCategory
├── Severity : ErrorSeverity
├── Details : IReadOnlyDictionary<string, object>?
├── InnerError : ErrorInfo?
└── StackTrace : string?    (populated only in debug builds)
```

**`ErrorCategory` enum:**

```
Validation
IO
Process
Template
Configuration
Schema
Scaffold
Plugin
Internal
```

**Files to create:**

| File | Purpose |
|------|---------|
| `Result.cs` | Non-generic `Result` (success/failure with no value) |
| `Result{T}.cs` | Generic `Result<T>` |
| `ErrorInfo.cs` | Structured error descriptor |
| `ErrorCategory.cs` | Error classification enum |
| `ResultExtensions.cs` | LINQ-style composition (`Select`, `SelectMany`, `Where`, `Combine`) |

**Migration strategy:** Introduce `Result<T>` for all new code. Retrofit existing service methods incrementally, starting with `ScaffoldEngine` and `ArtifactGenerator` which are the highest-traffic paths.

#### 3.2 Expand the Exception Hierarchy

**Add to `CodeGenerator.Core/Errors/`:**

| Exception | Exit Code | When |
|-----------|-----------|------|
| `CliConfigurationException` | 5 | Invalid CLI configuration, missing settings, corrupt config files |
| `CliPluginException` | 6 | Plugin discovery/load/execution failure |
| `CliSchemaException` | 7 | PlantUML or JSON schema parse/validation failure |
| `CliCancelledException` | 8 | Operation cancelled by user or token |
| `CliAggregateException` | varies | Wraps multiple `CliException` instances from batch operations |

Update `CliExitCodes`:

```csharp
public static class CliExitCodes
{
    public const int Success            = 0;
    public const int ValidationError    = 1;
    public const int IoError            = 2;
    public const int ProcessError       = 3;
    public const int TemplateError      = 4;
    public const int ConfigurationError = 5;
    public const int PluginError        = 6;
    public const int SchemaError        = 7;
    public const int Cancelled          = 8;
    public const int UnexpectedError    = 99;
}
```

#### 3.3 Error Code Registry

**Location:** `CodeGenerator.Core/Errors/ErrorCodes.cs`

A static class with constants for every known error code string used in `ErrorInfo.Code`. Organized by category:

```csharp
public static class ErrorCodes
{
    // Validation
    public const string InvalidIdentifier       = "VALIDATION_INVALID_IDENTIFIER";
    public const string MissingRequiredField    = "VALIDATION_MISSING_REQUIRED";
    public const string DirectoryNotWritable    = "VALIDATION_DIR_NOT_WRITABLE";

    // IO
    public const string FileNotFound            = "IO_FILE_NOT_FOUND";
    public const string FileAccessDenied        = "IO_ACCESS_DENIED";
    public const string DirectoryCreateFailed   = "IO_DIR_CREATE_FAILED";

    // Template
    public const string TemplateNotFound        = "TEMPLATE_NOT_FOUND";
    public const string TemplateRenderFailed    = "TEMPLATE_RENDER_FAILED";
    public const string TemplateSyntaxError     = "TEMPLATE_SYNTAX_ERROR";

    // Scaffold
    public const string ScaffoldParseFailed     = "SCAFFOLD_PARSE_FAILED";
    public const string ScaffoldConflict        = "SCAFFOLD_FILE_CONFLICT";
    public const string PostCommandFailed       = "SCAFFOLD_POST_CMD_FAILED";

    // Process
    public const string ProcessTimedOut         = "PROCESS_TIMEOUT";
    public const string ProcessNonZeroExit      = "PROCESS_NON_ZERO_EXIT";

    // Plugin
    public const string PluginLoadFailed        = "PLUGIN_LOAD_FAILED";
    public const string StrategyNotFound        = "PLUGIN_STRATEGY_NOT_FOUND";

    // Schema
    public const string SchemaInvalid           = "SCHEMA_INVALID";
    public const string SchemaEntityInvalid     = "SCHEMA_ENTITY_INVALID";
}
```

---

### Phase 2: Pipeline Error Handling

#### 3.4 Global Exception Handler at CLI Entry Point

**Location:** `CodeGenerator.Cli/Program.cs`

Wrap the top-level command execution in a handler that:

1. Catches `CliException` subclasses and maps to formatted console output + correct exit code.
2. Catches `OperationCanceledException` and maps to exit code 8.
3. Catches all other `Exception` types, logs full stack trace at `Debug` level, writes a sanitized message to stderr, and exits with code 99.
4. In verbose mode (`--verbose` / `-v`), includes stack traces and inner exception chains in output.

```
try
{
    await rootCommand.InvokeAsync(args);
}
catch (CliAggregateException ex)
{
    // Format each inner error, return highest-severity exit code
}
catch (CliException ex)
{
    // Format error, return ex.ExitCode
}
catch (OperationCanceledException)
{
    // "Operation cancelled.", exit 8
}
catch (Exception ex)
{
    // "An unexpected error occurred. Run with --verbose for details.", exit 99
}
```

#### 3.5 Generation Pipeline Error Aggregation

**Location:** `CodeGenerator.Core/Artifacts/`

For batch artifact generation (where multiple files are generated from a model):

```
ArtifactGenerationResult
├── List<GeneratedArtifact> Succeeded
├── List<ArtifactError> Failed
├── List<ArtifactWarning> Warnings
├── bool HasErrors
├── bool IsFullSuccess
├── ValidationResult MergedValidation
└── string ToSummary()
```

The `ArtifactGenerator` should:

1. Validate the model once upfront (fail fast on model-level errors).
2. Iterate over all generation strategies, catching per-strategy failures.
3. Record each success/failure in `ArtifactGenerationResult`.
4. Continue generating remaining artifacts after a strategy failure (unless `--fail-fast` flag is set).
5. Return the aggregated result for the caller to decide presentation.

#### 3.6 Scaffold Pipeline Error Enrichment

**Enhance `ScaffoldResult`:**

```csharp
public class ScaffoldResult
{
    public bool Success => ValidationResult.IsValid && !Errors.Any();
    public List<PlannedFile> PlannedFiles { get; set; } = [];
    public List<string> Conflicts { get; set; } = [];
    public ValidationResult ValidationResult { get; set; } = new();
    public List<PostCommandResult> PostCommandResults { get; set; } = [];

    // New
    public List<ErrorInfo> Errors { get; set; } = [];
    public List<string> RolledBackFiles { get; set; } = [];
    public TimeSpan Duration { get; set; }
    public string? CorrelationId { get; set; }
}
```

Each step in the scaffold pipeline (`parse` -> `validate` -> `plan` -> `execute` -> `post-commands`) should:

1. Produce its own `Result<T>` or contribute to the shared `ScaffoldResult`.
2. On failure at any step, skip remaining steps and trigger rollback.
3. Record rollback actions taken in `RolledBackFiles`.

---

### Phase 3: Resilience Patterns

#### 3.7 Retry Policy for I/O Operations

**Location:** `CodeGenerator.Core/IO/`

Introduce a lightweight retry helper (no external dependency required):

```csharp
public static class Retry
{
    public static async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        RetryOptions options,
        CancellationToken ct = default);
}

public class RetryOptions
{
    public int MaxAttempts { get; init; } = 3;
    public TimeSpan InitialDelay { get; init; } = TimeSpan.FromMilliseconds(100);
    public double BackoffMultiplier { get; init; } = 2.0;
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(5);
    public Func<Exception, bool> ShouldRetry { get; init; } = IsTransient;

    public static bool IsTransient(Exception ex) =>
        ex is IOException or UnauthorizedAccessException;
}
```

**Apply to:**

| Operation | Retry | Rationale |
|-----------|-------|-----------|
| File write/copy | Yes (3 attempts) | Antivirus locks, transient OS locks |
| Directory creation | Yes (2 attempts) | Race conditions in parallel generation |
| Process execution (post-commands) | No by default, opt-in | Side effects may not be idempotent |
| Template loading from disk | Yes (2 attempts) | File system caching delays |
| YAML/JSON parsing | No | Deterministic; retrying won't help |

#### 3.8 CancellationToken Propagation

**Scope:** All `async` methods across the pipeline.

1. Add `CancellationToken ct = default` parameter to every async method in:
   - `IScaffoldEngine.ScaffoldAsync`
   - `IArtifactGenerator.GenerateAsync`
   - `ICommandService.ExecuteAsync`
   - `ITemplateProcessor.ProcessAsync`
   - All scaffold services

2. CLI entry point creates a `CancellationTokenSource` linked to `Console.CancelKeyPress` (Ctrl+C).

3. On cancellation, trigger rollback of any partially generated files, then exit with code 8.

#### 3.9 Expanded Rollback Service

**Enhance `IGenerationRollbackService`:**

```csharp
public interface IGenerationRollbackService
{
    void TrackFileCreated(string filePath);
    void TrackFileModified(string filePath, string originalContent);
    void TrackDirectoryCreated(string directoryPath);
    void TrackFileDeleted(string filePath, string previousContent);

    RollbackReport Rollback();
    void Commit();  // Clears tracking; changes are accepted
}

public class RollbackReport
{
    public List<string> FilesDeleted { get; init; }
    public List<string> FilesRestored { get; init; }
    public List<string> DirectoriesDeleted { get; init; }
    public List<string> Failures { get; init; }
    public bool FullyRolledBack => !Failures.Any();
}
```

**Key behaviors:**

- Track all file system mutations (creates, modifications, deletions, directory creates).
- On rollback, reverse operations in LIFO order.
- If a rollback operation itself fails, log the failure but continue rolling back remaining items.
- `Commit()` clears the tracking log, signaling that the operation completed successfully and rollback is no longer needed.

---

### Phase 4: Observability

#### 3.10 Structured Logging

**Replace** the current basic console logging configuration with structured output.

**Configuration updates in `Program.cs`:**

```csharp
services.AddLogging(builder =>
{
    builder.AddConsole(options =>
    {
        options.FormatterName = isJsonOutput ? "json" : "simple";
    });

    if (isJsonOutput)
    {
        builder.AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
            options.JsonWriterOptions = new() { Indented = false };
        });
    }

    builder.SetMinimumLevel(isVerbose ? LogLevel.Debug : LogLevel.Information);
});
```

**Correlation IDs:**

1. Generate a `correlationId` (GUID) at the start of each CLI command invocation.
2. Push it into `ILogger.BeginScope(new { CorrelationId = correlationId })`.
3. All log entries within that scope automatically include the correlation ID.
4. Include in `ScaffoldResult.CorrelationId` and `ArtifactGenerationResult` for cross-referencing.

#### 3.11 Error Reporting Formatter

**Location:** `CodeGenerator.Cli/Formatting/`

```
IErrorFormatter
├── FormatError(ErrorInfo) → string
├── FormatValidationResult(ValidationResult) → string
├── FormatArtifactResult(ArtifactGenerationResult) → string
├── FormatScaffoldResult(ScaffoldResult) → string
└── FormatException(CliException, bool verbose) → string
```

**Implementations:**

| Formatter | Output |
|-----------|--------|
| `ConsoleErrorFormatter` | Colored, human-readable terminal output with ANSI codes |
| `JsonErrorFormatter` | Machine-readable JSON for CI/CD pipeline integration |
| `MarkdownErrorFormatter` | Markdown-formatted reports for logging to files |

**Console output example:**

```
ERROR [SCAFFOLD_PARSE_FAILED] Failed to parse scaffold configuration

  Line 12, Column 4: Unexpected mapping key 'dependencies'
  Expected one of: projects, files, commands

  Hint: Check that your YAML indentation is consistent (use 2 spaces).

  Run with --verbose for full stack trace.
```

#### 3.12 Diagnostic Context Enrichment

Add contextual information to errors automatically:

```csharp
public class DiagnosticContext
{
    public string? CurrentFile { get; set; }
    public string? CurrentStrategy { get; set; }
    public string? CurrentPhase { get; set; }  // "parse", "validate", "generate", "post-command"
    public string? ModelType { get; set; }
    public Dictionary<string, object> Properties { get; } = new();
}
```

Stored in `AsyncLocal<DiagnosticContext>` so it flows through async calls. When an exception is caught, the handler enriches `ErrorInfo.Details` with whatever diagnostic context is active.

---

### Phase 5: Validation Enhancements

#### 3.13 Fluent Validation Builder

**Location:** `CodeGenerator.Core/Validation/`

Complement the existing `IValidatable` pattern with a fluent builder for complex validation chains:

```csharp
public class Validator<T>
{
    public Validator<T> RuleFor<TProp>(
        Expression<Func<T, TProp>> property,
        Func<TProp, bool> predicate,
        string errorMessage);

    public Validator<T> When(
        Func<T, bool> condition,
        Action<Validator<T>> rules);

    public Validator<T> Must(
        Func<T, bool> predicate,
        string errorMessage);

    public ValidationResult Validate(T instance);
}
```

**Apply to:**

- `ScaffoldConfiguration` validation (replace procedural checks in `ConfigValidator`).
- `GenerationOptions` validation (replace `GenerationOptionsValidator`).
- PlantUML model validation.

#### 3.14 Cross-Cutting Validation Rules

Create shared validation rules that apply across multiple validators:

```csharp
public static class CommonRules
{
    public static bool IsValidCSharpIdentifier(string value);
    public static bool IsValidFilePath(string path);
    public static bool IsValidNamespace(string ns);
    public static bool IsSupportedFrameworkVersion(string version);
    public static bool IsWritableDirectory(string path);
}
```

These already exist in scattered form across `GenerationOptionsValidator` and other classes. Consolidate them into a single, tested utility.

#### 3.15 Validation Result Enhancements

**Add to `ValidationResult`:**

```csharp
public class ValidationResult
{
    // Existing
    public List<ValidationError> Errors { get; }
    public List<ValidationError> Warnings { get; }
    public bool IsValid { get; }

    // New
    public IReadOnlyList<ValidationError> InfoMessages { get; }
    public IReadOnlyList<ValidationError> All { get; }  // Errors + Warnings + Info

    public ValidationResult AddInfo(string propertyName, string message);
    public ValidationResult WithContext(string contextName);  // Prefixes all property names
    public string ToFormattedString(bool includeWarnings = true);
    public IDictionary<string, List<ValidationError>> GroupByProperty();
}
```

---

### Phase 6: Plugin & Strategy Error Isolation

#### 3.16 Strategy Execution Wrapper

**Location:** `CodeGenerator.Core/Artifacts/`

Wrap every `ISyntaxGenerationStrategy` and `IArtifactGenerationStrategy` execution in a protective boundary:

```csharp
public class StrategyExecutor<TModel>
{
    public Result<string> ExecuteSyntaxStrategy(
        ISyntaxGenerationStrategy<TModel> strategy,
        TModel model,
        DiagnosticContext context)
    {
        try
        {
            context.CurrentStrategy = strategy.GetType().Name;
            var output = strategy.Generate(model);
            return Result<string>.Success(output);
        }
        catch (SkipFileException)
        {
            return Result<string>.Success(string.Empty);  // Intentional skip
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(new ErrorInfo
            {
                Code = ErrorCodes.StrategyNotFound,
                Message = $"Strategy {strategy.GetType().Name} failed: {ex.Message}",
                Category = ErrorCategory.Plugin,
                InnerError = ErrorInfo.FromException(ex)
            });
        }
    }
}
```

**Benefits:**

- A single broken strategy cannot crash the entire generation pipeline.
- Errors are captured with full context (which strategy, which model, which phase).
- `SkipFileException` is handled as a normal control flow signal, not an error.

#### 3.17 Plugin Discovery Error Handling

**Location:** `CodeGenerator.Core/ConfigureServices.cs`

During assembly scanning for strategies:

1. Wrap `Assembly.GetTypes()` in a try/catch for `ReflectionTypeLoadException`.
2. Log each type that fails to load as a warning (with the loader exception).
3. Continue loading remaining types rather than failing the entire discovery.
4. Surface a summary at the end: "Loaded 47/50 strategies. 3 failed to load (run with --verbose for details)."

---

### Phase 7: Testing Infrastructure

#### 3.18 Error Handling Test Utilities

**Location:** `tests/CodeGenerator.Core.Tests/`

```csharp
public static class ResultAssertions
{
    public static void ShouldBeSuccess<T>(this Result<T> result);
    public static T ShouldBeSuccessWithValue<T>(this Result<T> result);
    public static void ShouldBeFailure<T>(this Result<T> result);
    public static void ShouldBeFailureWithCode<T>(this Result<T> result, string errorCode);
    public static void ShouldHaveValidationError(this ValidationResult result, string propertyName);
    public static void ShouldHaveNoErrors(this ValidationResult result);
}
```

#### 3.19 Test Coverage Requirements

| Area | Minimum Coverage | Test Types |
|------|-----------------|------------|
| `Result<T>` / `Result` | 100% | Unit |
| Exception hierarchy constructors | 100% | Unit |
| `ValidationResult` operations | 100% | Unit |
| Global exception handler | 95% | Integration |
| Rollback service | 95% | Integration |
| Retry policy | 90% | Unit |
| Error formatters | 90% | Unit |
| Strategy executor boundary | 90% | Unit |
| Scaffold pipeline errors | 85% | Integration |
| End-to-end error scenarios | 80% | E2E |

#### 3.20 Chaos/Fault Injection for Integration Tests

Create test helpers that inject faults into the generation pipeline:

```csharp
public class FaultInjectionOptions
{
    public double FileWriteFailureRate { get; set; } = 0.0;
    public double TemplateRenderFailureRate { get; set; } = 0.0;
    public double ProcessExecutionFailureRate { get; set; } = 0.0;
    public TimeSpan? SimulatedLatency { get; set; }
    public bool SimulateDiskFull { get; set; }
    public bool SimulatePermissionDenied { get; set; }
}
```

Register fault-injecting decorators via DI in test configurations to verify that:

- Partial failures are handled gracefully.
- Rollback cleans up correctly under fault conditions.
- Error aggregation works when multiple failures occur.

---

## 4. Migration Strategy

### Prioritized Rollout Order

| Priority | Phase | Items | Rationale |
|----------|-------|-------|-----------|
| **P0** | 1 | Global exception handler (3.4) | Prevents raw stack traces from reaching users immediately |
| **P0** | 1 | `Result<T>` types (3.1) | Foundation for all subsequent work |
| **P1** | 1 | Error code registry (3.3) | Enables consistent machine-readable error identification |
| **P1** | 2 | Generation pipeline aggregation (3.5) | Most user-visible improvement for batch generation |
| **P1** | 3 | Expanded rollback (3.9) | Prevents orphaned files on failure |
| **P2** | 3 | CancellationToken propagation (3.8) | Required for responsive Ctrl+C handling |
| **P2** | 2 | Scaffold pipeline enrichment (3.6) | Better diagnostics for most common workflow |
| **P2** | 4 | Structured logging (3.10) | Enables CI/CD integration and debugging |
| **P2** | 4 | Error formatters (3.11) | User-facing quality improvement |
| **P3** | 3 | Retry policy (3.7) | Handles transient I/O failures |
| **P3** | 5 | Validation enhancements (3.13-3.15) | Quality-of-life improvements |
| **P3** | 6 | Strategy isolation (3.16-3.17) | Robustness for plugin ecosystem |
| **P3** | 7 | Test infrastructure (3.18-3.20) | Ensures ongoing reliability |

### Backward Compatibility

- Existing `CliException` subclasses remain unchanged; new exceptions extend the same base.
- `ValidationResult` additions are purely additive; no breaking changes.
- `ScaffoldResult` new properties have default values; existing consumers unaffected.
- `Result<T>` is introduced alongside existing patterns; methods are migrated one at a time.
- Existing tests continue to pass without modification during migration.

### File Change Summary

| Action | Location | Count |
|--------|----------|-------|
| **New files** | `CodeGenerator.Abstractions/Results/` | 5 |
| **New files** | `CodeGenerator.Core/Errors/` | 4 |
| **New files** | `CodeGenerator.Core/IO/` | 2 |
| **New files** | `CodeGenerator.Cli/Formatting/` | 4 |
| **New files** | `tests/` | 6+ |
| **Modified** | `CodeGenerator.Core/Errors/CliExitCodes.cs` | 1 |
| **Modified** | `CodeGenerator.Cli/Program.cs` | 1 |
| **Modified** | `CodeGenerator.Core/Artifacts/Abstractions/ArtifactGenerator.cs` | 1 |
| **Modified** | `CodeGenerator.Core/Scaffold/Services/ScaffoldEngine.cs` | 1 |
| **Modified** | `CodeGenerator.Core/Scaffold/Models/ScaffoldResult.cs` | 1 |
| **Modified** | `CodeGenerator.Abstractions/Validation/ValidationResult.cs` | 1 |
| **Modified** | `CodeGenerator.Core/ConfigureServices.cs` | 1 |
| **Incrementally modified** | All async service interfaces and implementations | ~20-30 |

---

## 5. Error Handling Decision Matrix

Use this matrix to decide which error handling pattern to apply in a given situation:

| Situation | Pattern | Example |
|-----------|---------|---------|
| Invalid user input | `ValidationResult` with collected errors | Missing required field in scaffold YAML |
| Expected operational failure | `Result<T>.Failure(ErrorInfo)` | Template file not found on disk |
| Unrecoverable system failure | Throw `CliException` subclass | Out of disk space during generation |
| Transient I/O failure | Retry with backoff, then `Result.Failure` | File locked by another process |
| User requested cancellation | `OperationCanceledException` via `CancellationToken` | Ctrl+C during scaffold |
| Partial batch failure | `ArtifactGenerationResult` with per-item status | 18/20 files generated, 2 failed |
| Plugin/strategy failure | `StrategyExecutor` boundary catch -> `Result.Failure` | Broken syntax strategy |
| Programmer error / bug | Let exception propagate to global handler | Null reference in internal logic |
| Skip intentionally | `SkipFileException` (existing pattern) | Conditional file generation |

---

## 6. Error Message Guidelines

### Structure

Every user-facing error message should follow this structure:

```
{SEVERITY} [{ERROR_CODE}] {Brief summary}

  {Detailed explanation with specific values}

  Hint: {Actionable suggestion for the user}

  {Optional: path to relevant file or config}
```

### Rules

1. **Be specific.** "File not found: `src/models/User.cs`" not "A file was not found."
2. **Be actionable.** Always include what the user can do to fix the issue.
3. **No jargon in user-facing messages.** Translate `NullReferenceException` to "An internal error occurred."
4. **Include context.** Which operation was being performed, which file was being generated, which step of the scaffold.
5. **Distinguish user errors from system errors.** User errors get hints. System errors get "please report this issue."
6. **No sensitive data.** Never include full file paths from the system (only project-relative paths), environment variables, or internal state in user-facing output.
7. **Consistent severity labels.** `ERROR`, `WARNING`, `INFO` -- no variations.

---

## 7. Monitoring & Operational Readiness

### CLI Exit Code Contract

Document and enforce that every CLI invocation exits with a code from `CliExitCodes`. CI/CD pipelines and scripts depend on these codes being stable and meaningful.

| Code | Meaning | CI/CD Action |
|------|---------|-------------|
| 0 | Success | Continue pipeline |
| 1 | Validation error | Fix input, re-run |
| 2 | I/O error | Check permissions, disk space |
| 3 | Process error | Check post-command dependencies |
| 4 | Template error | Check template files |
| 5 | Configuration error | Check CLI config |
| 6 | Plugin error | Check plugin assemblies |
| 7 | Schema error | Check schema files |
| 8 | Cancelled | User cancelled or timeout |
| 99 | Unexpected | File a bug report |

### JSON Output Mode

When `--output json` is specified, all output (including errors) should be valid JSON:

```json
{
  "success": false,
  "correlationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "errors": [
    {
      "code": "SCAFFOLD_PARSE_FAILED",
      "message": "Failed to parse scaffold configuration",
      "category": "Scaffold",
      "details": {
        "line": 12,
        "column": 4,
        "file": "scaffold.yaml"
      }
    }
  ],
  "warnings": [],
  "artifacts": {
    "succeeded": 0,
    "failed": 0,
    "skipped": 0
  },
  "duration": "PT2.340S"
}
```

This enables automated tooling, IDE integrations, and CI/CD pipelines to parse and act on results programmatically.
