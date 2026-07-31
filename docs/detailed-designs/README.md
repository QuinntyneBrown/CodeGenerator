# Detailed designs

This tree holds one detailed design per feature. Each feature folder is a
self-contained page: plain-language background, the concrete components, the
level-2 (L2) requirements the feature realizes, and diagrams rendered inline.

Each design refines the requirements in [`docs/specs`](../specs). Every L2
requirement in `docs/specs/L2.md` is realized by exactly one feature below, and
every requirement table cites the level-1 (L1) requirement its entry refines.

Requirement text in the design tables is stated in `shall` form. The
identifiers are reproduced exactly as `docs/specs/L2.md` writes them, so every
citation resolves back to a declared requirement.

Five requirements are marked **Status: Gap** in the specification: `L2-085`,
`L2-091`, `L2-092`, `L2-093`, and `L2-103`. The designs that carry them describe
the intended control and state plainly that the implementation does not yet meet
it.

## Subsystems

### generation-engine
The two dispatch engines and the model-construction layer above them.

| Feature | Realizes |
|---------|----------|
| [generate-artifact-from-model](generation-engine/generate-artifact-from-model/README.md) | `L2-001` – `L2-007`, `L2-094` |
| [render-syntax-from-model](generation-engine/render-syntax-from-model/README.md) | `L2-008` – `L2-010`, `L2-095` |
| [build-models-fluently](generation-engine/build-models-fluently/README.md) | `L2-011`, `L2-012` |

### template-pipeline
Template rendering and the conventions that turn a folder into a generation plan.

| Feature | Realizes |
|---------|----------|
| [render-template-with-tokens](template-pipeline/render-template-with-tokens/README.md) | `L2-013` – `L2-016` |
| [discover-template-set](template-pipeline/discover-template-set/README.md) | `L2-017` – `L2-022`, `L2-096` |

### target-generation
The language and framework targets, from a single .NET solution to a full-stack
application.

| Feature | Realizes |
|---------|----------|
| [generate-dotnet-solution](target-generation/generate-dotnet-solution/README.md) | `L2-031`, `L2-032`, `L2-035`, `L2-036` |
| [generate-full-stack-application](target-generation/generate-full-stack-application/README.md) | `L2-033`, `L2-034` |
| [generate-language-targets](target-generation/generate-language-targets/README.md) | `L2-023` – `L2-030` |

### declarative-scaffolding
Workspace generation driven by one YAML document.

| Feature | Realizes |
|---------|----------|
| [scaffold-workspace-from-yaml](declarative-scaffolding/scaffold-workspace-from-yaml/README.md) | `L2-040`, `L2-043`, `L2-045` – `L2-047`, `L2-090`, `L2-091` |
| [resolve-project-architecture](declarative-scaffolding/resolve-project-architecture/README.md) | `L2-041`, `L2-042`, `L2-044` |

### cli-experience
The command surface, its configuration, and how a run is previewed.

| Feature | Realizes |
|---------|----------|
| [create-generator-project](cli-experience/create-generator-project/README.md) | `L2-048`, `L2-049`, `L2-052`, `L2-053`, `L2-057`, `L2-058` |
| [resolve-layered-configuration](cli-experience/resolve-layered-configuration/README.md) | `L2-050`, `L2-051`, `L2-054` – `L2-056` |
| [preview-and-prompt-a-run](cli-experience/preview-and-prompt-a-run/README.md) | `L2-068` – `L2-075` |

### run-integrity
What happens when a run fails part-way, and how the failure is reported.

| Feature | Realizes |
|---------|----------|
| [roll-back-a-failed-run](run-integrity/roll-back-a-failed-run/README.md) | `L2-065` – `L2-067`, `L2-087` – `L2-089` |
| [classify-and-render-failures](run-integrity/classify-and-render-failures/README.md) | `L2-059` – `L2-064` |

### extensibility
Adding strategies, ingesting external schemas, and generating into existing
projects.

| Feature | Realizes |
|---------|----------|
| [register-strategies-and-plugins](extensibility/register-strategies-and-plugins/README.md) | `L2-079` – `L2-081`, `L2-092` |
| [ingest-and-normalize-schemas](extensibility/ingest-and-normalize-schemas/README.md) | `L2-037` – `L2-039` |
| [apply-changes-incrementally](extensibility/apply-changes-incrementally/README.md) | `L2-076` – `L2-078` |

### release-assurance
Checking generated output and getting a change to consumers.

| Feature | Realizes |
|---------|----------|
| [verify-and-diagnose-a-run](release-assurance/verify-and-diagnose-a-run/README.md) | `L2-082`, `L2-083`, `L2-093`, `L2-097` – `L2-099` |
| [publish-and-gate-a-release](release-assurance/publish-and-gate-a-release/README.md) | `L2-084` – `L2-086`, `L2-100` – `L2-105` |

## Diagram sources

Each feature folder holds a `diagrams/` sibling containing the PlantUML sources
and the rendered images the design links to. Re-render the whole tree after
editing any source:

```bash
python <skill>/scripts/render_puml.py docs/detailed-designs
```
