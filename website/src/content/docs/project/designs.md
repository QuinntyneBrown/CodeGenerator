---
title: Detailed designs
description: Where the feature-by-feature design documentation lives.
sidebar:
  order: 3
---

The repository carries a detailed design per feature, each with a C4 context, container,
and component view, a class diagram, and a sequence diagram. They are not mirrored here
because the diagrams are rendered images that read best in the repository tree.

Browse them at
[`docs/detailed-designs`](https://github.com/QuinntyneBrown/CodeGenerator/tree/main/docs/detailed-designs).

| Subsystem | Features |
|---|---|
| [generation-engine](https://github.com/QuinntyneBrown/CodeGenerator/tree/main/docs/detailed-designs/generation-engine) | generate-artifact-from-model, render-syntax-from-model, build-models-fluently |
| [template-pipeline](https://github.com/QuinntyneBrown/CodeGenerator/tree/main/docs/detailed-designs/template-pipeline) | render-template-with-tokens, discover-template-set |
| [target-generation](https://github.com/QuinntyneBrown/CodeGenerator/tree/main/docs/detailed-designs/target-generation) | generate-dotnet-solution, generate-full-stack-application, generate-language-targets |
| [declarative-scaffolding](https://github.com/QuinntyneBrown/CodeGenerator/tree/main/docs/detailed-designs/declarative-scaffolding) | scaffold-workspace-from-yaml, resolve-project-architecture |
| [cli-experience](https://github.com/QuinntyneBrown/CodeGenerator/tree/main/docs/detailed-designs/cli-experience) | create-generator-project, resolve-layered-configuration, preview-and-prompt-a-run |
| [run-integrity](https://github.com/QuinntyneBrown/CodeGenerator/tree/main/docs/detailed-designs/run-integrity) | roll-back-a-failed-run, classify-and-render-failures |
| [extensibility](https://github.com/QuinntyneBrown/CodeGenerator/tree/main/docs/detailed-designs/extensibility) | register-strategies-and-plugins, ingest-and-normalize-schemas, apply-changes-incrementally |
| [release-assurance](https://github.com/QuinntyneBrown/CodeGenerator/tree/main/docs/detailed-designs/release-assurance) | verify-and-diagnose-a-run, publish-and-gate-a-release |

Each feature's design cites the level-2 requirements it realizes. Those requirements are
mirrored into this site: [L1](/project/requirements-l1/) and [L2](/project/requirements-l2/).
