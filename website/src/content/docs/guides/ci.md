---
title: Run in CI
description: Use the tool unattended, and branch on its exit codes.
sidebar:
  order: 7
---

The tool detects whether standard input is a terminal and adapts. In CI, input is
redirected, so it never prompts — a missing required option becomes a validation failure
instead of a hang.

## Installing in a workflow

```yaml
- uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '9.0.x'

- name: Install create-code-cli
  run: dotnet tool install QuinntyneBrown.CodeGenerator.Cli --tool-path ./.tools

- name: Scaffold
  run: ./.tools/create-code-cli scaffold --config ./scaffold.yaml --output ./generated
```

A `--tool-path` install keeps the agent's global tool set untouched and makes the version
explicit to anyone reading the log.

## Supply every required option

In a non-interactive session the tool does not ask. Omitting `--name` fails:

```
Validation failed:
  - Name: Required option '--name' was not provided and interactive mode is not
    available (stdin is not a terminal). Provide all required options on the
    command line.
```

The process exits `1`.

## Branch on exit codes

Each class of failure has its own code, so a script can react without parsing output:

```bash
create-code-cli scaffold --config ./scaffold.yaml
case $? in
  0) echo "generated" ;;
  1) echo "the configuration is invalid"; exit 1 ;;
  5) echo "no configuration file was found"; exit 1 ;;
  *) echo "unexpected failure"; exit 1 ;;
esac
```

The full list is in the [exit code reference](/reference/exit-codes/).

## Validate configuration on pull requests

Validation is fast and writes nothing, which makes it a good pull-request check:

```yaml
- name: Validate scaffold configuration
  run: ./.tools/create-code-cli scaffold --validate --config ./scaffold.yaml
```

## Getting detail from a failed job

Add `--verbose` to include stack traces, and `--diagnostics` for per-step timings:

```yaml
- name: Scaffold
  run: ./.tools/create-code-cli scaffold --verbose --diagnostics
```

## Cancellation

The tool handles <kbd>Ctrl</kbd>+<kbd>C</kbd> by cancelling in-flight work and exiting `8`.
A cancelled run leaves the files it had already written in place — it is not rolled back,
because a cancellation is not a failure of the run. Treat a `8` from a timed-out job as
"output directory is in an unknown state" and clean it before retrying.
