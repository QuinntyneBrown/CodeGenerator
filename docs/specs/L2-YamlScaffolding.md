# L2 Requirements — YAML-Driven Codebase Scaffolding

**Parent:** [L1-CodeGenerator.md](L1-CodeGenerator.md) — FR-19
**Status:** New requirement
**Date:** 2026-04-03

---

## FR-19: YAML-Driven Codebase Scaffolding

### FR-19.1: Scaffold CLI Command

The `CodeGenerator.Cli` shall provide a `scaffold` command that accepts a YAML configuration file path and generates the described codebase.

**Acceptance Criteria:**
1. GIVEN the command `codegen scaffold -c ./scaffold.yaml`, WHEN executed, THEN the codebase described in the YAML file is generated in the directory specified by the configuration's `outputPath` property.
2. GIVEN the command with `-o /custom/path` override, WHEN executed, THEN the output directory override takes precedence over the YAML `outputPath`.
3. GIVEN a YAML file that does not exist at the specified path, WHEN the command is executed, THEN an error message "Configuration file not found: {path}" is returned with exit code 1.
4. GIVEN the `--dry-run` flag, WHEN executed, THEN the command prints the list of files and directories that would be created without writing anything to disk.
5. GIVEN the `--force` flag, WHEN the output directory already exists, THEN existing files are overwritten. Without `--force`, the command shall refuse to overwrite existing files and report which files conflict.
6. GIVEN the `--validate` flag, WHEN executed, THEN the command validates the YAML against the schema and reports all errors without generating any files.

---

### FR-19.2: YAML Configuration Schema — Root Structure

The YAML configuration schema shall define the top-level structure of a scaffold configuration, including metadata, global settings, and project definitions.

**Acceptance Criteria:**
1. GIVEN a YAML configuration, WHEN parsed, THEN the root shall support the following required properties: `name` (string), `version` (string, semver), and `projects` (list of project definitions).
2. GIVEN a YAML configuration, WHEN parsed, THEN the root shall support the following optional properties: `outputPath` (string, default `.`), `description` (string), `globalVariables` (map of key-value pairs for template substitution), `gitInit` (boolean, default false), and `postScaffoldCommands` (list of shell commands to execute after scaffolding).
3. GIVEN `globalVariables` defined at the root level, WHEN any file template references `{{ variableName }}`, THEN the variable value is substituted using the template engine (FR-13).
4. GIVEN the root `name` property set to "Clarity", WHEN referenced in templates, THEN naming convention variants (PascalCase, camelCase, snake_case, kebab-case) are automatically available as `{{ name }}`, `{{ namePascalCase }}`, `{{ nameCamelCase }}`, `{{ nameSnakeCase }}`, `{{ nameKebabCase }}`.

---

### FR-19.3: YAML Configuration Schema — Project Definition

Each project entry in the `projects` list shall describe a single deployable or packageable unit within the codebase.

**Acceptance Criteria:**
1. GIVEN a project definition, WHEN parsed, THEN it shall require: `name` (string), `type` (enum: `dotnet-webapi`, `dotnet-classlib`, `dotnet-console`, `angular-app`, `angular-lib`, `react-app`, `react-lib`, `react-native-app`, `python-app`, `python-lib`, `flask-app`, `playwright-tests`, `detox-tests`, `static-site`, `custom`), and `path` (string, relative to outputPath).
2. GIVEN a project definition, WHEN parsed, THEN it shall support optional properties: `framework` (string, e.g., `net9.0`, `python3.12`, `node20`), `variables` (map, merged with globalVariables with project-level taking precedence), `dependencies` (list), `devDependencies` (list), `directories` (list of directory definitions), `files` (list of file definitions), `references` (list of other project names for inter-project references), and `features` (list of feature toggles).
3. GIVEN `type: dotnet-webapi`, WHEN scaffolded, THEN a .csproj file, Program.cs with builder/app pattern, appsettings.json, and appsettings.Development.json shall be generated with appropriate package references from `dependencies`.
4. GIVEN `type: react-app`, WHEN scaffolded, THEN a package.json, tsconfig.json, vite.config.ts, index.html, and src/ directory with App.tsx and main.tsx shall be generated.
5. GIVEN `type: angular-app`, WHEN scaffolded, THEN an angular.json, package.json, tsconfig.json, and src/ directory with app module/component shall be generated.
6. GIVEN `type: python-app`, WHEN scaffolded, THEN a pyproject.toml (or requirements.txt), __init__.py, and main.py shall be generated.
7. GIVEN `type: flask-app`, WHEN scaffolded, THEN a Flask app factory, config.py, requirements.txt, and application package directory shall be generated.
8. GIVEN `type: playwright-tests`, WHEN scaffolded, THEN a Playwright project with package.json, playwright.config.ts, tsconfig.json, pages/, specs/, and fixtures/ directories shall be generated.
9. GIVEN `type: custom`, WHEN scaffolded, THEN only the explicitly defined `directories` and `files` are created with no implicit files.

---

### FR-19.4: YAML Configuration Schema — Directory and File Definitions

The schema shall allow explicit directory and file definitions within each project for fine-grained control of the scaffold output.

**Acceptance Criteria:**
1. GIVEN a directory definition with `path` (string), WHEN scaffolded, THEN the directory is created relative to the project path. Nested directories shall be created recursively.
2. GIVEN a directory definition with optional `files` (list of file definitions), WHEN scaffolded, THEN each file is created within that directory.
3. GIVEN a file definition, WHEN parsed, THEN it shall require `name` (string, including extension) and one of: `content` (inline string content), `template` (name of an embedded or external template), or `source` (path to a file to copy verbatim).
4. GIVEN a file with `content` property containing template variables (e.g., `{{ projectName }}`), WHEN scaffolded, THEN template substitution is performed using merged global and project variables.
5. GIVEN a file with `template` property referencing an embedded resource template, WHEN scaffolded, THEN the template is located via the template locator (FR-13.3) and rendered with the project's variable context.
6. GIVEN a file with `source` property pointing to an existing file, WHEN scaffolded, THEN the file is copied verbatim without template processing.
7. GIVEN a file definition with optional `encoding` property (default `utf-8`), WHEN scaffolded, THEN the file is written with the specified encoding.

---

### FR-19.5: YAML Configuration Schema — Layer and Architecture Patterns

The schema shall support defining architectural layers and patterns commonly used in production codebases.

**Acceptance Criteria:**
1. GIVEN a project with `architecture` property set to `clean-architecture`, WHEN scaffolded, THEN Domain, Application, Infrastructure, and API/Presentation layer projects are generated with appropriate inter-project references.
2. GIVEN a project with `architecture` property set to `vertical-slices`, WHEN scaffolded, THEN a Features/ directory structure is generated with per-feature folders.
3. GIVEN a project with `layers` list (each with `name`, `type`, and optional `references`), WHEN scaffolded, THEN each layer is generated as a sub-project or directory with the specified inter-layer references.
4. GIVEN a layer with `entities` list (each with `name` and `properties`), WHEN scaffolded, THEN entity/model classes are generated with the specified properties as stub declarations (no business logic).
5. GIVEN a layer with `services` list (each with `name` and `methods`), WHEN scaffolded, THEN service interfaces and empty implementation classes are generated with method signatures matching the definitions.
6. GIVEN a layer with `endpoints` or `controllers` list, WHEN scaffolded, THEN controller classes or route handler files are generated with route stubs (returning placeholder responses).

---

### FR-19.6: YAML Configuration Schema — Entities, Models, and DTOs

The schema shall support defining data structures that are generated as stub classes across the codebase.

**Acceptance Criteria:**
1. GIVEN an entity definition with `name` and `properties` (each with `name`, `type`, and optional `required`, `default`, `description`), WHEN scaffolded for a .NET project, THEN a C# class with the specified properties is generated.
2. GIVEN the same entity definition, WHEN scaffolded for a Python project, THEN a Python class with typed properties (using type hints) is generated.
3. GIVEN the same entity definition, WHEN scaffolded for a TypeScript project (React/Angular), THEN a TypeScript interface with the specified properties is generated.
4. GIVEN a `dtos` list in the configuration, WHEN scaffolded, THEN DTO/request/response classes are generated alongside the entities with only the specified subset of properties.
5. GIVEN property types using cross-platform type aliases (`string`, `int`, `float`, `bool`, `datetime`, `uuid`, `list<T>`, `map<K,V>`), WHEN scaffolded, THEN the types are mapped to the target language's native types (e.g., `int` -> `int` in C#, `int` in Python, `number` in TypeScript).

---

### FR-19.7: YAML Configuration Schema — Testing Configuration

The schema shall support defining test project structures including page objects, test specs, and fixtures.

**Acceptance Criteria:**
1. GIVEN a project of type `playwright-tests` with `pageObjects` list, WHEN scaffolded, THEN page object classes are generated with the specified locators, actions, and queries following the Page Object Model pattern.
2. GIVEN a `pageObjects` entry with `locators` (each with `name`, `strategy`, and `selector`), WHEN scaffolded, THEN locator fields using the specified strategy (GetByTestId, GetByRole, GetByLabel, Locator) are generated.
3. GIVEN a project of type `playwright-tests` with `specs` list, WHEN scaffolded, THEN test spec files with `describe`/`test` blocks are generated with placeholder test bodies.
4. GIVEN a project of type `detox-tests` with `pageObjects` list, WHEN scaffolded, THEN Detox page object classes with testID-based selectors are generated.
5. GIVEN test configuration with `fixtures` list, WHEN scaffolded, THEN custom fixture files are generated with the specified type definitions.

---

### FR-19.8: YAML Schema Validation

The CLI shall validate YAML configuration files against the schema before scaffolding.

**Acceptance Criteria:**
1. GIVEN a YAML file missing a required property (e.g., `name`), WHEN validated, THEN an error "Required property 'name' is missing at path '/'" is reported.
2. GIVEN a YAML file with an invalid `type` enum value (e.g., `type: java-app`), WHEN validated, THEN an error "'java-app' is not a valid project type. Valid types are: dotnet-webapi, dotnet-classlib, ..." is reported.
3. GIVEN a YAML file with a `references` entry pointing to a non-existent project name, WHEN validated, THEN an error "Project reference 'X' not found in configuration" is reported.
4. GIVEN a YAML file with duplicate project names, WHEN validated, THEN an error "Duplicate project name 'X'" is reported.
5. GIVEN a YAML file with a `template` reference that cannot be resolved, WHEN validated, THEN an error "Template 'X' not found" is reported.
6. GIVEN a valid YAML file with no errors, WHEN validated, THEN the message "Configuration is valid" is printed and exit code 0 is returned.

---

### FR-19.9: Multi-Solution and Monorepo Support

The schema shall support scaffolding multi-project solutions and monorepo structures.

**Acceptance Criteria:**
1. GIVEN a configuration with `solutions` list (each containing a `name`, `path`, and `projects` reference list), WHEN scaffolded for .NET, THEN .sln (or .slnx) files are generated with project references to the listed projects.
2. GIVEN a configuration with multiple projects of mixed types (e.g., `dotnet-webapi` + `react-app` + `playwright-tests`), WHEN scaffolded, THEN all projects are generated in their respective paths within the single output directory.
3. GIVEN a configuration with `workspaces` property (npm/yarn workspaces), WHEN scaffolded, THEN a root package.json with workspaces configuration is generated.
4. GIVEN inter-project `references`, WHEN scaffolded for .NET projects, THEN `<ProjectReference>` entries are added to .csproj files. WHEN scaffolded for TypeScript projects, THEN `tsconfig.json` path aliases are configured.

---

### FR-19.10: Post-Scaffold Command Execution

The CLI shall execute configured post-scaffold commands after file generation is complete.

**Acceptance Criteria:**
1. GIVEN `postScaffoldCommands` defined at the root level, WHEN scaffolding completes, THEN each command is executed sequentially in the output directory using the command service (FR-18).
2. GIVEN `postScaffoldCommands` defined at the project level, WHEN scaffolding completes, THEN each command is executed in the project's directory.
3. GIVEN a post-scaffold command that fails (non-zero exit code), WHEN executed, THEN the error is reported but remaining commands continue executing. The overall command exits with code 1.
4. GIVEN the `--dry-run` flag, WHEN scaffolding, THEN post-scaffold commands are listed but not executed.

---

### FR-19.11: Configuration Schema Export

The CLI shall be able to export the YAML configuration schema for editor tooling and documentation.

**Acceptance Criteria:**
1. GIVEN the command `codegen scaffold --export-schema`, WHEN executed, THEN the full JSON Schema for the YAML configuration is written to stdout.
2. GIVEN the command `codegen scaffold --export-schema -o schema.json`, WHEN executed, THEN the JSON Schema is written to the specified file.
3. GIVEN the exported schema, WHEN used in a YAML editor with JSON Schema support, THEN autocompletion and inline validation are available for all configuration properties.
4. GIVEN the command `codegen scaffold --init`, WHEN executed, THEN a minimal example YAML configuration file with commented documentation for all available properties is generated as `scaffold.yaml` in the current directory.
