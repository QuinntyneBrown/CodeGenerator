## Summary

Describe the problem and the resulting behavior.

## Changes

- Describe the key change.

## Validation

List the commands, tests, or manual checks used to validate the change.

```text
dotnet test path/to/affected/tests.csproj
```

## Compatibility and risk

Identify affected packages and note any public API, generated-output, dependency, security, migration, or rollback implications.

## Checklist

- [ ] The change is focused and does not include unrelated edits.
- [ ] Tests cover new or changed behavior.
- [ ] Relevant build and test commands pass locally.
- [ ] Public documentation and examples are updated.
- [ ] `CHANGELOG.md` is updated when the change is notable to consumers.
- [ ] No secrets, private data, build outputs, or unrelated generated files are included.
- [ ] I have read and followed `CONTRIBUTING.md` and the Code of Conduct.
