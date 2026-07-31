# Project Governance

## Principles

CodeGenerator is maintained in the open. Decisions are guided by correctness, package compatibility, generated-code quality, security, maintainability, and usefulness across supported target ecosystems.

Project discussions and technical decisions should be documented in issues and pull requests whenever they can be public.

## Roles

### Contributors

Anyone who submits an issue, code, tests, templates, documentation, design feedback, review, or another project improvement is a contributor. Contributors must follow [CONTRIBUTING.md](CONTRIBUTING.md) and the [Code of Conduct](CODE_OF_CONDUCT.md).

### Maintainers

Maintainers:

- Triage issues and pull requests
- Review and merge changes
- Protect public API and package compatibility
- Manage package publishing and repository automation
- Coordinate security reports and releases
- Moderate project spaces and enforce community standards

The repository owner, [Quinntyne Brown](https://github.com/QuinntyneBrown), is the current lead maintainer.

## Decision making

Day-to-day decisions are made through issues and pull requests. Maintainers seek rough consensus using test evidence, compatibility impact, documented requirements, and the project's architectural boundaries.

The lead maintainer makes the final repository decision when consensus cannot be reached. Security-sensitive discussions may remain private until coordinated disclosure is complete.

Significant changes should:

1. Begin with a public issue or design proposal when practical.
2. Identify affected packages and compatibility constraints.
3. Record alternatives and tradeoffs.
4. Include tests or validation evidence.
5. Update public documentation before release.

## Releases

Packages are versioned independently. Maintainers determine release timing and apply [Semantic Versioning](https://semver.org/) according to the compatibility impact on each package.

Release notes should identify affected packages, notable behavior changes, breaking changes, migrations, and relevant security fixes. Notable work should be recorded in [CHANGELOG.md](CHANGELOG.md).

## Becoming a maintainer

Maintainers may invite contributors who demonstrate sustained, constructive participation; sound technical judgment; reliable review; care for compatibility and security; and consistent adherence to project standards.

Maintainer access may be removed for inactivity, security reasons, or violations of project policy after reasonable notice when circumstances permit.

## Governance changes

Changes to this document are proposed and reviewed through a pull request. Material changes should remain open long enough for active contributors to respond.
