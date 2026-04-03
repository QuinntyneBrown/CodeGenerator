// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Scaffold.Models;
using CodeGenerator.Core.Scaffold.Services;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class YamlScaffoldingTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly string _workspaceRoot;

    public YamlScaffoldingTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddCoreServices(typeof(YamlScaffoldingTests).Assembly);
        services.AddDotNetServices();
        services.AddScaffoldingServices();
        services.AddSingleton<ICommandService, NoOpCommandService>();

        _serviceProvider = services.BuildServiceProvider();
        _workspaceRoot = Path.Combine(Path.GetTempPath(), $"scaffold-test-{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        if (Directory.Exists(_workspaceRoot))
        {
            Directory.Delete(_workspaceRoot, true);
        }
    }

    #region DD-22: YAML Configuration Schema - Parsing

    [Fact]
    public void YamlConfigParser_ParsesMinimalConfig()
    {
        var parser = _serviceProvider.GetRequiredService<IYamlConfigParser>();

        var yaml = """
            name: TestProject
            version: 1.0.0
            projects:
              - name: MyApi
                type: dotnet-webapi
                path: src/MyApi
            """;

        var config = parser.Parse(yaml);

        Assert.Equal("TestProject", config.Name);
        Assert.Equal("1.0.0", config.Version);
        Assert.Single(config.Projects);
        Assert.Equal("MyApi", config.Projects[0].Name);
        Assert.Equal(ScaffoldProjectType.DotnetWebapi, config.Projects[0].Type);
        Assert.Equal("src/MyApi", config.Projects[0].Path);
    }

    [Fact]
    public void YamlConfigParser_ParsesMultiProjectConfig()
    {
        var parser = _serviceProvider.GetRequiredService<IYamlConfigParser>();

        var yaml = """
            name: Enterprise
            version: 2.0.0
            description: Multi-project solution
            outputPath: .
            gitInit: true
            globalVariables:
              author: TestAuthor
              company: TestCorp
            projects:
              - name: Enterprise.Api
                type: dotnet-webapi
                path: src/Enterprise.Api
                framework: net9.0
                references:
                  - Enterprise.Domain
                entities:
                  - name: Order
                    properties:
                      - name: id
                        type: uuid
                        required: true
                      - name: total
                        type: float
                      - name: items
                        type: list<string>
              - name: Enterprise.Domain
                type: dotnet-classlib
                path: src/Enterprise.Domain
              - name: Enterprise.Web
                type: react-app
                path: src/Enterprise.Web
            solutions:
              - name: Enterprise
                projects:
                  - Enterprise.Api
                  - Enterprise.Domain
            postScaffoldCommands:
              - dotnet build
            """;

        var config = parser.Parse(yaml);

        Assert.Equal("Enterprise", config.Name);
        Assert.Equal("2.0.0", config.Version);
        Assert.Equal("Multi-project solution", config.Description);
        Assert.True(config.GitInit);
        Assert.Equal("TestAuthor", config.GlobalVariables["author"]);
        Assert.Equal(3, config.Projects.Count);
        Assert.Single(config.Solutions);
        Assert.Equal(2, config.Solutions[0].Projects.Count);
        Assert.Single(config.PostScaffoldCommands);

        var apiProject = config.Projects.First(p => p.Name == "Enterprise.Api");
        Assert.Equal(ScaffoldProjectType.DotnetWebapi, apiProject.Type);
        Assert.Equal("net9.0", apiProject.Framework);
        Assert.Single(apiProject.References);
        Assert.Single(apiProject.Entities);

        var orderEntity = apiProject.Entities[0];
        Assert.Equal("Order", orderEntity.Name);
        Assert.Equal(3, orderEntity.Properties.Count);
        Assert.Equal("uuid", orderEntity.Properties[0].Type);
        Assert.True(orderEntity.Properties[0].Required);
    }

    [Fact]
    public void YamlConfigParser_ParsesPlaywrightTestProject()
    {
        var parser = _serviceProvider.GetRequiredService<IYamlConfigParser>();

        var yaml = """
            name: E2ETests
            version: 1.0.0
            projects:
              - name: MyApp.E2E
                type: playwright-tests
                path: tests/e2e
                pageObjects:
                  - name: Login
                    url: /login
                    locators:
                      - name: emailInput
                        strategy: GetByLabel
                        value: Email
                      - name: submitButton
                        strategy: GetByRole
                        value: button
                specs:
                  - name: LoginFlow
                    page: Login
                    tests:
                      - should display login form
                      - should login with valid credentials
                fixtures:
                  - name: User
                    properties:
                      email: string
                      password: string
            """;

        var config = parser.Parse(yaml);

        var project = config.Projects[0];
        Assert.Equal(ScaffoldProjectType.PlaywrightTests, project.Type);
        Assert.Single(project.PageObjects);
        Assert.Equal("Login", project.PageObjects[0].Name);
        Assert.Equal("/login", project.PageObjects[0].Url);
        Assert.Equal(2, project.PageObjects[0].Locators.Count);
        Assert.Equal("GetByLabel", project.PageObjects[0].Locators[0].Strategy);

        Assert.Single(project.Specs);
        Assert.Equal(2, project.Specs[0].Tests.Count);

        Assert.Single(project.Fixtures);
        Assert.Equal(2, project.Fixtures[0].Properties.Count);
    }

    [Fact]
    public void YamlConfigParser_ThrowsOnInvalidYaml()
    {
        var parser = _serviceProvider.GetRequiredService<IYamlConfigParser>();

        var yaml = """
            name: [invalid
            version: {{bad
            """;

        Assert.Throws<ScaffoldParseException>(() => parser.Parse(yaml));
    }

    #endregion

    #region DD-22: YAML Configuration Schema - Validation

    [Fact]
    public void ConfigValidator_ValidatesRequiredFields()
    {
        var validator = _serviceProvider.GetRequiredService<IConfigValidator>();

        var config = new ScaffoldConfiguration();

        var result = validator.Validate(config);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "name");
        Assert.Contains(result.Errors, e => e.PropertyName == "version");
        Assert.Contains(result.Errors, e => e.PropertyName == "projects");
    }

    [Fact]
    public void ConfigValidator_ValidatesSemver()
    {
        var validator = _serviceProvider.GetRequiredService<IConfigValidator>();

        var config = new ScaffoldConfiguration
        {
            Name = "Test",
            Version = "not-a-version",
            Projects = [new ProjectDefinition { Name = "P1", Path = "src/P1" }],
        };

        var result = validator.Validate(config);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "version");
    }

    [Fact]
    public void ConfigValidator_DetectsDuplicateProjectNames()
    {
        var validator = _serviceProvider.GetRequiredService<IConfigValidator>();

        var config = new ScaffoldConfiguration
        {
            Name = "Test",
            Version = "1.0.0",
            Projects =
            [
                new ProjectDefinition { Name = "MyProject", Path = "src/a" },
                new ProjectDefinition { Name = "MyProject", Path = "src/b" },
            ],
        };

        var result = validator.Validate(config);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Duplicate project name"));
    }

    [Fact]
    public void ConfigValidator_DetectsPathTraversal()
    {
        var validator = _serviceProvider.GetRequiredService<IConfigValidator>();

        var config = new ScaffoldConfiguration
        {
            Name = "Test",
            Version = "1.0.0",
            Projects =
            [
                new ProjectDefinition { Name = "Evil", Path = "../../etc" },
            ],
        };

        var result = validator.Validate(config);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Path traversal"));
    }

    [Fact]
    public void ConfigValidator_DetectsUnresolvedReferences()
    {
        var validator = _serviceProvider.GetRequiredService<IConfigValidator>();

        var config = new ScaffoldConfiguration
        {
            Name = "Test",
            Version = "1.0.0",
            Projects =
            [
                new ProjectDefinition
                {
                    Name = "MyApi",
                    Path = "src/api",
                    References = ["NonExistent"],
                },
            ],
        };

        var result = validator.Validate(config);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("not found"));
    }

    [Fact]
    public void ConfigValidator_ValidatesFileContentExclusivity()
    {
        var validator = _serviceProvider.GetRequiredService<IConfigValidator>();

        var config = new ScaffoldConfiguration
        {
            Name = "Test",
            Version = "1.0.0",
            Projects =
            [
                new ProjectDefinition
                {
                    Name = "MyProject",
                    Path = "src/proj",
                    Files =
                    [
                        new FileDefinition
                        {
                            Name = "bad.txt",
                            Content = "inline content",
                            Template = "some-template",
                        },
                    ],
                },
            ],
        };

        var result = validator.Validate(config);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("exactly one"));
    }

    [Fact]
    public void ConfigValidator_PassesValidConfig()
    {
        var validator = _serviceProvider.GetRequiredService<IConfigValidator>();

        var config = new ScaffoldConfiguration
        {
            Name = "ValidProject",
            Version = "1.0.0",
            Projects =
            [
                new ProjectDefinition
                {
                    Name = "MyApi",
                    Path = "src/MyApi",
                    Type = ScaffoldProjectType.DotnetWebapi,
                },
            ],
        };

        var result = validator.Validate(config);

        Assert.True(result.IsValid);
    }

    #endregion

    #region DD-22: Type Mapping

    [Theory]
    [InlineData("string", "csharp", "string")]
    [InlineData("int", "csharp", "int")]
    [InlineData("float", "csharp", "double")]
    [InlineData("bool", "csharp", "bool")]
    [InlineData("datetime", "csharp", "DateTime")]
    [InlineData("uuid", "csharp", "Guid")]
    [InlineData("string", "typescript", "string")]
    [InlineData("int", "typescript", "number")]
    [InlineData("bool", "typescript", "boolean")]
    [InlineData("datetime", "typescript", "Date")]
    [InlineData("uuid", "typescript", "string")]
    [InlineData("string", "python", "str")]
    [InlineData("int", "python", "int")]
    [InlineData("float", "python", "float")]
    [InlineData("bool", "python", "bool")]
    [InlineData("datetime", "python", "datetime")]
    [InlineData("uuid", "python", "UUID")]
    public void TypeMapper_MapsBasicTypes(string alias, string language, string expected)
    {
        var mapper = _serviceProvider.GetRequiredService<ITypeMapper>();

        var result = mapper.Map(alias, language);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("list<string>", "csharp", "List<string>")]
    [InlineData("list<int>", "typescript", "number[]")]
    [InlineData("list<string>", "python", "list[str]")]
    [InlineData("map<string, int>", "csharp", "Dictionary<string, int>")]
    [InlineData("map<string, int>", "typescript", "Record<string, number>")]
    [InlineData("map<string, int>", "python", "dict[str, int]")]
    public void TypeMapper_MapsGenericTypes(string alias, string language, string expected)
    {
        var mapper = _serviceProvider.GetRequiredService<ITypeMapper>();

        var result = mapper.Map(alias, language);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TypeMapper_PassesThroughUnknownTypes()
    {
        var mapper = _serviceProvider.GetRequiredService<ITypeMapper>();

        var result = mapper.Map("CustomType", "csharp");

        Assert.Equal("CustomType", result);
    }

    #endregion

    #region DD-21: Schema Exporter

    [Fact]
    public void SchemaExporter_ExportsValidJsonSchema()
    {
        var exporter = _serviceProvider.GetRequiredService<ISchemaExporter>();

        var schema = exporter.ExportJsonSchema();

        Assert.Contains("\"title\"", schema);
        Assert.Contains("ScaffoldConfiguration", schema);
        Assert.Contains("\"required\"", schema);
        Assert.Contains("name", schema);
        Assert.Contains("version", schema);
        Assert.Contains("projects", schema);
    }

    [Fact]
    public void SchemaExporter_GeneratesStarterYaml()
    {
        var exporter = _serviceProvider.GetRequiredService<ISchemaExporter>();

        var yaml = exporter.GenerateStarterYaml();

        Assert.Contains("name:", yaml);
        Assert.Contains("version:", yaml);
        Assert.Contains("projects:", yaml);
    }

    [Fact]
    public void SchemaExporter_StarterYamlIsParseable()
    {
        var exporter = _serviceProvider.GetRequiredService<ISchemaExporter>();
        var parser = _serviceProvider.GetRequiredService<IYamlConfigParser>();

        var yaml = exporter.GenerateStarterYaml();
        var config = parser.Parse(yaml);

        Assert.NotNull(config);
        Assert.False(string.IsNullOrEmpty(config.Name));
        Assert.False(string.IsNullOrEmpty(config.Version));
        Assert.NotEmpty(config.Projects);
    }

    #endregion

    #region DD-21: ScaffoldEngine Validate

    [Fact]
    public void ScaffoldEngine_ValidateReturnsErrorsForInvalidConfig()
    {
        var engine = _serviceProvider.GetRequiredService<IScaffoldEngine>();

        var yaml = """
            name: ""
            version: bad
            projects: []
            """;

        var result = engine.Validate(yaml);

        Assert.False(result.Success);
        Assert.False(result.ValidationResult.IsValid);
    }

    [Fact]
    public void ScaffoldEngine_ValidatePassesForValidConfig()
    {
        var engine = _serviceProvider.GetRequiredService<IScaffoldEngine>();

        var yaml = """
            name: Valid
            version: 1.0.0
            projects:
              - name: MyApi
                type: dotnet-webapi
                path: src/MyApi
            """;

        var result = engine.Validate(yaml);

        Assert.True(result.Success);
        Assert.True(result.ValidationResult.IsValid);
    }

    #endregion

    #region DD-23: Architecture Resolver

    [Fact]
    public void ArchitectureResolver_ResolvesCleanArchitecture()
    {
        var resolver = _serviceProvider.GetRequiredService<IArchitectureResolver>();

        var project = new ProjectDefinition
        {
            Name = "Contoso",
            Architecture = "clean-architecture",
            Entities =
            [
                new EntityDefinition { Name = "Order", Properties = [new PropertyDefinition { Name = "id", Type = "uuid" }] },
            ],
            Endpoints =
            [
                new EndpointDefinition { Name = "GetOrders", Method = "GET", Route = "/api/orders" },
            ],
        };

        var result = resolver.Resolve(project);

        Assert.Equal("clean-architecture", result.Pattern);
        Assert.Equal(4, result.Layers.Count);

        Assert.Contains(result.Layers, l => l.Name == "Contoso.Domain");
        Assert.Contains(result.Layers, l => l.Name == "Contoso.Application");
        Assert.Contains(result.Layers, l => l.Name == "Contoso.Infrastructure");
        Assert.Contains(result.Layers, l => l.Name == "Contoso.Api");

        var domain = result.Layers.First(l => l.Name == "Contoso.Domain");
        Assert.Single(domain.Entities);
        Assert.Equal("Order", domain.Entities[0].Name);

        var app = result.Layers.First(l => l.Name == "Contoso.Application");
        Assert.Contains("Contoso.Domain", app.References);

        var infra = result.Layers.First(l => l.Name == "Contoso.Infrastructure");
        Assert.Contains("Contoso.Application", infra.References);

        var api = result.Layers.First(l => l.Name == "Contoso.Api");
        Assert.Contains("Contoso.Application", api.References);
        Assert.Contains("Contoso.Infrastructure", api.References);
        Assert.Single(api.Endpoints);
    }

    [Fact]
    public void ArchitectureResolver_ResolvesVerticalSlices()
    {
        var resolver = _serviceProvider.GetRequiredService<IArchitectureResolver>();

        var project = new ProjectDefinition
        {
            Name = "SlicesApp",
            Architecture = "vertical-slices",
            Entities = [new EntityDefinition { Name = "Item" }],
        };

        var result = resolver.Resolve(project);

        Assert.Equal("vertical-slices", result.Pattern);
        Assert.Single(result.Layers);
        Assert.Equal("SlicesApp", result.Layers[0].Name);
        Assert.Single(result.Layers[0].Entities);
    }

    [Fact]
    public void ArchitectureResolver_ResolvesCustomLayers()
    {
        var resolver = _serviceProvider.GetRequiredService<IArchitectureResolver>();

        var project = new ProjectDefinition
        {
            Name = "CustomApp",
            Layers =
            [
                new LayerDefinition { Name = "Core", Type = "dotnet-classlib" },
                new LayerDefinition { Name = "Web", Type = "dotnet-webapi", References = ["Core"] },
            ],
        };

        var result = resolver.Resolve(project);

        Assert.Equal("custom", result.Pattern);
        Assert.Equal(2, result.Layers.Count);
        Assert.Equal("Core", result.Layers[0].Name);
        Assert.Equal("Web", result.Layers[1].Name);
        Assert.Contains("Core", result.Layers[1].References);
    }

    #endregion

    #region DD-23: Full End-to-End Scaffold

    [Fact]
    public async Task ScaffoldEngine_ScaffoldsDotnetWebApiProject()
    {
        var engine = _serviceProvider.GetRequiredService<IScaffoldEngine>();

        var yaml = """
            name: SimpleApi
            version: 1.0.0
            projects:
              - name: SimpleApi
                type: dotnet-webapi
                path: src/SimpleApi
                framework: net9.0
                entities:
                  - name: Product
                    properties:
                      - name: id
                        type: uuid
                        required: true
                      - name: name
                        type: string
                        required: true
                      - name: price
                        type: float
            """;

        var result = await engine.ScaffoldAsync(yaml, _workspaceRoot);

        Assert.True(result.Success);
        Assert.NotEmpty(result.PlannedFiles);

        var projectPath = Path.Combine(_workspaceRoot, "SimpleApi", "src", "SimpleApi");
        Assert.True(File.Exists(Path.Combine(projectPath, "SimpleApi.csproj")));
        Assert.True(File.Exists(Path.Combine(projectPath, "Program.cs")));
        Assert.True(File.Exists(Path.Combine(projectPath, "appsettings.json")));

        var entityPath = Path.Combine(projectPath, "Models", "Product.cs");
        Assert.True(File.Exists(entityPath));

        var entityContent = await File.ReadAllTextAsync(entityPath);
        Assert.Contains("class Product", entityContent);
        Assert.Contains("Guid", entityContent);
        Assert.Contains("Name", entityContent);
        Assert.Contains("double", entityContent);
    }

    [Fact]
    public async Task ScaffoldEngine_ScaffoldsReactApp()
    {
        var engine = _serviceProvider.GetRequiredService<IScaffoldEngine>();

        var yaml = """
            name: ReactFrontend
            version: 1.0.0
            projects:
              - name: my-react-app
                type: react-app
                path: src/web
            """;

        var result = await engine.ScaffoldAsync(yaml, _workspaceRoot);

        Assert.True(result.Success);

        var projectPath = Path.Combine(_workspaceRoot, "ReactFrontend", "src", "web");
        Assert.True(File.Exists(Path.Combine(projectPath, "package.json")));
        Assert.True(File.Exists(Path.Combine(projectPath, "tsconfig.json")));
        Assert.True(File.Exists(Path.Combine(projectPath, "vite.config.ts")));
        Assert.True(File.Exists(Path.Combine(projectPath, "index.html")));
        Assert.True(File.Exists(Path.Combine(projectPath, "src", "App.tsx")));
        Assert.True(File.Exists(Path.Combine(projectPath, "src", "main.tsx")));
    }

    [Fact]
    public async Task ScaffoldEngine_ScaffoldsPythonFlaskApp()
    {
        var engine = _serviceProvider.GetRequiredService<IScaffoldEngine>();

        var yaml = """
            name: FlaskService
            version: 1.0.0
            projects:
              - name: my-flask-app
                type: flask-app
                path: src/api
                entities:
                  - name: User
                    properties:
                      - name: username
                        type: string
                        required: true
                      - name: email
                        type: string
                        required: true
            """;

        var result = await engine.ScaffoldAsync(yaml, _workspaceRoot);

        Assert.True(result.Success);

        var projectPath = Path.Combine(_workspaceRoot, "FlaskService", "src", "api");
        Assert.True(File.Exists(Path.Combine(projectPath, "requirements.txt")));
        Assert.True(File.Exists(Path.Combine(projectPath, "config.py")));
        Assert.True(File.Exists(Path.Combine(projectPath, "app", "__init__.py")));

        var entityPath = Path.Combine(projectPath, "Models", "User.py");
        Assert.True(File.Exists(entityPath));

        var entityContent = await File.ReadAllTextAsync(entityPath);
        Assert.Contains("class User", entityContent);
        Assert.Contains("str", entityContent);
    }

    [Fact]
    public async Task ScaffoldEngine_ScaffoldsMultiProjectWithSolution()
    {
        var engine = _serviceProvider.GetRequiredService<IScaffoldEngine>();

        var yaml = """
            name: FullStack
            version: 1.0.0
            globalVariables:
              company: Contoso
            projects:
              - name: FullStack.Api
                type: dotnet-webapi
                path: src/FullStack.Api
                references:
                  - FullStack.Domain
              - name: FullStack.Domain
                type: dotnet-classlib
                path: src/FullStack.Domain
                entities:
                  - name: Customer
                    properties:
                      - name: id
                        type: uuid
                        required: true
                      - name: name
                        type: string
              - name: FullStack.Web
                type: react-app
                path: src/web
            solutions:
              - name: FullStack
                projects:
                  - FullStack.Api
                  - FullStack.Domain
            """;

        var result = await engine.ScaffoldAsync(yaml, _workspaceRoot);

        Assert.True(result.Success);

        var rootPath = Path.Combine(_workspaceRoot, "FullStack");

        // .NET projects created
        Assert.True(File.Exists(Path.Combine(rootPath, "src", "FullStack.Api", "FullStack.Api.csproj")));
        Assert.True(File.Exists(Path.Combine(rootPath, "src", "FullStack.Domain", "FullStack.Domain.csproj")));

        // React project created
        Assert.True(File.Exists(Path.Combine(rootPath, "src", "web", "package.json")));

        // Entity generated in Domain project
        Assert.True(File.Exists(Path.Combine(rootPath, "src", "FullStack.Domain", "Models", "Customer.cs")));
    }

    [Fact]
    public async Task ScaffoldEngine_ScaffoldsPlaywrightTestProject()
    {
        var engine = _serviceProvider.GetRequiredService<IScaffoldEngine>();

        var yaml = """
            name: E2ETests
            version: 1.0.0
            projects:
              - name: app-e2e
                type: playwright-tests
                path: tests/e2e
                pageObjects:
                  - name: Dashboard
                    url: /dashboard
                    locators:
                      - name: welcomeText
                        strategy: GetByTestId
                        value: welcome-message
                specs:
                  - name: DashboardTests
                    page: Dashboard
                    tests:
                      - should display welcome message
                fixtures:
                  - name: TestUser
                    properties:
                      name: string
                      role: string
            """;

        var result = await engine.ScaffoldAsync(yaml, _workspaceRoot);

        Assert.True(result.Success);

        var projectPath = Path.Combine(_workspaceRoot, "E2ETests", "tests", "e2e");
        Assert.True(File.Exists(Path.Combine(projectPath, "playwright.config.ts")));
        Assert.True(File.Exists(Path.Combine(projectPath, "package.json")));
        Assert.True(File.Exists(Path.Combine(projectPath, "pages", "Dashboard.page.ts")));
        Assert.True(File.Exists(Path.Combine(projectPath, "specs", "DashboardTests.spec.ts")));
        Assert.True(File.Exists(Path.Combine(projectPath, "fixtures", "TestUser.fixture.ts")));

        var pageContent = await File.ReadAllTextAsync(Path.Combine(projectPath, "pages", "Dashboard.page.ts"));
        Assert.Contains("DashboardPage", pageContent);
        Assert.Contains("getByTestId", pageContent);
    }

    [Fact]
    public async Task ScaffoldEngine_DryRunDoesNotWriteFiles()
    {
        var engine = _serviceProvider.GetRequiredService<IScaffoldEngine>();

        var yaml = """
            name: DryRunTest
            version: 1.0.0
            projects:
              - name: DryApi
                type: dotnet-webapi
                path: src/DryApi
            """;

        var result = await engine.ScaffoldAsync(yaml, _workspaceRoot, dryRun: true);

        Assert.True(result.Success);
        Assert.NotEmpty(result.PlannedFiles);

        // Dry run should not create any files
        Assert.False(Directory.Exists(Path.Combine(_workspaceRoot, "DryRunTest")));
    }

    [Fact]
    public async Task ScaffoldEngine_ScaffoldsExplicitDirectoriesAndFiles()
    {
        var engine = _serviceProvider.GetRequiredService<IScaffoldEngine>();

        var yaml = """
            name: CustomFiles
            version: 1.0.0
            projects:
              - name: MyCustom
                type: custom
                path: src/custom
                directories:
                  - path: config
                    files:
                      - name: settings.json
                        content: '{"key": "value"}'
                  - path: scripts
                files:
                  - name: README.md
                    content: "# My Project"
            """;

        var result = await engine.ScaffoldAsync(yaml, _workspaceRoot);

        Assert.True(result.Success);

        var projectPath = Path.Combine(_workspaceRoot, "CustomFiles", "src", "custom");
        Assert.True(File.Exists(Path.Combine(projectPath, "config", "settings.json")));
        Assert.True(Directory.Exists(Path.Combine(projectPath, "scripts")));
        Assert.True(File.Exists(Path.Combine(projectPath, "README.md")));

        var settingsContent = await File.ReadAllTextAsync(Path.Combine(projectPath, "config", "settings.json"));
        Assert.Contains("key", settingsContent);
    }

    [Fact]
    public async Task ScaffoldEngine_ScaffoldsCleanArchitectureFromYaml()
    {
        var engine = _serviceProvider.GetRequiredService<IScaffoldEngine>();

        var yaml = """
            name: CleanApp
            version: 1.0.0
            projects:
              - name: CleanApp
                type: dotnet-webapi
                path: src/CleanApp
                architecture: clean-architecture
                framework: net9.0
                entities:
                  - name: Todo
                    properties:
                      - name: id
                        type: uuid
                        required: true
                      - name: title
                        type: string
                        required: true
                      - name: completed
                        type: bool
            """;

        var result = await engine.ScaffoldAsync(yaml, _workspaceRoot);

        Assert.True(result.Success);

        var rootPath = Path.Combine(_workspaceRoot, "CleanApp");

        // All 4 layers should be created
        Assert.True(File.Exists(Path.Combine(rootPath, "src", "CleanApp.Domain", "CleanApp.Domain.csproj")));
        Assert.True(File.Exists(Path.Combine(rootPath, "src", "CleanApp.Application", "CleanApp.Application.csproj")));
        Assert.True(File.Exists(Path.Combine(rootPath, "src", "CleanApp.Infrastructure", "CleanApp.Infrastructure.csproj")));
        Assert.True(File.Exists(Path.Combine(rootPath, "src", "CleanApp.Api", "CleanApp.Api.csproj")));

        // Entity should be in Domain layer
        var entityPath = Path.Combine(rootPath, "src", "CleanApp.Domain", "Models", "Todo.cs");
        Assert.True(File.Exists(entityPath));

        var content = await File.ReadAllTextAsync(entityPath);
        Assert.Contains("class Todo", content);
        Assert.Contains("Guid", content);
    }

    [Fact]
    public async Task ScaffoldEngine_TypeScriptEntityGeneration()
    {
        var engine = _serviceProvider.GetRequiredService<IScaffoldEngine>();

        var yaml = """
            name: TsEntities
            version: 1.0.0
            projects:
              - name: my-app
                type: react-app
                path: src/app
                entities:
                  - name: Product
                    properties:
                      - name: id
                        type: uuid
                        required: true
                      - name: name
                        type: string
                        required: true
                      - name: tags
                        type: list<string>
                      - name: metadata
                        type: map<string, string>
            """;

        var result = await engine.ScaffoldAsync(yaml, _workspaceRoot);

        Assert.True(result.Success);

        var entityPath = Path.Combine(_workspaceRoot, "TsEntities", "src", "app", "Models", "Product.ts");
        Assert.True(File.Exists(entityPath));

        var content = await File.ReadAllTextAsync(entityPath);
        Assert.Contains("interface Product", content);
        Assert.Contains("string", content);
        Assert.Contains("string[]", content);
        Assert.Contains("Record<string, string>", content);
    }

    #endregion
}
