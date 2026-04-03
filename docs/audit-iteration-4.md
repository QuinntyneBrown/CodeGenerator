# Iteration 4 Audit: Precision Edge-Case Analysis

## Summary

Deep line-by-line trace of all generators identified **8 actionable issues** across formatting, defaults, edge cases, and missing features. The "feature gap" list (composite keys, hybrid properties, etc.) is noted but deferred — those are framework expansion items, not parity fixes.

**Actionable Fixes This Iteration:**

| # | Gap | Severity | Component |
|---|-----|----------|-----------|
| 16 | Table name uses KebobCase (hyphens) not snake_case | HIGH | ModelSyntaxGenerator |
| 17 | Repository custom methods ignore ReturnTypeHint | MEDIUM | RepositorySyntaxGenerator |
| 18 | Repository uses class attribute pattern instead of super().__init__() | MEDIUM | RepositorySyntaxGenerator |
| 19 | Sub-schema fields missing DumpOnly/LoadOnly support | MEDIUM | SchemaSyntaxGenerator |
| 20 | Store actions lack typed signatures | MEDIUM | StoreModel + StoreSyntaxGenerator |
| 21 | Hook body indentation bug (multi-line) | MEDIUM | HookSyntaxGenerator |
| 22 | Indent() calls inconsistent across generators | LOW | Multiple files |
| 23 | Model missing trailing newline after __repr__ | LOW | ModelSyntaxGenerator |

---

## GAP 16 (HIGH): Table Name Uses KebobCase (Hyphens)

### Problem
`ModelSyntaxGenerationStrategy.cs` line ~106:
```csharp
var tableName = model.TableName ?? namingConventionConverter.Convert(NamingConvention.KebobCase, model.Name) + "s";
```
`NamingConvention.KebobCase` in this codebase actually produces **snake_case** (underscores), not hyphenated kebab-case. BUT the naive `+ "s"` pluralization is wrong for some names (e.g., "Category" → "categorys" instead of "categories").

### Fix
The `+ "s"` is a simple pluralization that works for most cases (users, products, orders, reviews). For edge cases like "category", users must set `TableName` explicitly. This is acceptable — just document it. However, we should verify the KebobCase converter actually produces underscores. If it produces hyphens, table names would be `user-s` which would be invalid SQL.

**Action**: Verify KebobCase output; if it produces underscores, the issue is only pluralization (LOW). If hyphens, need to fix (HIGH).

---

## GAP 17 (MEDIUM): Repository Custom Methods Missing ReturnTypeHint

### Problem
`RepositorySyntaxGenerationStrategy.cs` renders custom methods but ignores `method.ReturnTypeHint`:
```csharp
builder.AppendLine($"    def {method.Name}({paramStr}):");
```
Reference: `def find_by_username(self, username: str) -> Optional[User]:`

### Fix
Add return type hint rendering:
```csharp
var returnHint = !string.IsNullOrEmpty(method.ReturnTypeHint) ? $" -> {method.ReturnTypeHint}" : "";
builder.AppendLine($"    def {method.Name}({paramStr}){returnHint}:");
```

---

## GAP 18 (MEDIUM): Repository Uses Class Attribute Instead of super().__init__()

### Problem
Generator produces:
```python
class UserRepository(BaseRepository):
    model = User
```
Reference produces:
```python
class UserRepository(BaseRepository):
    def __init__(self):
        super().__init__(User)
```

### Fix
Add `UseSuperInit: bool` (default true) to `RepositoryModel`. When true, generate `__init__` with `super().__init__()` instead of class attribute.

---

## GAP 19 (MEDIUM): Sub-Schema Fields Missing DumpOnly/LoadOnly

### Problem
Main schema fields support `dump_only=True` and `load_only=True`, but sub-schema field rendering skips these options.

### Fix
Add DumpOnly/LoadOnly rendering to the sub-schema field loop (copy from main field loop).

---

## GAP 20 (MEDIUM): Store Actions Lack Typed Signatures

### Problem
Generator produces: `fetchUsers: (...args: any[]) => void;`
Reference produces: `fetchUsers: (page?: number, perPage?: number) => Promise<void>;`

### Fix
Add `ActionSignatures: Dictionary<string, string>` to `StoreModel`. Maps action name to its typed signature (e.g., `"fetchUsers"` → `"(page?: number, perPage?: number) => Promise<void>"`). When present, use it instead of the generic `(...args: any[]) => void`.

---

## GAP 21 (MEDIUM): Hook Body Indentation Bug

### Problem
`HookSyntaxGenerationStrategy.cs` applies `.Indent()` to the full body string as a single unit. Multi-line bodies only get the first line indented.

### Fix
Split body on `Environment.NewLine` and indent each line, matching the pattern used in `ServiceSyntaxGenerationStrategy`.

---

## GAP 22 (LOW): Inconsistent Indent() Calls

### Problem
Some generators use `.Indent(1)` (1 space), others `.Indent(1, 2)` (2 spaces), others `.Indent(2)` (2 spaces but meaning "indent level 2").

### Fix
Standardize all to `.Indent(level, width)` pattern where width=4 for Python, width=2 for TypeScript.

---

## GAP 23 (LOW): Model Missing Trailing Newline

### Problem
`ModelSyntaxGenerationStrategy` doesn't append a final newline after `__repr__`, unlike other generators.

### Fix
Add `builder.AppendLine()` at the end.

---

## Metrics

| Metric | Iter 1 | Iter 2 | Iter 3 | Iter 4 |
|--------|--------|--------|--------|--------|
| Gaps found | 10 | 5 | 5 | 8 |
| Fixed | 5 | 3 | 5 | TBD |
| Cumulative fixed | 5 | 8 | 13 | TBD |
| Coverage | ~65% | ~85% | ~95% | ~95% |
| Expected after | ~65% | ~85% | ~95% | ~98% |
