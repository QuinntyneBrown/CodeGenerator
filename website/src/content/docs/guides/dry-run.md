---
title: Preview a run with --dry-run
description: See where a scaffold run would put output before it writes anything.
sidebar:
  order: 4
---

Code generation writes many files at once, which makes it awkward to undo by hand. The
`--dry-run` flag computes the plan and writes nothing.

```bash
create-code-cli scaffold --dry-run
```

## What a dry run does not do

- No file or directory is created.
- No [post-scaffold command](/scaffold/root/) runs.
- Rollback is not engaged, because there is nothing to reverse.

Parsing and validation still happen, so a dry run is also a way to check a configuration
end to end without touching the disk.

## What it reports

```
Dry run - files that would be created:
  Create: /work/my-project/MyProject.sln
  Create: /work/my-project/src/MyProject.Api
```

:::caution[Known limitation]
**`--dry-run` does not list individual files.** It lists one path per project and one per solution. A real run creates substantially more files than the preview reports.

Use `--dry-run` to confirm where output lands, not what is produced. To see the full file set, scaffold into an empty temporary directory and list it.

_Tracked as [KL-002](/reference/known-limitations/#kl-002) · Applies to CLI 1.2.1_
:::

## Seeing the full file set

Until the plan reports individual files, scaffold into an empty directory and list it:

```bash
create-code-cli scaffold --output "$(mktemp -d)"
```

On Windows PowerShell:

```powershell
$temp = New-Item -ItemType Directory -Path (Join-Path $env:TEMP (New-Guid))
create-code-cli scaffold --output $temp
Get-ChildItem $temp -Recurse -File | Select-Object FullName
```

## Combining with validation

`--validate` stops after parsing and validation, without computing a plan. It is the
fastest check and the right one for a pre-commit hook:

```bash
create-code-cli scaffold --validate
```

A valid file exits `0`; an invalid one lists every error and exits `1`. See
[exit codes](/reference/exit-codes/).
