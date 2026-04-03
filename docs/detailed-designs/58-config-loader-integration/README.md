# Config Loader Integration into Command Flow — Detailed Design

## 1. Overview

The `ConfigurationLoader`, `CodeGeneratorConfig`, and `CodeGeneratorConfiguration` classes exist but are not wired into the actual CLI command flow. `Program.cs` constructs a `CodeGeneratorConfiguration` with four empty dictionaries. `CreateCodeGeneratorCommand` hard-codes defaults for `--framework` (`net9.0`), `--output` (cwd), and `--slnx` (`false`) directly in option declarations. The `.codegenerator.json` file is never loaded.

This design integrates the 4-tier configuration hierarchy — **built-in defaults > `.codegenerator.json` > environment variables > CLI arguments** — into `Program.cs`, `CreateCodeGeneratorCommand`, and `ScaffoldCommand` so that users can set project-level defaults that commands respect without requiring explicit flags.

**Actors:** CLI user, CI pipeline  
**Scope:** `CodeGenerator.Cli` project — `Program.cs`, commands, DI registration

## 2. Architecture

### 2.1 C4 Context Diagram
![C4 Context](diagrams/c4_context.png)

### 2.2 C4 Container Diagram
![C4 Container](diagrams/c4_container.png)

### 2.3 C4 Component Diagram
![C4 Component](diagrams/c4_component.png)

## 3. Component Details

### 3.1 Program.cs — Configuration Bootstrap

**Responsibility:** Build the 4-tier `CodeGeneratorConfiguration` and register it in DI before any command runs.

**Current state:** Registers an empty `CodeGeneratorConfiguration` singleton.

**Target state:**
1. Define built-in defaults dictionary: `{ "framework": "net9.0", "output": ".", "slnx": "false" }`.
2. Call `IConfigurationLoader.LoadAsync(Directory.GetCurrentDirectory())` to get file config from `.codegenerator.json`.
3. Map `CodeGeneratorConfig.Defaults` properties to flat keys (`framework`, `solutionFormat` → `slnx`, `output`).
4. Read `CODEGEN_*` environment variables via `IConfiguration` (already registered) and map to config keys (e.g., `CODEGEN_FRAMEWORK` → `framework`).
5. CLI args are not available at this point — they are handled in step 3.2.
6. Construct `CodeGeneratorConfiguration(defaults, fileConfig, envConfig, cliConfig: empty)` and register as singleton.
7. Register `IConfigurationLoader` as singleton.

**Dependencies:** `IConfigurationLoader`, `IConfiguration`

### 3.2 CreateCodeGeneratorCommand — Config-Aware Option Defaults

**Responsibility:** Use `ICodeGeneratorConfiguration` to source default values for CLI options, and feed CLI-provided values back as the `cliConfig` tier.

**Current state:** `getDefaultValue` lambdas return hard-coded values.

**Target state:**
1. Resolve `ICodeGeneratorConfiguration` from `_serviceProvider` in constructor.
2. Replace hard-coded `getDefaultValue` lambdas:
   - `--framework`: `() => config.GetValue("framework", "net9.0")`
   - `--output`: `() => config.GetValue("output", Directory.GetCurrentDirectory())`
   - `--slnx`: `() => config.GetValue<bool>("slnx", false)`
3. In `HandleAsync`, no changes needed — System.CommandLine already merges CLI args over defaults.

**Dependencies:** `ICodeGeneratorConfiguration`

### 3.3 ScaffoldCommand — Config-Aware Defaults

**Responsibility:** Use config for default output directory and potentially default config file path.

**Current state:** `--output` defaults to `Directory.GetCurrentDirectory()`.

**Target state:**
1. Resolve `ICodeGeneratorConfiguration` in constructor.
2. `--output` default: `() => config.GetValue("output", Directory.GetCurrentDirectory())`.
3. Config file search: if `--config` not provided and `scaffold.yaml` not found at output dir, also check config's `templates.templatesDirectory` if set.

**Dependencies:** `ICodeGeneratorConfiguration`

### 3.4 EnvironmentVariableMapper (New)

**Responsibility:** Map `CODEGEN_*` environment variables from `IConfiguration` to config key-value pairs.

**Interface:**
```csharp
public static class EnvironmentVariableMapper
{
    public static Dictionary<string, string> Map(IConfiguration configuration);
}
```

**Mapping rules:**
| Environment Variable | Config Key |
|---|---|
| `CODEGEN_FRAMEWORK` | `framework` |
| `CODEGEN_OUTPUT` | `output` |
| `CODEGEN_SLNX` | `slnx` |
| `CODEGEN_AUTHOR` | `templates.author` |
| `CODEGEN_LICENSE` | `templates.license` |

**Location:** `src/CodeGenerator.Cli/Configuration/EnvironmentVariableMapper.cs`

### 3.5 ConfigFileMapper (New)

**Responsibility:** Flatten `CodeGeneratorConfig` object into a `Dictionary<string, string>` for the `fileConfig` tier.

**Interface:**
```csharp
public static class ConfigFileMapper
{
    public static Dictionary<string, string> ToFlatDictionary(CodeGeneratorConfig config);
}
```

**Mapping:**
- `config.Defaults.Framework` → `"framework"`
- `config.Defaults.SolutionFormat` → `"slnx"` (maps `"slnx"` string to `"true"`, else `"false"`)
- `config.Defaults.Output` → `"output"`
- `config.Templates.Author` → `"templates.author"`
- `config.Templates.License` → `"templates.license"`
- `config.Templates.TemplatesDirectory` → `"templates.directory"`

Null values are omitted.

**Location:** `src/CodeGenerator.Cli/Configuration/ConfigFileMapper.cs`

## 4. Data Model

### 4.1 Class Diagram
![Class Diagram](diagrams/class_diagram.png)

### 4.2 Entity Descriptions

| Entity | Description |
|---|---|
| `IConfigurationLoader` | Walks directory tree to find and parse `.codegenerator.json`. Already exists. |
| `ConfigurationLoader` | Implementation. Already exists. |
| `CodeGeneratorConfig` | Typed model for `.codegenerator.json` with `Defaults` and `Templates` sections. Already exists. |
| `CodeGeneratorConfiguration` | 4-tier merged config with case-insensitive key lookup. Already exists. |
| `ICodeGeneratorConfiguration` | Read interface for resolved config. Already exists in Abstractions. |
| `EnvironmentVariableMapper` | Static helper to extract `CODEGEN_*` env vars into a dictionary. **New.** |
| `ConfigFileMapper` | Static helper to flatten `CodeGeneratorConfig` into a dictionary. **New.** |

## 5. Key Workflows

### 5.1 Application Startup — Config Resolution
![Sequence Diagram](diagrams/sequence_startup.png)

1. `Program.cs` creates built-in defaults dictionary.
2. `Program.cs` calls `ConfigurationLoader.LoadAsync(cwd)` — walks up from cwd to find `.codegenerator.json`.
3. `ConfigFileMapper.ToFlatDictionary(fileConfig)` flattens the result.
4. `EnvironmentVariableMapper.Map(configuration)` extracts `CODEGEN_*` vars.
5. `new CodeGeneratorConfiguration(defaults, fileDict, envDict, cliDict: empty)` merges all tiers.
6. Registered as singleton in DI.

### 5.2 Command Execution — Config-Aware Defaults
![Sequence Diagram](diagrams/sequence_command.png)

1. `CreateCodeGeneratorCommand` constructor resolves `ICodeGeneratorConfiguration`.
2. Option defaults are sourced from config: `config.GetValue("framework", "net9.0")`.
3. System.CommandLine parses CLI args — explicitly provided args override config defaults.
4. `HandleAsync` receives the final merged values. No further config lookup needed.

## 6. API Contracts

No public API changes. The `.codegenerator.json` schema is unchanged:

```json
{
  "defaults": {
    "framework": "net9.0",
    "solutionFormat": "slnx",
    "output": "./generated"
  },
  "templates": {
    "author": "Quinntyne Brown",
    "license": "MIT",
    "templatesDirectory": "./templates"
  }
}
```

## 7. Security Considerations

- `ConfigurationLoader` walks up the directory tree. This is standard behavior (similar to `.gitconfig`, `tsconfig.json`). No path traversal risk since it only reads, never writes.
- Environment variables are read-only via `IConfiguration`. No shell expansion.
- File config values are used as defaults for safe parameters (framework name, directory path). The existing `GenerationOptionsValidator` validates all values before generation.

## 8. Open Questions

1. **Should CLI args be tracked as a 4th tier?** Currently System.CommandLine handles the override by setting option defaults from config. An alternative is to inject all raw CLI args as `cliConfig` into `CodeGeneratorConfiguration` for diagnostic/telemetry use. The simpler approach (option defaults) is recommended.
2. **Multiple `.codegenerator.json` files?** The current `ConfigurationLoader` returns the first one found walking upward. Should it merge multiple files (project-level + user-home-level)? Recommendation: keep single-file for now, add multi-file in a future iteration.
