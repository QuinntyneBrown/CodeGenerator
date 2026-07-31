# Contributing to CodeGenerator

Thank you for investing your time in CodeGenerator. Contributions may include code, tests, templates, documentation, issue triage, design feedback, and examples.

By participating, you agree to follow the [Code of Conduct](CODE_OF_CONDUCT.md). Use [SUPPORT.md](SUPPORT.md) for help and [SECURITY.md](SECURITY.md) for suspected vulnerabilities.

## Before you begin

- Search [existing issues](https://github.com/QuinntyneBrown/CodeGenerator/issues) and [pull requests](https://github.com/QuinntyneBrown/CodeGenerator/pulls) before starting work.
- Open an issue before implementing a large feature, changing a public API, adding a target framework, or changing generation semantics.
- Keep changes focused. Avoid unrelated refactoring, formatting, or generated artifacts.
- Never include credentials, access tokens, private source code, customer data, or sensitive generated output in issues, tests, logs, or commits.

## Development setup

### Prerequisites

- Git
- [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- Target-specific tooling only when exercising a generator that invokes it, such as Node.js or Python

### Clone and build

```bash
git clone https://github.com/QuinntyneBrown/CodeGenerator.git
cd CodeGenerator

dotnet restore CodeGenerator.sln
dotnet build CodeGenerator.sln --configuration Release --no-restore
```

The reusable packages target .NET 8 and .NET 9; the CLI targets .NET 9.

## Development workflow

1. Fork the repository and create a branch from the latest `main`.
2. Use a short, descriptive branch name such as `feature/python-records`, `fix/template-resolution`, or `docs/quickstart`.
3. Add or update tests for behavioral changes.
4. Implement the smallest coherent change and keep the affected test suite green.
5. Update the README, package documentation, or [CHANGELOG.md](CHANGELOG.md) when public behavior changes.
6. Open a pull request and respond to review feedback.

Do not commit directly to `main`. Maintainers merge approved pull requests after the required checks pass.

## Design and compatibility expectations

CodeGenerator is a set of independently versioned public packages. Changes should preserve these boundaries:

- `CodeGenerator.Abstractions` contains lightweight contracts and primitives. Avoid adding target-specific or heavyweight dependencies.
- `CodeGenerator.Core` owns cross-target orchestration, configuration, validation, templates, and shared services.
- Target packages own their models, builders, templates, and generation strategies.
- Target packages may depend on Core; Core must not depend on a target package.
- Public API changes require tests and a clear compatibility rationale.
- Breaking changes must be intentional, documented, and released with an appropriate major-version change.
- Generated output should be deterministic where practical and should follow the conventions of its target ecosystem.
- New file-system behavior must preserve dry-run, conflict-resolution, and rollback expectations where those services apply.

When adding a model or generation strategy:

1. Place the model in the target package that owns the output.
2. Implement the corresponding syntax or artifact strategy.
3. Register required services through the target package's service-registration extension.
4. Add focused unit tests for the model, builder, and strategy.
5. Add integration coverage when the change crosses strategy, template, file-system, or command boundaries.
6. Embed or package templates consistently with the target project's existing project file.

## Tests and quality gates

Run the checks relevant to your change before opening a pull request:

```bash
dotnet build CodeGenerator.sln --configuration Release
dotnet test CodeGenerator.sln --configuration Release --no-build
```

For a focused change, run the closest project while iterating:

```bash
dotnet test tests/CodeGenerator.Core.UnitTests/CodeGenerator.Core.UnitTests.csproj
dotnet test tests/CodeGenerator.DotNet.UnitTests/CodeGenerator.DotNet.UnitTests.csproj
dotnet test tests/CodeGenerator.IntegrationTests/CodeGenerator.IntegrationTests.csproj
```

Some generators can invoke external tools. If your change exercises Node.js, Python, Git, or another external runtime, document the versions used and include the relevant test evidence in the pull request.

Documentation-only and repository-metadata changes do not require a runtime test unless they alter executable examples or automation. Check links, commands, and rendered Markdown.

## Commits

Write concise, imperative commit subjects. Examples:

```text
dotnet: add primary-constructor generation
cli: validate scaffold configuration before writes
docs: clarify target package registration
```

Keep each commit internally consistent. Do not include build outputs, package artifacts, temporary files, secrets, or unrelated generated code.

## Pull requests

A pull request should:

- Explain the problem and the resulting behavior
- Link related issues
- Identify affected packages and public APIs
- Include appropriate test evidence
- Call out compatibility, security, dependency, and rollback implications
- Update documentation and the `Unreleased` changelog section when appropriate
- Keep unrelated changes out of the diff

At least one maintainer approval is required. Reviewers may request changes to protect package compatibility, generated-code quality, security, or maintainability.

## Reporting problems

Use the repository issue forms for reproducible defects and feature proposals. Do not use a public issue for a vulnerability; follow [SECURITY.md](SECURITY.md).

## Licensing

By contributing, you agree that your contributions will be licensed under the repository's [MIT License](LICENSE.txt).

## Recognition

Merged human contributions are recognized in [CONTRIBUTORS.md](CONTRIBUTORS.md) and the repository's [contributors graph](https://github.com/QuinntyneBrown/CodeGenerator/graphs/contributors).
