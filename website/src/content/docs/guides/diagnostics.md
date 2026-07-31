---
title: Diagnose a slow or failing run
description: Use --diagnostics and --verbose to see timings, environment, and failure detail.
sidebar:
  order: 6
---

Two flags make a run explain itself.

## --verbose

Raises logging to `Debug` and includes stack traces in error output.

```bash
create-code-cli scaffold --verbose
```

It is honoured even when the failure happens before command parsing finishes, because the
flag is read from the raw arguments during startup.

Without it, an unanticipated failure prints only a summary:

```
ERROR [INTERNAL_UNEXPECTED] An unexpected error occurred.
Re-run with --verbose to see the full stack trace.
```

## --diagnostics

Prints the execution environment and a per-step timing table.

```bash
create-code-cli scaffold --diagnostics
```

See [the diagnostics report](/troubleshooting/diagnostics-report/) for how to read the
output.

:::caution[Known limitation]
**`--diagnostics` prints nothing when generation fails.** On the root command, the report is rendered after the error-handling block, so a failure skips it. Timings are collected and discarded.

Combine with `--verbose` to see the failure detail. Timings for a failing step are available only by re-running the command against a case that succeeds.

_Tracked as [KL-007](/reference/known-limitations/#kl-007) · Applies to CLI 1.2.1_
:::

The `scaffold` command does render the report on its validation and configuration failure
paths, so `scaffold --validate --diagnostics` reports timings whether or not the document
is valid.

## Correlation identifiers

Every invocation generates a correlation identifier and attaches it to the results it
produces. Two runs never share one, so a scaffold result can be tied back to a single
invocation when several are captured together.

## Using both

The two flags answer different questions and combine well:

```bash
create-code-cli scaffold --verbose --diagnostics
```

`--verbose` explains *what went wrong*; `--diagnostics` explains *where the time went*.
