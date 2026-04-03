# Implementation Patterns from xregistry/codegen

Analysis of [xregistry/codegen](https://github.com/xregistry/codegen) (`xrcg`) and patterns
that CodeGenerator could adopt or adapt.

## Overview

`xrcg` is a Python-based CLI that generates production-ready messaging SDKs from xRegistry
message catalog definitions across 5 languages (Java, C#, Python, TypeScript, Go) and 7+
protocols (Kafka, AMQP, MQTT, Azure Event Hubs/Service Bus/Event Grid, HTTP/CloudEvents).
Its architecture is roughly 80% Jinja2 templates with a Python orchestration layer.

CodeGenerator is a .NET-based multi-framework code generation framework using a strategy-dispatch
pattern, DotLiquid templates, and Roslyn for C# syntax trees. It generates code for .NET, Python,
React, Angular, Flask, React Native, Playwright, and Detox.

---

## Pattern 1: Template-Mirrors-Output Directory Structure

### How xrcg does it

Template files are organized so their directory structure directly maps to the output project
structure. A template at `cs/kafkaproducer/src/Producer.cs.jinja` produces output at
`src/Producer.cs`. Adding a file to the template directory automatically adds it to generated
projects with zero configuration.

```
templates/
  cs/
    kafkaproducer/
      src/
        Producer.cs.jinja        --> src/Producer.cs
        ProducerConfig.cs.jinja  --> src/ProducerConfig.cs
      test/
        ProducerTest.cs.jinja    --> test/ProducerTest.cs
```

### What CodeGenerator could adopt

CodeGenerator currently uses embedded resources (`Templates/**/*.txt`) loaded via
`EmbeddedResourceTemplateLocatorBase<T>`. The template-to-output mapping is encoded in strategy
classes rather than directory conventions.

**Idea:** Introduce an alternative template discovery mode where templates in a conventional
directory structure are auto-discovered and rendered without needing a dedicated strategy class
per file. This would let users/agents add new files to generated projects by simply dropping a
template into the right directory.

**Implementation sketch:**
- Add a `ConventionBasedArtifactGenerationStrategy` that walks a template directory tree
- Template paths relative to the style root become output paths relative to the project root
- Template filename minus `.liquid` extension becomes the output filename
- Existing embedded-resource strategies remain for complex generation logic

---

## Pattern 2: Language/Style Two-Level Template Hierarchy

### How xrcg does it

Templates are organized in a two-level matrix: **language** x **style**.

```
templates/
  {language}/           # cs, java, py, ts, go
    _common/            # shared macros across all styles for this language
    _schemas/           # schema-to-data-class templates
    {style}/            # kafkaproducer, mqttclient, amqpconsumer, etc.
      _templateinfo.json
      src/
      test/
```

This separation means protocol-specific logic (Kafka, AMQP) is orthogonal to language-specific
logic (C#, Java). New protocols can be added without touching language code.

### What CodeGenerator could adopt

CodeGenerator already separates by language (DotNet, Python, React, etc.) at the assembly level,
but within each assembly, patterns like "CQRS handler" or "REST controller" are baked into
individual strategy classes.

**Idea:** Introduce a **style** dimension within each language module. For example, `.NET` could
have styles like `cleanarchitecture`, `minimalapi`, `verticalslice`. Each style would have its
own template set and metadata, selectable at generation time.

**Benefits:**
- Agents could pick architectural styles without CodeGenerator needing new strategy classes
- Community/user-contributed styles become possible via template directories
- Existing strategy-based generation remains for complex Roslyn-powered output

---

## Pattern 3: Template Metadata Sidecar Files

### How xrcg does it

Each template set has a `_templateinfo.json` that configures behavior without code:

```json
{
  "description": "C# Apache Kafka Producer",
  "priority": 1,
  "main_project_name": "{project_name|pascal}KafkaProducer",
  "data_project_name": "{project_name|pascal}Data",
  "data_project_dir": "{project_name~schemas}",
  "src_layout": true
}
```

### What CodeGenerator could adopt

CodeGenerator uses C# classes to configure each generation strategy (priority, naming, etc.).
Adding metadata sidecar files (JSON or YAML) alongside template directories would enable:

- Template configuration without recompiling
- Naming conventions expressed as filter chains (`{name|pascal}Controller`)
- Priority ordering for strategy dispatch
- Description/documentation embedded with templates

**Implementation sketch:**
- Define a `TemplateSetInfo` model loaded from `_templateinfo.json`
- `ConventionBasedArtifactGenerationStrategy` reads this metadata at discovery time
- Naming filter chains reuse the existing `NamingConventionConverter`

---

## Pattern 4: Dynamic Filename Placeholders

### How xrcg does it

Template filenames contain expansion macros resolved at generation time:

```
{classname}Controller.cs.jinja     --> UserController.cs (iterated per class)
{mainprojectdir!dotunderscore}__init__.py.jinja  --> my_project/__init__.py
{rootdir~samples}sample.py.jinja   --> samples/sample.py
```

Special macros:
- `{classname}` / `{classdir}` -- triggers per-entity iteration
- `{rootdir}` -- forces output to project root
- `{testdir}` -- test directory path
- `!` for filter chains in filenames (since `|` is invalid in filenames)

### What CodeGenerator could adopt

CodeGenerator's `TemplatedFileModel` already supports token dictionaries for template content,
but filenames are computed in strategy code.

**Idea:** Extend `TemplatedFileModel` or the convention-based strategy to support placeholder
tokens in template filenames. A template named `{{EntityName}}Controller.cs.liquid` would be
rendered once per entity, with the filename resolved from the model.

**Key decisions:**
- Use `{{}}` for filename placeholders (consistent with Liquid syntax)
- Iteration trigger: if a filename contains `{{EntityName}}`, iterate over the entities collection
- Path segment separator: `~` (matching xrcg) or `/` with encoding

---

## Pattern 5: Underscore-Prefix Ordering for Post-Processing

### How xrcg does it

Files and directories prefixed with `_` are processed AFTER regular templates. This is critical
because post-processing templates (like `.csproj` files that list all generated classes) need to
know what was generated.

```
src/Producer.cs.jinja           # generated first
src/Consumer.cs.jinja           # generated first
_project.csproj.jinja           # generated last, can reference all classes
```

### What CodeGenerator could adopt

CodeGenerator's strategy priority system (`GetPriority()`) already handles ordering, but it's
per-strategy-class, not per-template.

**Idea:** In the convention-based template mode, adopt the underscore-prefix ordering convention.
Templates prefixed with `_` are rendered last, receiving a context that includes all previously
generated file paths and metadata. This enables:

- `.csproj` / `package.json` files that automatically include all generated source files
- Barrel/index files that re-export all generated modules
- Test runner configurations that reference all test files

---

## Pattern 6: Cross-Template State (Stacks and Dictionaries)

### How xrcg does it

`ContextStacksManager` provides state that persists across template renders:

```python
# In per-class template:
push("UserService", "generated_classes")

# In post-processing project template:
{% for cls in stack("generated_classes") %}
<Compile Include="{{ cls }}.cs" />
{% endfor %}
```

Three mechanisms:
1. **Named stacks**: `push(value, name)` / `pop(name)` / `stack(name)`
2. **Dictionary**: `save(value, key)` / `get(key)`
3. **File stack**: `push_file(content, name)` -- queue additional file outputs

### What CodeGenerator could adopt

CodeGenerator has `IObjectCache` (`ConcurrentDictionary<string, object>`) and `IContext` with
domain events, but these aren't designed for cross-template state accumulation.

**Idea:** Add a `GenerationContext` that accumulates state across template renders within a
single generation session:

```csharp
public interface IGenerationContext
{
    void Push(string stackName, object value);
    IReadOnlyList<object> GetStack(string stackName);
    void Set(string key, object value);
    object Get(string key);
    IReadOnlyList<FileModel> GeneratedFiles { get; }
}
```

Register this as scoped (per generation run). Template strategies can push state, and
post-processing strategies can read accumulated state.

---

## Pattern 7: Conditional File Generation (`exit` Tag)

### How xrcg does it

Templates can conditionally skip their own output:

```jinja2
{%- if not definition.messagegroups -%}
{% exit %}
{%- endif -%}
```

When `{% exit %}` is hit, the file is simply not written. No error, no empty file.

### What CodeGenerator could adopt

CodeGenerator's strategy dispatch uses `CanHandle()` for filtering, but this operates at the
strategy level, not per-file.

**Idea:** Support a convention where templates returning empty/whitespace-only content are not
written to disk. Or add an explicit `{% skip %}` / `{% exit %}` custom Liquid tag:

```csharp
// Custom DotLiquid tag
public class ExitTag : Tag
{
    public override void Render(Context context, TextWriter result)
    {
        throw new SkipFileException();
    }
}
```

The template processor catches `SkipFileException` and suppresses file output.

---

## Pattern 8: Shared Macro Libraries (`_common/`)

### How xrcg does it

Reusable Jinja2 macros live in `_common/` directories with `.jinja.include` extension:

```
cs/_common/kafka.jinja.include
cs/_common/cloudevents.jinja.include
cs/_common/util.jinja.include
```

Templates import them:
```jinja2
{%- import "util.jinja.include" as util -%}
{{ util.format_namespace(namespace) }}
```

### What CodeGenerator could adopt

CodeGenerator's templates are embedded resources without a shared macro system. Common patterns
(namespace declarations, using statements, file headers) are repeated across templates or
handled in C# code.

**Idea:** Introduce DotLiquid partial/include support for shared template fragments:

```liquid
{% include 'common/file_header' %}
{% include 'common/namespace_open' %}
...
{% include 'common/namespace_close' %}
```

Store shared fragments as embedded resources under `Templates/_common/` and register them
with the `LiquidTemplateProcessor`.

---

## Pattern 9: Centralized Dependency Management

### How xrcg does it

Dependencies for generated projects are stored in reference files:

```
dependencies/
  cs/net80/dependencies.csproj
  java/jdk21/pom.xml
  python/py312/requirements.txt
  typescript/node22/package.json
```

Templates query via `dependency(language, runtime_version, name)` to get the correct version.
This centralizes version management and allows Dependabot to keep them updated.

### What CodeGenerator could adopt

CodeGenerator hard-codes dependency versions in templates or strategy classes (e.g., Zustand,
Axios versions in React templates).

**Idea:** Create a `dependencies/` directory with reference manifests per framework/version:

```
dependencies/
  dotnet/net8/packages.json
  react/v18/packages.json
  python/3.12/requirements.json
  flask/3.0/requirements.json
```

Add a `IDependencyResolver` service that templates and strategies query:

```csharp
public interface IDependencyResolver
{
    string GetVersion(string framework, string runtimeVersion, string packageName);
    IReadOnlyDictionary<string, string> GetAllDependencies(string framework, string runtimeVersion);
}
```

**Benefits:**
- Single source of truth for dependency versions
- Automated dependency updates via Dependabot/Renovate
- Easy multi-target support (e.g., .NET 8 vs .NET 9)

---

## Pattern 10: Rich Custom Template Filters

### How xrcg does it

27 custom Jinja2 filters provide language-aware transformations:

| Filter | Example |
|--------|---------|
| `pascal` | `user_service` -> `UserService` |
| `camel` | `user_service` -> `userService` |
| `snake` | `UserService` -> `user_service` |
| `namespace` | `Com.Example.Service` -> `Com.Example` |
| `strip_namespace` | `Com.Example.Service` -> `Service` |
| `exists` | Recursive property search in nested dicts |
| `mark_handled` | Flags a resource as processed |
| `schema_type` | Returns language-appropriate type name |

### What CodeGenerator could adopt

CodeGenerator already has `NamingConventionConverter` and `TokensBuilder` which auto-generates
naming variants (PascalCase, camelCase, snake_case, plurals). This is already strong.

**Additional filters to consider:**
- `namespace` / `strip_namespace` -- useful for Python module paths and Java package names
- `exists` -- recursive property search in model dictionaries
- `schema_type` -- language-aware type mapping (e.g., `string` -> `str` for Python)
- `mark_handled` / `is_handled` -- resource tracking to prevent duplicate generation

Register these as custom DotLiquid filters:

```csharp
Template.RegisterFilter(typeof(CodeGeneratorFilters));
```

---

## Pattern 11: Hierarchical Configuration (CLI > Env > File > Defaults)

### How xrcg does it

Four-tier priority-based config resolution:
1. CLI arguments (highest priority)
2. Environment variables
3. User config file (`~/.config/xrcg/config.json`)
4. Built-in defaults

### What CodeGenerator could adopt

CodeGenerator's CLI uses simple command arguments without a layered configuration system.

**Idea:** Implement `IConfigurationProvider` with layered resolution for generation options:

```csharp
public interface ICodeGeneratorConfiguration
{
    T GetValue<T>(string key, T defaultValue = default);
    // Resolution: CLI args > env vars > .codegenerator.json > defaults
}
```

This would allow users to set project-level defaults (preferred frameworks, naming conventions,
output directories) in a `.codegenerator.json` file, overridable by CLI args or environment
variables.

---

## Pattern 12: Input Validation with JSON Schema

### How xrcg does it

The `validate` command uses JSON Schema Draft 7 validation against bundled schemas before
generation begins. Sub-schemas handle different document sections (endpoints, messages, schemas).

### What CodeGenerator could adopt

CodeGenerator has `IValidatable` with `ValidationResult` on models, which is good for model-level
validation. What's missing is **input validation** -- validating user-provided data (PlantUML,
OpenAPI specs, configuration files) before they enter the generation pipeline.

**Idea:** Add JSON Schema validation for any structured input (OpenAPI specs, configuration files)
as a pre-generation step. Use `NJsonSchema` or `JsonSchema.Net` for .NET-native validation:

```csharp
public interface IInputValidator
{
    ValidationResult ValidateInput(string content, string schemaName);
}
```

---

## Pattern 13: Resource Tracking (mark_handled / is_handled)

### How xrcg does it

When processing schemas or message definitions, templates call `mark_handled(resource)` after
processing each one. Later templates check `is_handled(resource)` to avoid duplicate generation.
This prevents the same schema from being generated as both an Avro class and a JSON Schema class.

### What CodeGenerator could adopt

CodeGenerator's strategy dispatch uses `CanHandle()` which is stateless -- it doesn't know what
other strategies have already processed.

**Idea:** Add resource tracking to `IGenerationContext`:

```csharp
public interface IGenerationContext
{
    void MarkHandled(string resourceId, string handlerName);
    bool IsHandled(string resourceId);
    string GetHandler(string resourceId);
}
```

This prevents duplicate file generation when multiple strategies could handle the same model
element, and provides traceability for debugging.

---

## Pattern 14: Multi-Format Schema Abstraction

### How xrcg does it

Uses [Avrotize](https://github.com/clemensv/avrotize) as a universal schema converter. Input
schemas (JSON Schema, Avro, Proto, JSON Structure) are normalized through Avrotize, which then
generates language-specific data classes.

```
JSON Schema ─┐
Avro ────────┤──> Avrotize ──> C# / Java / Python / TypeScript / Go classes
Proto ───────┤
JSON ────────┘
```

### What CodeGenerator could adopt

CodeGenerator already handles PlantUML and OpenAPI via dedicated parsers
(`PlantUmlParserService`, `OpenApiService`). The pattern of abstracting input formats through a
normalizer is already partially present.

**Idea:** Formalize a schema normalization pipeline:

```csharp
public interface ISchemaNormalizer
{
    NormalizedSchema Normalize(string content, SchemaFormat format);
}

public enum SchemaFormat { PlantUml, OpenApi, JsonSchema, Avro, Proto }
```

Each input format gets a normalizer that produces a common `NormalizedSchema`, which all
downstream generators consume. This decouples input parsing from code generation.

---

## Prioritized Recommendations

### High Value, Low Effort

1. **Centralized dependency management** (Pattern 9) -- Extract hard-coded versions into
   reference files. Immediate maintainability win.
2. **Shared template macros** (Pattern 8) -- Reduce template duplication with `_common/`
   includes.
3. **Conditional file generation** (Pattern 7) -- Add `{% exit %}` or skip-on-empty to
   avoid generating unnecessary files.
4. **Additional template filters** (Pattern 10) -- `namespace`, `schema_type`, `exists`
   filters round out the existing strong filter set.

### High Value, Medium Effort

5. **Cross-template state** (Pattern 6) -- `IGenerationContext` with stacks enables
   post-processing templates that reference all generated files.
6. **Resource tracking** (Pattern 13) -- Prevents duplicate generation and aids debugging.
7. **Template metadata sidecar** (Pattern 3) -- `_templateinfo.json` enables configuration
   without recompilation.

### High Value, Higher Effort

8. **Template-mirrors-output convention** (Pattern 1) -- Convention-based template discovery
   is a significant architectural addition but dramatically simplifies adding new file types.
9. **Language/style matrix** (Pattern 2) -- Architectural styles as a selectable dimension
   unlocks community contributions.
10. **Dynamic filename placeholders** (Pattern 4) -- Per-entity iteration from filename
    conventions reduces strategy boilerplate.
11. **Schema normalization pipeline** (Pattern 14) -- Formalizing input format abstraction
    improves extensibility.

### Lower Priority

12. **Underscore-prefix ordering** (Pattern 5) -- Useful primarily if convention-based
    templates are adopted.
13. **Hierarchical configuration** (Pattern 11) -- Nice ergonomic improvement but not
    blocking any capabilities.
14. **Input validation with JSON Schema** (Pattern 12) -- Defensive improvement, lower
    impact on generation quality.
