---
title: The diagnostics report
description: How to read the environment and timing output from --diagnostics.
sidebar:
  order: 2
---

`--diagnostics` prints two tables after a run.

```bash
create-code-cli scaffold --diagnostics
```

## Environment

| Row | Meaning |
|---|---|
| CLI Version | The version of `create-code-cli` that ran. |
| .NET SDK | The SDK resolved on this machine. |
| Runtime | The runtime the tool executed on. |
| OS | Operating system description. |
| Architecture | Process architecture, for example `X64` or `Arm64`. |
| Shell | Taken from `SHELL`, then `COMSPEC`, otherwise `unknown`. |
| Working Directory | The directory the tool was invoked from. |

This is the block to paste into a bug report.

## Step timings

Each named step is listed with its duration and a proportional bar, followed by a total.
Durations under one second are shown in milliseconds.

The step names are fixed, so a slow run can be attributed precisely.

**Root command:** `Validate options`, `Create directories`, `Create solution file`,
`Generate .csproj`, `Generate project files`, `Add project to solution`,
`Generate install script`.

**`scaffold`:** `Load configuration file`, `Validate YAML`, `Scaffold files`.

## Interpreting a slow run

`Create solution file` and `Add project to solution` shell out to the .NET SDK, so time
spent there is the SDK's, not the generator's. A slow `Scaffold files` step is the
generator's own work.

## When no report appears

:::caution[Known limitation]
**`--diagnostics` prints nothing when generation fails.** On the root command, the report is rendered after the error-handling block, so a failure skips it. Timings are collected and discarded.

Combine with `--verbose` to see the failure detail. Timings for a failing step are available only by re-running the command against a case that succeeds.

_Tracked as [KL-007](/reference/known-limitations/#kl-007) · Applies to CLI 1.2.1_
:::

Without the flag, timing is disabled entirely rather than measured and discarded, so
`--diagnostics` costs nothing when it is not asked for.
