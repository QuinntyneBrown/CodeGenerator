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
| 24 | [Centralized Dependency Management](24-centralized-dependency-management/README.md) | Proposed | Pattern 14 | Central NuGet/npm version catalog for generated projects |
| 25 | [Shared Template Macros](25-shared-template-macros/README.md) | Proposed | Pattern 3 | Reusable Liquid partials/macros across template libraries |
| 26 | [Conditional File Generation](26-conditional-file-generation/README.md) | Proposed | Pattern 7 | Skip file output when template content is empty or exit tag hit |
| 27 | [Additional Template Filters](27-additional-template-filters/README.md) | Proposed | Pattern 4 | Custom DotLiquid filters for naming, pluralization, type mapping |
| 28 | [Cross-Template State](28-cross-template-state/README.md) | Proposed | Pattern 6 | Named stacks and dictionaries persisted across template renders |
| 29 | [Resource Tracking](29-resource-tracking/README.md) | Proposed | Pattern 13 | mark_handled/is_handled to prevent duplicate generation |
| 30 | [Template Metadata Sidecar](30-template-metadata-sidecar/README.md) | Proposed | Pattern 9 | .meta.json sidecar files for template configuration |
| 31 | [Convention-Based Template Discovery](31-convention-based-template-discovery/README.md) | Proposed | Pattern 1 | Template directory structure mirrors output; auto-discovery |
| 32 | [Language/Style Template Matrix](32-language-style-template-matrix/README.md) | Proposed | Pattern 2 | Two-level language/style template hierarchy |
| 33 | [Dynamic Filename Placeholders](33-dynamic-filename-placeholders/README.md) | Proposed | Pattern 8 | Template filenames with {model} placeholders resolved at render time |
| 34 | [Schema Normalization Pipeline](34-schema-normalization-pipeline/README.md) | Proposed | Pattern 10 | Normalize input schemas before generation |
| 35 | [Underscore-Prefix Ordering](35-underscore-prefix-ordering/README.md) | Proposed | Pattern 5 | Templates prefixed with _ render after regular templates with enriched context |
| 36 | [Hierarchical Configuration](36-hierarchical-configuration/README.md) | Proposed | Pattern 11 | Four-tier config: CLI > env vars > .codegenerator.json > defaults |
| 37 | [Input Validation with JSON Schema](37-input-validation-json-schema/README.md) | Proposed | Pattern 12 | Validate YAML/JSON inputs against bundled JSON Schemas before generation |
