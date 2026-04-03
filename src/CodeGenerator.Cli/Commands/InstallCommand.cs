// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Cli.Commands;

public class InstallCommand : Command
{
    private readonly IServiceProvider _serviceProvider;

    public InstallCommand(IServiceProvider serviceProvider)
        : base("install", "Installs a Claude skill for CodeGenerator")
    {
        _serviceProvider = serviceProvider;

        var outputOption = new Option<string>(
            aliases: ["-o", "--output"],
            description: "The target directory (defaults to current directory)",
            getDefaultValue: () => Directory.GetCurrentDirectory());

        AddOption(outputOption);

        this.SetHandler(HandleAsync, outputOption);
    }

    private async Task HandleAsync(string outputDirectory)
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<InstallCommand>>();

        var skillDirectory = Path.Combine(outputDirectory, ".claude", "skills", "code-generator");

        Directory.CreateDirectory(skillDirectory);

        var skillFilePath = Path.Combine(skillDirectory, "SKILL.md");

        await File.WriteAllTextAsync(skillFilePath, SkillContent);

        logger.LogInformation("Installed Claude skill: {Path}", skillFilePath);
    }

    private const string SkillContent = @"---
name: code-generator
description: Generate code using CodeGenerator framework — supports DotNet, Python, Flask, Angular, React, ReactNative, Playwright, Detox
user-invocable: true
---

# CodeGenerator Skill

Use this skill when asked to generate code for .NET, Python, Flask, Angular, React, React Native, Playwright, or Detox using the CodeGenerator framework. Construct models and pass them to `IArtifactGenerator.GenerateAsync()` or `ISyntaxGenerator.GenerateAsync()`.

## Package References

```xml
<PackageReference Include=""QuinntyneBrown.CodeGenerator.Core"" Version=""1.3.0"" />
<PackageReference Include=""QuinntyneBrown.CodeGenerator.DotNet"" Version=""1.2.0"" />
<PackageReference Include=""QuinntyneBrown.CodeGenerator.Angular"" Version=""1.2.0"" />
<PackageReference Include=""QuinntyneBrown.CodeGenerator.React"" Version=""1.2.5"" />
<PackageReference Include=""QuinntyneBrown.CodeGenerator.ReactNative"" Version=""1.2.1"" />
<PackageReference Include=""QuinntyneBrown.CodeGenerator.Python"" Version=""1.2.1"" />
<PackageReference Include=""QuinntyneBrown.CodeGenerator.Flask"" Version=""1.2.6"" />
<PackageReference Include=""QuinntyneBrown.CodeGenerator.Playwright"" Version=""1.2.5"" />
<PackageReference Include=""QuinntyneBrown.CodeGenerator.Detox"" Version=""1.2.1"" />
```

## Service Registration

```csharp
services.AddCoreServices(typeof(Program).Assembly);
services.AddDotNetServices();
services.AddAngularServices();
services.AddReactServices();
services.AddReactNativeServices();
services.AddPythonServices();
services.AddFlaskServices();
services.AddPlaywrightServices();
services.AddDetoxServices();
```

## Core Abstractions

### IArtifactGenerator

Generates files from model objects. Resolve from DI and call `GenerateAsync`:

```csharp
using CodeGenerator.Core.Artifacts.Abstractions;

var artifactGenerator = serviceProvider.GetRequiredService<IArtifactGenerator>();
await artifactGenerator.GenerateAsync(model);  // model can be any file/project/solution model
```

### ISyntaxGenerator

Generates source code strings from syntax models:

```csharp
using CodeGenerator.Core.Syntax;

var syntaxGenerator = serviceProvider.GetRequiredService<ISyntaxGenerator>();
string code = await syntaxGenerator.GenerateAsync(classModel);
```

### ICommandService

Runs shell commands (e.g., `dotnet new sln`):

```csharp
using CodeGenerator.Core.Services;

var commandService = serviceProvider.GetRequiredService<ICommandService>();
commandService.Start(""dotnet new sln -n MySolution"", workingDirectory);
```

### ITemplateProcessor

Processes Liquid templates with token dictionaries:

```csharp
using CodeGenerator.Core.Services;

var templateProcessor = serviceProvider.GetRequiredService<ITemplateProcessor>();
string result = templateProcessor.Process(template, new Dictionary<string, object>
{
    { ""name"", ""MyClass"" },
    { ""namespace"", ""MyApp.Models"" }
});
// Also supports async: await templateProcessor.ProcessAsync(template, tokens)
// And dynamic models: templateProcessor.Process(template, new { name = ""MyClass"" })
```

## DotNet Models

### SolutionModel

Top-level model representing a .NET solution:

```csharp
using CodeGenerator.DotNet.Artifacts.Solutions;

var solution = new SolutionModel(name: ""MySolution"", directory: outputDirectory);
// Key properties:
//   solution.Name              -> ""MySolution""
//   solution.SolutionDirectory -> ""{outputDirectory}/MySolution""
//   solution.SrcDirectory      -> ""{outputDirectory}/MySolution/src""
//   solution.TestDirectory     -> ""{outputDirectory}/MySolution/tests""
//   solution.Projects          -> List<ProjectModel>
//   solution.DependOns          -> List<DependsOnModel>
```

### ProjectModel

Represents a .NET project inside a solution:

```csharp
using CodeGenerator.DotNet.Artifacts.Projects;
using CodeGenerator.DotNet.Artifacts.Projects.Enums;

var project = new ProjectModel(DotNetProjectType.WebApi, ""MyApp.Api"", solution.SrcDirectory);
// Key properties:
//   project.Name             -> ""MyApp.Api""
//   project.Directory        -> ""{srcDir}/MyApp.Api""
//   project.DotNetProjectType -> DotNetProjectType.WebApi
//   project.TargetFramework  -> ""net9.0"" (default; set to ""net8.0"" etc. to change)
//   project.Files            -> List<FileModel>
//   project.Packages         -> List<PackageModel>
//   project.References       -> List<string>
//   project.Namespace        -> ""MyApp.Api"" (defaults to Name)
//   project.Path             -> ""{Directory}/{Name}.csproj""
```

### DotNetProjectType Enum

```csharp
using CodeGenerator.DotNet.Artifacts.Projects.Enums;

// Available values:
// Console, NUnit, XUnit, ClassLib, WebApi, MinimalWebApi, Web, Worker, Angular, TypeScriptStandalone
```

### ContentFileModel

Simple text-based file — use for raw content files:

```csharp
using CodeGenerator.DotNet.Artifacts.Files;

var file = new ContentFileModel(
    content: ""file contents here"",
    name: ""MyFile"",
    directory: outputDirectory,
    extension: "".cs""
);
await artifactGenerator.GenerateAsync(file);
// Writes to: {outputDirectory}/MyFile.cs
```

### CodeFileModel<T>

Typed file model for syntax-based code generation:

```csharp
using CodeGenerator.DotNet.Artifacts.Files;

var file = new CodeFileModel<ClassModel>(classModel, ""MyClass"", directory, "".cs"");
// Contains: Usings, Namespace, syntax object of type T
await artifactGenerator.GenerateAsync(file);
```

### ClassModel

Represents a C# class:

```csharp
using CodeGenerator.DotNet.Syntax.Classes;

var classModel = new ClassModel(""MyService"")
{
    AccessModifier = AccessModifier.Public,
    BaseClass = ""BaseService"",
    Static = false,
    Sealed = false
};
// Add members:
classModel.Properties.Add(new PropertyModel { Name = ""Name"", Type = new TypeModel(""string"") });
classModel.Fields.Add(new FieldModel { Name = ""_logger"", Type = new TypeModel(""ILogger""), ReadOnly = true });
classModel.Constructors.Add(new ConstructorModel { Name = ""MyService"" });
classModel.AddMethod();  // Adds a method with Interface = false
classModel.CreateDto();  // Creates a DTO version of this class
```

### EntityModel

Extends ClassModel for domain entities:

```csharp
using CodeGenerator.DotNet.Syntax.Entities;

var entity = new EntityModel(""Order"")
{
    AggregateRootName = ""OrderAggregate""
};
entity.Properties.Add(new PropertyModel { Name = ""Total"", Type = new TypeModel(""decimal"") });
entity.CreateDto();  // Creates a DTO from this entity
```

### InterfaceModel

Represents a C# interface:

```csharp
using CodeGenerator.DotNet.Syntax.Interfaces;

var iface = new InterfaceModel(""IOrderService"");
iface.Implements.Add(new TypeModel(""IDisposable""));
iface.AddMethod();  // Adds a method with Interface = true
```

### MethodModel

Represents a method signature and body:

```csharp
using CodeGenerator.DotNet.Syntax.Methods;

var method = new MethodModel
{
    Name = ""GetOrderAsync"",
    ReturnType = new TypeModel(""Task<Order>""),
    AccessModifier = AccessModifier.Public,
    Async = true,
    Static = false,
    Override = false,
    Interface = false
};
method.Params.Add(new ParamModel { Name = ""id"", Type = new TypeModel(""Guid"") });
```

### PropertyModel

Represents a C# property:

```csharp
using CodeGenerator.DotNet.Syntax.Properties;

var prop = new PropertyModel
{
    Name = ""OrderId"",
    Type = new TypeModel(""Guid""),
    AccessModifier = AccessModifier.Public,
    Required = false,
    Id = false
};
```

### FieldModel

Represents a C# field:

```csharp
using CodeGenerator.DotNet.Syntax.Fields;

var field = new FieldModel
{
    Name = ""_logger"",
    Type = new TypeModel(""ILogger<MyService>""),
    ReadOnly = true,
    Static = false,
    AccessModifier = AccessModifier.Private
};
// Static helpers:
var mediatorField = FieldModel.Mediator;                 // IMediator _mediator
var loggerField = FieldModel.LoggerOf(""MyService"");   // ILogger<MyService> _logger
```

### ConstructorModel

Represents a constructor:

```csharp
using CodeGenerator.DotNet.Syntax.Constructors;

var ctor = new ConstructorModel
{
    Name = ""MyService"",
    AccessModifier = AccessModifier.Public
};
ctor.Params.Add(new ParamModel { Name = ""logger"", Type = new TypeModel(""ILogger<MyService>"") });
ctor.BaseParams.Add(""logger"");  // Passes to base constructor
```

### TypeModel

Represents a type reference:

```csharp
using CodeGenerator.DotNet.Syntax.Types;

var type = new TypeModel(""string"");
var genericType = new TypeModel(""List<Order>"");
var taskType = new TypeModel(""Task<IActionResult>"");
```

## DotNet Factories

Resolve from DI to create pre-configured models:

### ISolutionFactory

```csharp
using CodeGenerator.DotNet.Artifacts.Solutions.Factories;

var solutionFactory = serviceProvider.GetRequiredService<ISolutionFactory>();
var solution = await solutionFactory.Create(""MySolution"");
```

### IProjectFactory

```csharp
using CodeGenerator.DotNet.Artifacts.Projects.Factories;

var projectFactory = serviceProvider.GetRequiredService<IProjectFactory>();
var apiProject = await projectFactory.CreateWebApi(""MyApp.Api"", srcDirectory);
var libProject = await projectFactory.CreateLibrary(""MyApp.Core"", srcDirectory);
var coreProject = await projectFactory.CreateCore(""MyApp.Core"", srcDirectory);
var infraProject = await projectFactory.CreateInfrastructure(""MyApp.Infrastructure"", srcDirectory);
```

### IClassFactory

```csharp
using CodeGenerator.DotNet.Syntax.Classes.Factories;

var classFactory = serviceProvider.GetRequiredService<IClassFactory>();
var controller = classFactory.CreateController(entityModel, directory);
var dbContext = classFactory.CreateDbContext(""AppDbContext"", entities, directory);
```

### ICqrsFactory

```csharp
using CodeGenerator.DotNet.Syntax.Units.Factories;

var cqrsFactory = serviceProvider.GetRequiredService<ICqrsFactory>();
var query = await cqrsFactory.CreateQueryAsync(""GetById"", ""Order"", ""Guid OrderId"");
var command = await cqrsFactory.CreateCommandAsync(""CreateOrder"", ""string Name, decimal Total"");
```

## Angular Models

### WorkspaceModel

Represents an Angular workspace (monorepo):

```csharp
using CodeGenerator.Angular.Artifacts;

var workspace = new WorkspaceModel(""my-workspace"", ""18.0.0"", rootDirectory);
// Key properties:
//   workspace.Name          -> ""my-workspace""
//   workspace.Version       -> ""18.0.0""
//   workspace.Directory     -> ""{rootDirectory}/my-workspace""
//   workspace.Projects      -> List<ProjectModel>
```

### ProjectModel (Angular)

Represents an Angular project within a workspace:

```csharp
using CodeGenerator.Angular.Artifacts;

var project = new ProjectModel(""my-app"", ""app"", rootDirectory);
// Key properties:
//   project.Name            -> ""my-app""
//   project.Prefix          -> ""app""
//   project.ProjectType     -> ""application""
//   project.Directory       -> computed from root + name
// Supports scoped packages: new ProjectModel(""@scope/my-lib"", ""lib"", rootDirectory)
```

### FunctionModel

Represents a TypeScript/Angular function:

```csharp
using CodeGenerator.Angular.Syntax;

var func = new FunctionModel
{
    Name = ""fetchOrders"",
    Body = ""return this.http.get<Order[]>('/api/orders');""
};
func.Imports.Add(new ImportModel(""HttpClient"", ""@angular/common/http""));
```

### ImportModel

Represents a TypeScript import statement:

```csharp
using CodeGenerator.Angular.Syntax;

var import = new ImportModel(""Component"", ""@angular/core"");
// import.Types  -> List<TypeModel> containing ""Component""
// import.Module -> ""@angular/core""
```

## Python Models

```csharp
using CodeGenerator.Python.Syntax;
using CodeGenerator.Python.Artifacts;

// ClassModel — Python class
var cls = new ClassModel(""UserService"");
cls.Bases.Add(""BaseService"");
cls.Properties.Add(new PropertyModel { Name = ""name"", Type = ""str"" });
cls.Methods.Add(new MethodModel(""get_user"") { Body = ""return self.repo.get(id)"" });
cls.Decorators.Add(new DecoratorModel(""inject""));
cls.Imports.Add(new ImportModel(""typing"", ""Optional""));

// FunctionModel — standalone function
var func = new FunctionModel(""calculate_total"") { Body = ""return sum(items)"", IsAsync = false };
func.Params.Add(new ParamModel { Name = ""items"", TypeHint = new TypeHintModel(""list"") });

// ModuleModel — Python module with imports, classes, functions
var module = new ModuleModel(""services"");
module.Classes.Add(cls);
module.Functions.Add(func);

// ProjectModel — Python project scaffolding
var project = new ProjectModel(""my-app"", Constants.ProjectType.Flask, outputDir);
project.Packages.Add(new PackageModel { Name = ""flask"", Version = ""3.0.0"" });
await artifactGenerator.GenerateAsync(project);
```

## Flask Models

```csharp
using CodeGenerator.Flask.Syntax;
using CodeGenerator.Flask.Artifacts;

// ControllerModel — Flask controller with routes
var controller = new ControllerModel(""user"") { UrlPrefix = ""/api/users"" };
controller.Routes.Add(new ControllerRouteModel {
    Path = ""/"", Methods = [""GET""], HandlerName = ""get_users"",
    Body = ""return jsonify(schema.dump(User.query.all(), many=True))""
});

// ModelModel — SQLAlchemy model
var model = new ModelModel(""User"") { TableName = ""users"" };
model.Columns.Add(new ColumnModel(""id"", ""db.Integer"") { PrimaryKey = true });
model.Columns.Add(new ColumnModel(""name"", ""db.String(100)"") { Nullable = false });

// SchemaModel — Marshmallow schema
var schema = new SchemaModel(""User"");
schema.Fields.Add(new SchemaFieldModel { Name = ""name"", FieldType = ""ma.String"", Required = true });

// ServiceModel — service layer
var service = new ServiceModel(""UserService"");
service.Methods.Add(new ServiceMethodModel { Name = ""get_all"", Body = ""return User.query.all()"" });

// ProjectModel — Flask project scaffolding (creates venv, installs deps, creates structure)
var flaskProject = new ProjectModel(""my-api"", outputDir) { ProjectType = Constants.ProjectType.FlaskApi };
await artifactGenerator.GenerateAsync(flaskProject);
```

## React Models

```csharp
using CodeGenerator.React.Syntax;
using CodeGenerator.React.Artifacts;

// ComponentModel — React functional component
var component = new ComponentModel(""UserCard"");
component.Props.Add(new PropertyModel { Name = ""name"", Type = ""string"" });
component.Props.Add(new PropertyModel { Name = ""email"", Type = ""string"" });
component.BodyContent = ""<div>{name}</div>"";
string tsx = await syntaxGenerator.GenerateAsync(component);

// HookModel — custom React hook
var hook = new HookModel(""useAuth"");
hook.StateProperties.Add(new PropertyModel { Name = ""user"", Type = ""User | null"" });

// StoreModel — Zustand store
var store = new StoreModel(""userStore"");
store.StateProperties.Add(new PropertyModel { Name = ""users"", Type = ""User[]"" });
store.Actions.Add(new ActionModel { Name = ""fetchUsers"", Body = ""/* fetch logic */"" });

// ApiClientModel — Axios API client
var apiClient = new ApiClientModel(""UserApi"") { BaseUrl = ""/api/users"" };
apiClient.Methods.Add(new ApiClientMethodModel { Name = ""getAll"", HttpMethod = ""GET"", Path = ""/"" });

// WorkspaceModel — Vite + React + TypeScript workspace
var workspace = new WorkspaceModel(""my-app"", ""1.0.0"", outputDir);
await artifactGenerator.GenerateAsync(workspace);
```

## React Native Models

```csharp
using CodeGenerator.ReactNative.Syntax;
using CodeGenerator.ReactNative.Artifacts;

// ScreenModel — React Native screen component
var screen = new ScreenModel(""HomeScreen"");
screen.Props.Add(new PropertyModel { Name = ""title"", Type = ""string"" });

// ComponentModel — reusable React Native component
var rnComponent = new ComponentModel(""Avatar"");
rnComponent.Props.Add(new PropertyModel { Name = ""imageUrl"", Type = ""string"" });

// NavigationModel — React Navigation configuration
var nav = new NavigationModel(""AppNavigation"", ""stack"");
nav.Screens.AddRange([""Home"", ""Profile"", ""Settings""]);

// ProjectModel — React Native project scaffolding
var rnProject = new ProjectModel(""MyApp"", outputDir);
await artifactGenerator.GenerateAsync(rnProject);
```

## Playwright Models

```csharp
using CodeGenerator.Playwright.Syntax;
using CodeGenerator.Playwright.Artifacts;

// PageObjectModel — page object with locators and actions
var page = new PageObjectModel(""LoginPage"") { Url = ""/login"" };
page.Locators.Add(new LocatorModel { Name = ""emailInput"", Strategy = ""GetByTestId"", Selector = ""email-input"" });
page.Actions.Add(new PageActionModel { Name = ""login"", Body = ""await this.emailInput.fill(email);"" });
page.Queries.Add(new PageQueryModel { Name = ""getErrorMessage"", ReturnExpression = ""this.errorText.textContent()"" });

// TestSpecModel — Playwright test specification
var testSpec = new TestSpecModel(""Login"");
testSpec.Tests.Add(new TestCaseModel { Name = ""should login successfully"", Body = ""/* test body */"" });

// FixtureModel — custom Playwright fixtures
var fixture = new FixtureModel(""auth"");
fixture.Definitions.Add(new FixtureDefinitionModel { Name = ""loginPage"", Type = ""LoginPage"" });

// ProjectModel — Playwright project scaffolding
var pwProject = new ProjectModel(""e2e-tests"", outputDir);
await artifactGenerator.GenerateAsync(pwProject);
```

## Detox Models

```csharp
using CodeGenerator.Detox.Syntax;
using CodeGenerator.Detox.Artifacts;

// PageObjectModel — mobile page object with testID elements
var page = new PageObjectModel(""LoginPage"");
page.Elements.Add(new PropertyModel { Name = ""emailInput"", TestId = ""email-input"" });
page.Interactions.Add(new InteractionModel { Name = ""login"", Body = ""await this.emailInput.typeText(email);"" });

// TestSpecModel — Detox test specification
var testSpec = new TestSpecModel(""Login"");
testSpec.Tests.Add(new TestModel { Description = ""should login"", Steps = [""await loginPage.login()""] });

// ProjectModel — Detox project scaffolding
var detoxProject = new ProjectModel(""e2e"", outputDir);
await artifactGenerator.GenerateAsync(detoxProject);
```

## Generation Pattern

1. **Create models** — build syntax/artifact models for the target framework
2. **Generate** — pass to `IArtifactGenerator.GenerateAsync(model)` for files or `ISyntaxGenerator.GenerateAsync(model)` for code strings
3. **Builders** — use fluent builders for concise model construction: `ClassBuilder.For(""User"").WithProperty(""Name"", ""string"").Build()`
";
}
