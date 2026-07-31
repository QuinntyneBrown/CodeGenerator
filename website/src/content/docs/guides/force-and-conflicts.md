---
title: Overwriting and file conflicts
description: What happens when a scaffold run targets a path that already has a file.
sidebar:
  order: 5
---

:::danger[Destructive behavior]
**`--force` has no effect.** Scaffolding overwrites any existing file at a target path whether or not `--force` is passed. The flag is accepted, validated, and never read.

Commit or back up your work before scaffolding into a directory that already contains files. Run with `--dry-run` first to see which paths are targeted.

_Tracked as [KL-001](/reference/known-limitations/#kl-001) · Applies to CLI 1.2.1_
:::

## What actually happens

`create-code-cli scaffold` writes each planned file unconditionally. An existing file at
the same path is replaced, and its previous contents are not recoverable from the tool.

This is the same with and without `--force`.

## Working safely

**Commit first.** The most reliable protection is version control. With a clean working
tree, `git diff` after a scaffold run shows exactly what changed and `git checkout` undoes
it.

**Scaffold into an empty directory.** Generate into a temporary location and copy in what
you want:

```bash
create-code-cli scaffold --output ./generated
```

**Preview the target paths.** [`--dry-run`](/guides/dry-run/) reports the project and
solution paths a run would write under, which is enough to see whether it overlaps
existing work.

## Rollback is not a substitute

The tool reverses its own filesystem changes when a run *fails* — see
[rollback](/troubleshooting/rollback/). That is a different mechanism: it undoes a partial
run that threw, not a successful run that overwrote something you wanted to keep. A
successful overwrite is committed and final.
