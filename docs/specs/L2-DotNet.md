# L2 Requirements — .NET Code Generation

**Parent:** [L1-CodeGenerator.md](L1-CodeGenerator.md) — FR-02, FR-03, FR-11, FR-12, FR-16, FR-17
**Status:** Reverse-engineered from source code
**Date:** 2026-04-03

---

## FR-02: .NET Code Generation

### FR-02.1: Class Generation

The framework shall generate C# class declarations with configurable modifiers, inheritance, fields, constructors, methods, properties, and attributes.

**Acceptance Criteria:**
- GIVEN a `ClassModel` with Name, access modifier, and base class, WHEN syntax is generated, THEN a valid C# class declaration with inheritance is produced.
- GIVEN a `ClassModel` with `Static=true`, WHEN generated, THEN the class declaration includes the `static` keyword.
- GIVEN a `ClassModel` with fields and constructors, WHEN generated, THEN field declarations and constructor with parameter initialization are included.
- GIVEN a `ClassModel` with attributes, WHEN generated, THEN attribute declarations (e.g., `[ApiController]`) appear above the class.

### FR-02.2: Interface Generation

The framework shall generate C# interface declarations with method signatures and properties.

**Acceptance Criteria:**
- GIVEN an `InterfaceModel` with methods, WHEN generated, THEN each method appears as a signature without body.
- GIVEN an `InterfaceModel` with generic constraints, WHEN generated, THEN the constraints appear in the declaration.

### FR-02.3: Record Generation

The framework shall generate C# 9+ record types as immutable data classes.

**Acceptance Criteria:**
- GIVEN a `RecordModel` with properties, WHEN generated, THEN a record declaration with positional parameters or init-only properties is produced.

### FR-02.4: Controller Generation

The framework shall generate ASP.NET Core API controllers with route attributes and action methods.

**Acceptance Criteria:**
- GIVEN a `ControllerModel` with entity name, WHEN generated, THEN a controller class with `[ApiController]`, `[Route]`, and constructor-injected dependencies is produced.
- GIVEN controller methods for CRUD operations, WHEN generated, THEN each method has appropriate HTTP method attributes (`[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`), `[ProducesResponseType]`, and `[SwaggerOperation]` attributes.

### FR-02.5: Minimal API Route Handlers

The framework shall generate Minimal API route handlers for GET, GET by ID, POST, PUT/UPDATE, and DELETE operations.

**Acceptance Criteria:**
- GIVEN a `RouteHandlerModel` for Create, WHEN generated, THEN a POST handler with request body, MediatR `Send()`, and response mapping is produced.
- GIVEN a `RouteHandlerModel` for GetById, WHEN generated, THEN a GET handler with route parameter extraction and MediatR dispatch is produced.
- GIVEN a `RouteHandlerModel` for Delete, WHEN generated, THEN a DELETE handler with route parameter and MediatR dispatch is produced.

### FR-02.6: CQRS Command/Query Generation

The framework shall generate MediatR-based CQRS commands, queries, request handlers, responses, and validators.

**Acceptance Criteria:**
- GIVEN a `CommandModel` with entity properties, WHEN generated, THEN a `IRequest<T>` command class, a request handler implementing `IRequestHandler<TRequest, TResponse>`, and a response DTO are produced.
- GIVEN a `QueryModel`, WHEN generated, THEN a query request class and corresponding handler are produced.
- GIVEN a `RequestValidatorModel` with rules, WHEN generated, THEN a FluentValidation `AbstractValidator<T>` with `RuleFor` statements is produced.

### FR-02.7: DDD Entity and Aggregate Generation

The framework shall generate domain entities, aggregates, DTOs, and extension methods following DDD patterns.

**Acceptance Criteria:**
- GIVEN an `EntityModel` with properties, WHEN generated, THEN a domain entity class with typed ID property is produced.
- GIVEN an `AggregateModel`, WHEN generated, THEN the aggregate root class, DTO, extension methods for mapping, and associated commands/queries are all produced.

### FR-02.8: DbContext Generation

The framework shall generate Entity Framework Core DbContext classes and interfaces.

**Acceptance Criteria:**
- GIVEN a `DbContextModel` with entity names, WHEN generated, THEN a DbContext class with `DbSet<T>` properties for each entity is produced.
- GIVEN a `DbContextInterfaceModel`, WHEN generated, THEN an interface defining the DbSet properties and SaveChanges methods is produced.

### FR-02.9: Test Generation

The framework shall generate unit test methods and SpecFlow/BDD test artifacts.

**Acceptance Criteria:**
- GIVEN a test model, WHEN generated, THEN a test method with `[Fact]` or `[Theory]` attribute and Arrange-Act-Assert structure is produced.
- GIVEN a `SpecFlowFeatureModel` with scenarios, WHEN generated, THEN a `.feature` file with Feature/Scenario/Given/When/Then syntax is produced.
- GIVEN a `SpecFlowStepsModel`, WHEN generated, THEN step definition methods with `[Given]`/`[When]`/`[Then]` bindings are produced.

### FR-02.10: Namespace and Document Generation

The framework shall generate complete C# files with namespace declarations, using statements, and type definitions.

**Acceptance Criteria:**
- GIVEN a `DocumentModel` (namespace + class + usings), WHEN generated, THEN a complete C# file with file-scoped namespace, using directives, and class body is produced.

---

## FR-03: Solution Scaffolding

### FR-03.1: Minimal API Solution

The framework shall scaffold a Minimal API solution with an API project and unit test project.

**Acceptance Criteria:**
- GIVEN a solution name and entity, WHEN `Minimal()` is called on `ISolutionFactory`, THEN a solution is created with `src/{Name}.Api` and `tests/{Name}.Api.Tests` projects, with the test project referencing the API project.

### FR-03.2: Clean Architecture Microservice Solution

The framework shall scaffold a Clean Architecture solution with Domain, Application, Infrastructure, and API layers.

**Acceptance Criteria:**
- GIVEN a solution name, WHEN `CleanArchitectureMicroservice()` is called, THEN four projects are created: Domain, Application, Infrastructure, and Api, with correct inter-project dependencies (Api→Application, Api→Infrastructure, Application→Domain, Infrastructure→Domain).

### FR-03.3: Project File Generation

The framework shall generate `.csproj` files with target framework, package references, and project references.

**Acceptance Criteria:**
- GIVEN a `ProjectModel` with packages and references, WHEN the project is generated, THEN a valid `.csproj` with `<PackageReference>` and `<ProjectReference>` elements is produced.

### FR-03.4: Solution File Generation

The framework shall generate `.sln` files with project entries and solution folders.

**Acceptance Criteria:**
- GIVEN a `SolutionModel` with projects, WHEN the solution is generated, THEN a valid `.sln` file with all project references is produced.

---

## FR-11: PlantUML Parsing

### FR-11.1: Class Diagram Parsing

The framework shall parse PlantUML class diagrams into structured models.

**Acceptance Criteria:**
- GIVEN a PlantUML class definition `class User { +name: String; -email: String; +getName(): String }`, WHEN parsed, THEN a `PlantUmlClassModel` is produced with properties (name, email) having correct visibility and a method (getName) with return type.
- GIVEN visibility markers (`+`, `-`, `#`, `~`), WHEN parsed, THEN they map to Public, Private, Protected, and Package visibility respectively.
- GIVEN generic types like `List<User>`, WHEN parsed, THEN the generic type is correctly captured.

### FR-11.2: Relationship Parsing

The framework shall parse PlantUML relationships between classes.

**Acceptance Criteria:**
- GIVEN `User --* Order` (composition), WHEN parsed, THEN a relationship model with correct source, target, and type is produced.
- GIVEN `User --|> Person` (inheritance), WHEN parsed, THEN an inheritance relationship is captured.
- GIVEN `User o-- Address` (aggregation), WHEN parsed, THEN an aggregation relationship is captured.

### FR-11.3: Enum Parsing

The framework shall parse PlantUML enum definitions.

**Acceptance Criteria:**
- GIVEN `enum Status { Active, Inactive, Pending }`, WHEN parsed, THEN a `PlantUmlEnumModel` with three values is produced.

### FR-11.4: Directory and File Parsing

The framework shall parse all `.puml` files in a directory into a solution model.

**Acceptance Criteria:**
- GIVEN a directory with multiple `.puml` files, WHEN `ParseDirectoryAsync()` is called, THEN a `PlantUmlSolutionModel` containing all parsed documents is returned.
- GIVEN a single `.puml` file path, WHEN `ParseFileAsync()` is called, THEN a single `PlantUmlDocumentModel` is returned.

### FR-11.5: Sequence Diagram to Solution Transformation

The framework shall transform PlantUML sequence diagrams into solution architecture models.

**Acceptance Criteria:**
- GIVEN a sequence diagram with participants and messages, WHEN `ISequenceToSolutionPlantUmlService` processes it, THEN a solution model with appropriate classes and methods is generated.

---

## FR-12: Roslyn Code Analysis

### FR-12.1: Project Analysis

The framework shall open and analyze `.csproj` files using MSBuild and Roslyn.

**Acceptance Criteria:**
- GIVEN a path to a `.csproj` file, WHEN the `ICodeAnalysisService` opens it, THEN the project's metadata, references, and source files are accessible.
- GIVEN MSBuild has not been registered, WHEN analysis is first invoked, THEN MSBuild is registered exactly once (thread-safe singleton).

### FR-12.2: Package Detection

The framework shall detect installed NuGet and npm packages.

**Acceptance Criteria:**
- GIVEN a NuGet package name and project directory, WHEN `IsPackageInstalledAsync` is called, THEN `true` is returned if the package exists in the project.
- GIVEN an npm package name, WHEN `IsNpmPackageInstalledAsync` is called, THEN `true` is returned if the package is installed globally.

---

## FR-16: Git Integration

### FR-16.1: Automated Pull Request Creation

The framework shall create GitHub pull requests from feature branches.

**Acceptance Criteria:**
- GIVEN a feature branch with commits and a PR title, WHEN `CreatePullRequestAsync` is called, THEN a PR is created on GitHub from the feature branch to the default branch, and the merge is executed.
- GIVEN the PR is merged, WHEN the operation completes, THEN the local repository switches to the default branch and pulls latest changes.

---

## FR-17: Dependency Injection Automation

### FR-17.1: Service Registration Generation

The framework shall generate and update `ConfigureServices.cs` files with service registrations.

**Acceptance Criteria:**
- GIVEN an interface and implementation name, WHEN `Add(interfaceName, className, directory)` is called, THEN a `services.AddSingleton<IInterface, Class>()` line is added to the `ConfigureServices.cs` in the specified directory.
- GIVEN the registration already exists in the file, WHEN `Add` is called again with the same interface, THEN no duplicate is added.
- GIVEN the `ConfigureServices.cs` does not exist, WHEN `AddConfigureServices(layer, directory)` is called, THEN the file is created with a static class and `Add{Layer}Services()` extension method.

### FR-17.2: Hosted Service Registration

The framework shall register hosted/background services.

**Acceptance Criteria:**
- GIVEN a hosted service name, WHEN `AddHosted(name, directory)` is called, THEN a `services.AddHostedService<Name>()` line is added to ConfigureServices.
