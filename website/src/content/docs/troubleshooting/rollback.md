---
title: Rollback behaviour
description: What the tool undoes when a run fails, and what it deliberately leaves alone.
sidebar:
  order: 3
---

A generation run creates many files, often after invoking external toolchains. A failure
part-way through would otherwise leave a half-built tree that is neither usable nor
visibly broken. The tool treats a run as a unit of work.

## What is tracked

As a run proceeds it records what it changed:

- **Files created** — deleted on rollback.
- **Files modified** — a backup of the previous content is taken at the moment of
  tracking, and restored on rollback.
- **Directories created** — removed on rollback, but only if still empty.

## Ordering

Reversal happens last-in-first-out, so a directory created before the files inside it is
removed after them.

A failure to reverse one action — a file locked by another process, for example — does not
abort the rest. The remaining actions are still reversed, and the failure is recorded.

## Commit

When a run succeeds, the changes are committed: the undo state is discarded and every
backup file is deleted. A successful run is final, and a later failure in a different run
cannot reach back and undo it.

## What is not rolled back

**Cancellation.** Interrupting with <kbd>Ctrl</kbd>+<kbd>C</kbd> exits `8` and leaves the
files already written in place. This is deliberate: a cancellation is a decision, not a
failure, and silently deleting work in progress would be the wrong response. Clean the
output directory yourself before retrying.

**A successful overwrite.** Rollback undoes a *failed* run. It does not undo a run that
succeeded and replaced a file you wanted to keep — see
[overwriting and file conflicts](/guides/force-and-conflicts/).

**External commands.** Anything a [post-scaffold command](/scaffold/variables/) did is
outside the tool's tracking. A command that installed packages or pushed to a remote is not
reversed.

## Seeing it happen

The failure path logs before reversing:

```
Scaffold failed, rolling back...
```

Run with [`--verbose`](/guides/diagnostics/) to see each reversed path at `Debug` level.
