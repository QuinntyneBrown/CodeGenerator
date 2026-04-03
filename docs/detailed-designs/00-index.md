# Detailed Designs — Index

**Parent:** [L1-CodeGenerator.md](../specs/L1-CodeGenerator.md)
**Date:** 2026-04-03

| # | Feature | Status | Requirements | Description |
|---|---------|--------|--------------|-------------|
| 01 | [Core Generation Engine](01-core-generation-engine/README.md) | Draft | FR-01, FR-13, FR-14, FR-18 | Strategy-based dispatch, template engine, naming conventions, command execution |
| 02 | [.NET Code Generation](02-dotnet-code-generation/README.md) | Draft | FR-02, FR-03, FR-11, FR-12, FR-16, FR-17 | C# syntax, solution scaffolding, CQRS, DDD, PlantUML, Roslyn, Git |
| 03 | [Python Code Generation](03-python-code-generation/README.md) | Draft | FR-04 | Python classes, functions, modules, decorators, type hints, project scaffolding |
| 04 | [Flask Backend Generation](04-flask-backend-generation/README.md) | Draft | FR-05 | Flask app factories, controllers, SQLAlchemy models, repositories, schemas |
| 05 | [Angular Generation](05-angular-generation/README.md) | Draft | FR-06 | Angular workspaces, standalone components, Jest config, barrel files |
| 06 | [React Generation](06-react-generation/README.md) | Draft | FR-07 | React + Vite + TypeScript components, hooks, Zustand stores, API clients |
| 07 | [React Native Generation](07-react-native-generation/README.md) | Draft | FR-08 | React Native screens, components, navigation, Zustand stores, styles |
| 08 | [Playwright Test Generation](08-playwright-test-generation/README.md) | Draft | FR-09 | Playwright page objects, test specs, fixtures, multi-browser config |
| 09 | [Detox Test Generation](09-detox-test-generation/README.md) | Draft | FR-10 | Detox mobile page objects, Jest test specs, iOS/Android configuration |
| 10 | [CLI Tool](10-cli-tool/README.md) | Draft | FR-15 | `create-code-cli` global tool for scaffolding code generator projects |

### Priority Action Designs

| # | Feature | Status | Audit Ref | Description |
|---|---------|--------|-----------|-------------|
| 11 | [Extract Abstractions](11-extract-abstractions/README.md) | Draft | Priority #1 | Decouple Core from heavy dependencies into lightweight Abstractions package |
| 12 | [Model Validation](12-model-validation/README.md) | Draft | Priority #2 | Add IValidatable and validation pipeline to prevent broken agent output |
| 13 | [Builder/Fluent APIs](13-builder-fluent-apis/README.md) | Draft | Priority #3 | Reduce model construction verbosity for maximum token savings |
| 14 | [Expand SKILL.md](14-expand-skill-documentation/README.md) | Draft | Priority #4 | Cover all 8 frameworks so agents know the full API surface |
| 15 | [Incremental Generation](15-incremental-generation/README.md) | Draft | Priority #5 | Add-file-to-existing-project alongside scaffold-new-project |
| 16 | [Standardize Generation](16-standardize-generation/README.md) | Draft | Priority #6 | ISyntaxGenerator for code, Liquid templates for config — consistently |
| 17 | [Dry-Run Mode](17-dry-run-mode/README.md) | Draft | Priority #7 | Preview generated content without writing to disk |
| 18 | [Non-DotNet Parity](18-non-dotnet-parity/README.md) | Draft | Priority #8 | Bring all non-DotNet modules to minimum capability baseline |
| 19 | [Simplify Strategy Chain](19-simplify-strategy-chain/README.md) | Draft | Priority #9 | Reduce 4-layer wrapper chain to 2-layer direct dispatch |
| 20 | [Unit Tests](20-unit-tests/README.md) | Draft | Priority #10 | Add unit test coverage for Core logic |

### YAML-Driven Codebase Scaffolding (FR-19)

| # | Feature | Status | Requirements | Description |
|---|---------|--------|--------------|-------------|
| 21 | [YAML Scaffold Command](21-yaml-scaffold-command/README.md) | Draft | FR-19.1, FR-19.10, FR-19.11 | CLI `scaffold` command with --dry-run, --force, --validate, --export-schema, --init |
| 22 | [YAML Configuration Schema](22-yaml-configuration-schema/README.md) | Draft | FR-19.2, FR-19.3, FR-19.4, FR-19.8 | Schema definition, parsing, validation, type mapping, default file generation |
| 23 | [YAML Scaffolding Engine](23-yaml-scaffolding-engine/README.md) | Draft | FR-19.5, FR-19.6, FR-19.7, FR-19.9 | Architecture patterns, entity/DTO generation, test config, multi-solution/monorepo |
