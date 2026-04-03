# Iteration 5 — Deep Enterprise Feature Audit

## Summary
Deep feature-level audit of all Flask and React generators against production enterprise patterns. Found 28 new gaps (12 Flask, 16 React). Prioritized by severity for implementation.

## Prior Fixes Verified
All 19 fixes from iterations 1-4 confirmed correct and present.

---

## Flask Gaps (24-35)

| Gap | Component | Severity | Description | Status |
|-----|-----------|----------|-------------|--------|
| 24 | ModelModel/ColumnModel | HIGH | Column Length for String types (`db.String(255)`) | ACTIONABLE |
| 25 | SchemaModel/SchemaFieldModel | MEDIUM | AllowNone property (`allow_none=True`) | ACTIONABLE |
| 26 | ControllerModel | MEDIUM | Query parameter parsing (`request.args.get()`) | ACTIONABLE |
| 27 | BaseRepositoryModel | MEDIUM | Soft delete support | ACTIONABLE |
| 28 | ModelModel/ColumnModel | HIGH | Check constraints (`db.CheckConstraint(...)`) | ACTIONABLE |
| 29 | ModelModel/ColumnModel | MEDIUM | ServerDefault property (`server_default=...`) | ACTIONABLE |
| 30 | BaseRepositoryModel | MEDIUM | Filter/search methods (`filter_by`, `find_first`) | ACTIONABLE |
| 31 | AppFactoryModel | HIGH | Error handler registration (`@app.errorhandler`) | ACTIONABLE |
| 32 | ServiceModel | MEDIUM | Exception/error handling patterns | WORKABLE (use Body) |
| 33 | ConfigModel | MEDIUM | Standard config settings (JSON_SORT_KEYS, etc.) | ACTIONABLE |
| 34 | ModelModel/ColumnModel | LOW | Column comment support | ACTIONABLE |
| 35 | SchemaModel | MEDIUM | Custom validator functions | DEFERRED |

### GAP 24 — Column Length (HIGH)
- **File:** `ModelModel.cs` → Add `int? Length` to ColumnModel
- **File:** `ModelSyntaxGenerationStrategy.cs` → Render `db.String(80)` when Length set
- **Impact:** All String columns currently lack VARCHAR size

### GAP 25 — AllowNone (MEDIUM)
- **File:** `SchemaModel.cs` → Add `bool? AllowNone` to SchemaFieldModel
- **File:** `SchemaSyntaxGenerationStrategy.cs` → Render `allow_none=True/False`

### GAP 26 — Query Parameters (MEDIUM)
- **File:** `ControllerModel.cs` → Add `ControllerRouteQueryParameter` class + list on route
- **File:** `ControllerSyntaxGenerationStrategy.cs` → Render `request.args.get(...)` lines

### GAP 27 — Soft Delete (MEDIUM)
- **File:** `BaseRepositoryModel.cs` → Add `bool UseSoftDelete`, `string SoftDeleteColumn`
- **File:** `BaseRepositorySyntaxGenerationStrategy.cs` → Conditional soft/hard delete

### GAP 28 — Check Constraints (HIGH)
- **File:** `ModelModel.cs` → Add `string? CheckConstraint` to ColumnModel
- **File:** `ModelSyntaxGenerationStrategy.cs` → Render `db.CheckConstraint('expr')`

### GAP 29 — ServerDefault (MEDIUM)
- **File:** `ModelModel.cs` → Add `string? ServerDefault` to ColumnModel
- **File:** `ModelSyntaxGenerationStrategy.cs` → Render `server_default=...`

### GAP 30 — Filter Methods (MEDIUM)
- **File:** `BaseRepositoryModel.cs` → Add `bool IncludeFilterMethods`
- **File:** `BaseRepositorySyntaxGenerationStrategy.cs` → Generate `filter_by()`, `find_first()`

### GAP 31 — Error Handlers (HIGH)
- **File:** `AppFactoryModel.cs` → Add `AppFactoryErrorHandler` class + list
- **File:** `AppFactorySyntaxGenerationStrategy.cs` → Render `@app.errorhandler(N)`

### GAP 33 — Config Settings (MEDIUM)
- **File:** `ConfigModel.cs` → Add `bool IncludeJsonSettings`, `bool IncludeLogging`
- **File:** `ConfigSyntaxGenerationStrategy.cs` → Render standard settings

### GAP 34 — Column Comments (LOW)
- **File:** `ModelModel.cs` → Add `string? Comment` to ColumnModel
- **File:** `ModelSyntaxGenerationStrategy.cs` → Render `comment='...'`

---

## React Gaps (36-51)

| Gap | Component | Severity | Description | Status |
|-----|-----------|----------|-------------|--------|
| 36 | ComponentModel | HIGH | Children prop typing (`children?: React.ReactNode`) | ACTIONABLE |
| 37 | ComponentModel | HIGH | Hook definitions need body, not just names | COMPLEX |
| 38 | ComponentModel | HIGH | forwardRef generic type hardcoded to HTMLDivElement | ACTIONABLE |
| 39 | ComponentModel | MEDIUM | React.memo() wrapper support | ACTIONABLE |
| 40 | PropertyModel | HIGH | readonly keyword for interface properties | ACTIONABLE |
| 41 | TypeScriptInterfaceModel | HIGH | Generic type parameters (`<T>`) | ACTIONABLE |
| 42 | PropertyModel | HIGH | Array element type tracking (`User[]` not `any[]`) | ACTIONABLE |
| 43 | ApiClientModel | HIGH | Error handling with try/catch | ACTIONABLE |
| 44 | ApiClientModel | HIGH | Query parameter support (not URL path params) | ACTIONABLE |
| 45 | ApiClientModel | HIGH | Auth header interceptor | ACTIONABLE |
| 46 | StoreModel | MEDIUM | Async action detection in signatures | ACTIONABLE |
| 47 | StoreModel | HIGH | Error/loading state auto-generation | ACTIONABLE |
| 48 | HookModel | HIGH | Dependency array support for useEffect | ACTIONABLE |
| 49 | ComponentModel | MEDIUM | Conditional rendering support | DEFERRED |
| 50 | ComponentModel | MEDIUM | Props spread operator | ACTIONABLE |
| 51 | HookModel | MEDIUM | Generic type parameters for hooks | ACTIONABLE |

### GAP 36 — Children Prop (HIGH)
- **File:** `ComponentModel.cs` → Add `bool IncludeChildren = true`
- **File:** `ComponentSyntaxGenerationStrategy.cs` → Auto-add `children?: React.ReactNode`

### GAP 38 — forwardRef Element Type (HIGH)
- **File:** `ComponentModel.cs` → Add `string RefElementType = "HTMLDivElement"`
- **File:** `ComponentSyntaxGenerationStrategy.cs` → Use model.RefElementType

### GAP 39 — React.memo (MEDIUM)
- **File:** `ComponentModel.cs` → Add `bool UseMemo = false`
- **File:** `ComponentSyntaxGenerationStrategy.cs` → Wrap in `React.memo()`

### GAP 40 — readonly (HIGH)
- **File:** `PropertyModel.cs` → Add `bool IsReadonly = false`
- **File:** `TypeScriptInterfaceSyntaxGenerationStrategy.cs` → Prefix `readonly`

### GAP 41 — Interface Generics (HIGH)
- **File:** `TypeScriptInterfaceModel.cs` → Add `List<string> TypeParameters`
- **File:** `TypeScriptInterfaceSyntaxGenerationStrategy.cs` → Render `<T, K>`

### GAP 42 — Array Types (HIGH)
- **File:** `PropertyModel.cs` → Add `bool IsArray`, `string? ArrayElementType`
- **File:** `TypeScriptInterfaceSyntaxGenerationStrategy.cs` → Render `User[]`

### GAP 43 — API Error Handling (HIGH)
- **File:** `ApiClientModel.cs` → Add `bool WrapInTryCatch = true`
- **File:** `ApiClientSyntaxGenerationStrategy.cs` → try/catch wrapper

### GAP 44 — API Query Params (HIGH)
- **File:** `ApiClientModel.cs` → Add `QueryParameterModel` class + list on methods
- **File:** `ApiClientSyntaxGenerationStrategy.cs` → `{ params: {...} }` in axios

### GAP 45 — Auth Interceptor (HIGH)
- **File:** `ApiClientModel.cs` → Add `bool IncludeAuthInterceptor`, `string AuthTokenStorageKey`
- **File:** `ApiClientSyntaxGenerationStrategy.cs` → Render interceptor block

### GAP 47 — Store Error/Loading State (HIGH)
- **File:** `StoreModel.cs` → Add `bool IncludeAsyncState = true`
- **File:** `StoreSyntaxGenerationStrategy.cs` → Add isLoading/error to interface + init

### GAP 48 — Hook Dependencies (HIGH)
- **File:** `HookModel.cs` → Add `EffectDefinition` class with Dependencies list
- **File:** `HookSyntaxGenerationStrategy.cs` → Render effects with dependency arrays

### GAP 50 — Props Spread (MEDIUM)
- **File:** `ComponentModel.cs` → Add `bool SpreadProps = false`
- **File:** `ComponentSyntaxGenerationStrategy.cs` → Destructure `{ prop1, ...rest }`

### GAP 51 — Hook Generics (MEDIUM)
- **File:** `HookModel.cs` → Add `List<string> TypeParameters`
- **File:** `HookSyntaxGenerationStrategy.cs` → Render `<T>` after function name

---

## Implementation Priority

### Tier 1 — Simple Property Additions (do first)
Gaps 24, 28, 29, 34 (Flask column properties), 25 (AllowNone), 40 (readonly), 41 (generics), 42 (array types), 38 (RefElementType), 36 (children), 51 (hook generics), 50 (SpreadProps), 39 (memo)

### Tier 2 — Moderate Additions
Gaps 26 (query params), 27 (soft delete), 30 (filter methods), 31 (error handlers), 33 (config), 43 (API try/catch), 44 (API query params), 45 (auth interceptor), 47 (store state), 48 (hook deps), 46 (async detection)

### Tier 3 — Complex/Deferred
Gaps 37 (hook definitions restructure), 49 (conditional rendering), 35 (custom validators)
