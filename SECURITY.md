# Security Policy

## Supported versions

CodeGenerator packages are versioned independently. Security fixes are applied to the latest stable release of an affected package and to the `main` branch when applicable.

| Version | Supported |
| --- | --- |
| Latest stable package release | Yes |
| Current `main` branch | Best effort |
| Older releases, forks, and generated applications | No |

Consumers should keep all CodeGenerator packages and their transitive dependencies current. The maintainers cannot provide security fixes for generated applications or modified forks.

## Reporting a vulnerability

Do not disclose a suspected vulnerability in a public issue, discussion, pull request, commit, or social channel.

Use [GitHub private vulnerability reporting](https://github.com/QuinntyneBrown/CodeGenerator/security/advisories/new). If GitHub does not offer the private reporting form, contact the [lead maintainer](https://github.com/QuinntyneBrown) through the contact information on their profile. Include:

- The affected package, version, component, or generated output
- A description of the issue and its potential impact
- Reproduction steps or a minimal proof of concept
- Relevant configuration and environment details
- Any suggested mitigation, if known

Do not include credentials, access tokens, private source code, personal data, or unredacted sensitive logs. Use synthetic inputs wherever possible.

Maintainers will acknowledge reports as soon as practical, investigate them, and coordinate remediation and disclosure with the reporter. Response and remediation times are best effort because this is a volunteer-maintained project.

## Scope

Security reports may cover:

- Code execution, path traversal, or unintended file writes in generation workflows
- Unsafe template processing or untrusted-input handling
- Secret exposure in logs, diagnostics, generated files, or configuration
- Dependency vulnerabilities that are exploitable through CodeGenerator
- Generated output that introduces a repeatable security defect under documented use

General bugs, feature requests, and support questions belong in the public [issue tracker](https://github.com/QuinntyneBrown/CodeGenerator/issues).

## Generated-code responsibility

CodeGenerator produces source code and project scaffolds, not production assurances. Consumers are responsible for reviewing generated code, validating dependencies, testing authorization and input handling, protecting secrets, and completing security and privacy reviews appropriate to their deployment.
