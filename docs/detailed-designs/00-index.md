# Detailed Designs — Index

**Parent:** [L1-CodeGenerator.md](../specs/L1-CodeGenerator.md)
**Date:** 2026-04-03

| # | Feature | Status | Requirements | Description |
|---|---------|--------|--------------|-------------|
| 01 | [Core Generation Engine](01-core-generation-engine/README.md) | Implemented | FR-01, FR-13, FR-14, FR-18 | Strategy-based dispatch, template engine, naming conventions, command execution |
| 02 | [.NET Code Generation](02-dotnet-code-generation/README.md) | Implemented | FR-02, FR-03, FR-11, FR-12, FR-16, FR-17 | C# syntax, solution scaffolding, CQRS, DDD, PlantUML, Roslyn, Git |
| 03 | [Python Code Generation](03-python-code-generation/README.md) | Implemented | FR-04 | Python classes, functions, modules, decorators, type hints, project scaffolding |
| 04 | [Flask Backend Generation](04-flask-backend-generation/README.md) | Implemented | FR-05 | Flask app factories, controllers, SQLAlchemy models, repositories, schemas |
| 05 | [Angular Generation](05-angular-generation/README.md) | Implemented | FR-06 | Angular workspaces, standalone components, Jest config, barrel files |
| 06 | [React Generation](06-react-generation/README.md) | Implemented | FR-07 | React + Vite + TypeScript components, hooks, Zustand stores, API clients |
| 07 | [React Native Generation](07-react-native-generation/README.md) | Implemented | FR-08 | React Native screens, components, navigation, Zustand stores, styles |
| 08 | [Playwright Test Generation](08-playwright-test-generation/README.md) | Implemented | FR-09 | Playwright page objects, test specs, fixtures, multi-browser config |
| 09 | [Detox Test Generation](09-detox-test-generation/README.md) | Implemented | FR-10 | Detox mobile page objects, Jest test specs, iOS/Android configuration |
| 10 | [CLI Tool](10-cli-tool/README.md) | Implemented | FR-15 | `create-code-cli` global tool for scaffolding code generator projects |

### Priority Action Designs

| # | Feature | Status | Audit Ref | Description |
|---|---------|--------|-----------|-------------|
| 11 | [Extract Abstractions](11-extract-abstractions/README.md) | Implemented | Priority #1 | Decouple Core from heavy dependencies into lightweight Abstractions package |
| 12 | [Model Validation](12-model-validation/README.md) | Implemented | Priority #2 | Add IValidatable and validation pipeline to prevent broken agent output |
| 13 | [Builder/Fluent APIs](13-builder-fluent-apis/README.md) | Implemented | Priority #3 | Reduce model construction verbosity for maximum token savings |
| 14 | [Expand SKILL.md](14-expand-skill-documentation/README.md) | Implemented | Priority #4 | Cover all 8 frameworks so agents know the full API surface |
| 15 | [Incremental Generation](15-incremental-generation/README.md) | Implemented | Priority #5 | Add-file-to-existing-project alongside scaffold-new-project |
| 16 | [Standardize Generation](16-standardize-generation/README.md) | Implemented | Priority #6 | ISyntaxGenerator for code, Liquid templates for config — consistently |
| 17 | [Dry-Run Mode](17-dry-run-mode/README.md) | Implemented | Priority #7 | Preview generated content without writing to disk |
| 18 | [Non-DotNet Parity](18-non-dotnet-parity/README.md) | Implemented | Priority #8 | Bring all non-DotNet modules to minimum capability baseline |
| 19 | [Simplify Strategy Chain](19-simplify-strategy-chain/README.md) | Implemented | Priority #9 | Reduce 4-layer wrapper chain to 2-layer direct dispatch |
| 20 | [Unit Tests](20-unit-tests/README.md) | Implemented | Priority #10 | Add unit test coverage for Core logic |

### YAML-Driven Codebase Scaffolding (FR-19)

| # | Feature | Status | Requirements | Description |
|---|---------|--------|--------------|-------------|
| 21 | [YAML Scaffold Command](21-yaml-scaffold-command/README.md) | Implemented | FR-19.1, FR-19.10, FR-19.11 | CLI `scaffold` command with --dry-run, --force, --validate, --export-schema, --init |
| 22 | [YAML Configuration Schema](22-yaml-configuration-schema/README.md) | Implemented | FR-19.2, FR-19.3, FR-19.4, FR-19.8 | Schema definition, parsing, validation, type mapping, default file generation |
| 23 | [YAML Scaffolding Engine](23-yaml-scaffolding-engine/README.md) | Implemented | FR-19.5, FR-19.6, FR-19.7, FR-19.9 | Architecture patterns, entity/DTO generation, test config, multi-solution/monorepo |

### xregistry/codegen Pattern Adaptations

| # | Feature | Status | Pattern Ref | Description |
|---|---------|--------|-------------|-------------|
| 24 | [Centralized Dependency Management](24-centralized-dependency-management/README.md) | Draft | Pattern 9 | IDependencyResolver with JSON manifests replacing hard-coded versions |
| 25 | [Shared Template Macros](25-shared-template-macros/README.md) | Draft | Pattern 8 | DotLiquid includes from shared `_common/` embedded resource fragments |
| 26 | [Conditional File Generation](26-conditional-file-generation/README.md) | Draft | Pattern 7 | `{% exit %}` tag and skip-on-empty to suppress unnecessary file output |
| 27 | [Additional Template Filters](27-additional-template-filters/README.md) | Draft | Pattern 10 | Custom DotLiquid filters: naming, pluralization, type mapping |
| 28 | [Cross-Template State](28-cross-template-state/README.md) | Draft | Pattern 6 | IGenerationContext with named stacks, key-value store, file tracking |
| 29 | [Resource Tracking](29-resource-tracking/README.md) | Draft | Pattern 13 | MarkHandled/IsHandled to prevent duplicate generation |
| 30 | [Template Metadata Sidecar](30-template-metadata-sidecar/README.md) | Draft | Pattern 3 | `_templateinfo.json` sidecar files for template configuration |
| 31 | [Convention-Based Template Discovery](31-convention-based-template-discovery/README.md) | Draft | Pattern 1 | Template directory structure mirrors output; auto-discovery |
| 32 | [Language/Style Template Matrix](32-language-style-template-matrix/README.md) | Draft | Pattern 2 | Two-level language x style template hierarchy with `_common/` inheritance |
| 33 | [Dynamic Filename Placeholders](33-dynamic-filename-placeholders/README.md) | Draft | Pattern 4 | `{{EntityName}}Controller.cs.liquid` with per-entity iteration |
| 34 | [Schema Normalization Pipeline](34-schema-normalization-pipeline/README.md) | Draft | Pattern 14 | ISchemaNormalizer unifying PlantUML, OpenAPI, JSON Schema inputs |
| 35 | [Underscore-Prefix Ordering](35-underscore-prefix-ordering/README.md) | Draft | Pattern 5 | `_`-prefixed templates render last with enriched context |
| 36 | [Hierarchical Configuration](36-hierarchical-configuration/README.md) | Draft | Pattern 11 | Four-tier config: CLI > env vars > .codegenerator.json > defaults |
| 37 | [Input Validation with JSON Schema](37-input-validation-json-schema/README.md) | Draft | Pattern 12 | Validate YAML/JSON inputs against bundled JSON Schemas before generation |

### CLI Vision — Part 1: Making CodeGenerator.Cli 10/10

| # | Feature | Status | Vision Ref | Description |
|---|---------|--------|------------|-------------|
| 38 | [Bulletproof Error Handling](38-bulletproof-error-handling/README.md) | Draft | Vision 1.1 | Categorized exceptions, meaningful exit codes, rollback on failure |
| 39 | [Input Validation Layer](39-input-validation-layer/README.md) | Draft | Vision 1.2 | GenerationOptionsValidator for CLI inputs before generation |
| 40 | [Rich Console Output](40-rich-console-output/README.md) | Draft | Vision 1.3 | Spectre.Console progress display, tree view, structured errors |
| 41 | [Comprehensive Test Suite](41-comprehensive-test-suite/README.md) | Draft | Vision 1.4 | 90%+ coverage: unit tests, integration tests, Roslyn validation |
| 42 | [Interactive Mode](42-interactive-mode/README.md) | Draft | Vision 1.5 | Spectre.Console prompts when args missing, TTY detection for CI |
| 43 | [Dry-Run and Preview](43-dry-run-and-preview/README.md) | Draft | Vision 1.6 | --dry-run/--what-if flags wired to existing GenerationContext |
| 44 | [Configuration File Support](44-configuration-file-support/README.md) | Draft | Vision 1.7 | .codegenerator.json with 4-tier resolution hierarchy |
| 45 | [Extract Embedded Templates](45-extract-embedded-templates/README.md) | Draft | Vision 1.8 | Move 500+ lines to .liquid embedded resources with user overrides |
| 46 | [Version Consistency](46-version-consistency/README.md) | Draft | Vision 1.9 | Centralize package versions, assembly metadata at runtime |
| 47 | [Shell Completion](47-shell-completion/README.md) | Draft | Vision 1.10 | dotnet-suggest integration for bash, zsh, PowerShell, fish |
| 48 | [Post-Generation Verification](48-post-generation-verification/README.md) | Draft | Vision 1.11 | --verify flag: dotnet build + smoke test on generated output |
| 49 | [Telemetry and Diagnostics](49-telemetry-and-diagnostics/README.md) | Implemented | Vision 1.12 | --diagnostics flag: environment info, per-step timing |
| 50 | [Plugin Discovery](50-plugin-discovery/README.md) | Draft | Vision 1.13 | ICliPlugin interface, NuGet/directory/flag-based assembly loading |

### Production-Grade Error Handling (error-handling-plan.md)

| # | Feature | Status | Plan Phase | Description |
|---|---------|--------|------------|-------------|
| 51 | [Result&lt;T&gt; Type and Error Primitives](51-result-type-error-primitives/README.md) | Implemented | Phase 1 | Result monad, ErrorInfo, ErrorCategory, ErrorCodes registry, expanded exceptions |
| 52 | [Global Exception Handler and Pipeline Aggregation](52-global-exception-handler/README.md) | Implemented | Phase 2 | Program.cs try-catch, ArtifactGenerationResult, ScaffoldResult enrichment |
| 53 | [Resilience Patterns](53-resilience-patterns/README.md) | Implemented | Phase 3 | Retry with backoff, CancellationToken propagation, expanded rollback |
| 54 | [Observability and Error Formatting](54-observability-error-formatting/README.md) | Implemented | Phase 4 | Structured logging, correlation IDs, IErrorFormatter, DiagnosticContext |
| 55 | [Validation Enhancements](55-validation-enhancements/README.md) | Draft | Phase 5 | Fluent Validator&lt;T&gt;, CommonRules, ValidationResult enhancements |
| 56 | [Strategy and Plugin Error Isolation](56-strategy-plugin-error-isolation/README.md) | Draft | Phase 6 | StrategyExecutor boundary, plugin discovery error handling |
| 57 | [Error Handling Test Infrastructure](57-error-handling-test-infrastructure/README.md) | Draft | Phase 7 | ResultAssertions, FaultInjectionOptions, fault-injecting decorators |

### Command Flow Integration

| # | Feature | Status | Integration Ref | Description |
|---|---------|--------|-----------------|-------------|
| 58 | [Config Loader Integration](58-config-loader-integration/README.md) | Draft | Integration 1 | Wire 4-tier config (defaults > .codegenerator.json > env vars > CLI args) into command flow |
| 59 | [Rollback Service Integration](59-rollback-service-integration/README.md) | Draft | Integration 2 | Wire GenerationRollbackService into commands with try/catch/commit/rollback |
| 60 | [Interactive Prompts Integration](60-interactive-prompts-integration/README.md) | Draft | Integration 3 | Wire SpectrePromptService into commands with TTY detection and fallback |
