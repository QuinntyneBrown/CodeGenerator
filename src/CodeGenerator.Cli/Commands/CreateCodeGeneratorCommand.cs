// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.CommandLine;
using System.IO.Abstractions;
using System.Reflection;
using System.Text;
using CodeGenerator.Cli.Rendering;
using CodeGenerator.Cli.Services;
using CodeGenerator.Cli.Validation;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Configuration;
using CodeGenerator.Core.Diagnostics;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Services;
using CodeGenerator.DotNet.Artifacts.Files;
using CodeGenerator.DotNet.Artifacts.Projects;
using CodeGenerator.DotNet.Artifacts.Projects.Enums;
using CodeGenerator.DotNet.Artifacts.Solutions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace CodeGenerator.Cli.Commands;

public class CreateCodeGeneratorCommand : RootCommand
{
    private readonly IServiceProvider _serviceProvider;

    public CreateCodeGeneratorCommand(IServiceProvider serviceProvider)
        : base("Creates a new code generator CLI project")
    {
        _serviceProvider = serviceProvider;

        var config = serviceProvider.GetService<ICodeGeneratorConfiguration>();

        var nameOption = new Option<string>(
            aliases: ["-n", "--name"],
            description: "The name of the solution to create")
        {
            IsRequired = false
        };

        var outputOption = new Option<string>(
            aliases: ["-o", "--output"],
            description: "The output directory (defaults to current directory)",
            getDefaultValue: () => config?.GetValue("output") ?? Directory.GetCurrentDirectory());

        var frameworkOption = new Option<string>(
            aliases: ["-f", "--framework"],
            description: "The target framework (e.g. net8.0, net9.0)",
            getDefaultValue: () => config?.GetValue("framework") ?? "net9.0");

        var slnxOption = new Option<bool>(
            aliases: ["--slnx"],
            description: "Use .slnx (XML-based) solution format instead of .sln",
            getDefaultValue: () => config?.GetValue<bool>("slnx", false) ?? false);

        var localSourceRootOption = new Option<string?>(
            aliases: ["--local-source-root"],
            description: "Optional path to the local CodeGenerator src directory for project references");

        var diagnosticsOption = new Option<bool>(
            aliases: ["--diagnostics"],
            description: "Show environment info and per-step timing",
            getDefaultValue: () => false);

        var failFastOption = new Option<bool>(
            aliases: ["--fail-fast"],
            description: "Abort on first strategy failure",
            getDefaultValue: () => false);

        AddOption(nameOption);
        AddOption(outputOption);
        AddOption(frameworkOption);
        AddOption(slnxOption);
        AddOption(localSourceRootOption);
        AddOption(diagnosticsOption);
        AddOption(failFastOption);

        AddCommand(new InstallCommand(serviceProvider));
        AddCommand(new ScaffoldCommand(serviceProvider));

        this.SetHandler(HandleAsync, nameOption, outputOption, frameworkOption, slnxOption, localSourceRootOption, diagnosticsOption, failFastOption);
    }

    private async Task HandleAsync(string name, string outputDirectory, string framework, bool slnx, string? localSourceRoot, bool diagnostics, bool failFast)
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<CreateCodeGeneratorCommand>>();
        var fileSystem = _serviceProvider.GetRequiredService<IFileSystem>();
        var artifactGenerator = _serviceProvider.GetRequiredService<IArtifactGenerator>();
        var commandService = _serviceProvider.GetRequiredService<ICommandService>();

        if (failFast && artifactGenerator is ArtifactGenerator concreteGenerator)
            concreteGenerator.FailFast = true;
        var ct = _serviceProvider.GetService<CancellationTokenSource>()?.Token ?? CancellationToken.None;

        // Design 54: Initialize correlation ID for observability
        var correlationId = Guid.NewGuid().ToString();
        DiagnosticContext.Current.CorrelationId = correlationId;

        IGenerationTimer timer = diagnostics ? new GenerationTimer() : new NullGenerationTimer();

        // Design 60: Prompt for missing options in interactive mode
        var promptService = _serviceProvider.GetService<IInteractivePromptService>();
        if (string.IsNullOrWhiteSpace(name) && promptService != null)
        {
            var partial = new GenerationOptions
            {
                Name = name ?? string.Empty,
                OutputDirectory = outputDirectory,
                Framework = framework,
                Slnx = slnx,
                LocalSourceRoot = localSourceRoot,
            };

            var prompted = promptService.PromptForMissingOptions(partial);
            name = prompted.Name;
            outputDirectory = prompted.OutputDirectory;
            framework = prompted.Framework;
            slnx = prompted.Slnx;
            localSourceRoot = prompted.LocalSourceRoot;
        }

        // Step 1: Validate before any generation
        using (timer.TimeStep("Validate options"))
        {
            var validator = new GenerationOptionsValidator(fileSystem);

            var options = new GenerationOptions
            {
                Name = name,
                OutputDirectory = outputDirectory,
                Framework = framework,
                Slnx = slnx,
                LocalSourceRoot = localSourceRoot,
            };

            var validationResult = validator.Validate(options);

            if (!validationResult.IsValid)
            {
                throw new CliValidationException(validationResult);
            }

            foreach (var warning in validationResult.Warnings)
            {
                logger.LogWarning("{Property}: {Message}", warning.PropertyName, warning.ErrorMessage);
            }
        }

        // Design 59: Wrap generation with rollback protection
        using var scope = _serviceProvider.CreateScope();
        var rollbackService = scope.ServiceProvider.GetRequiredService<IGenerationRollbackService>();

        // Step 2: Proceed with generation
        logger.LogInformation("Creating code generator solution: {Name}", name);
        logger.LogInformation("Output directory: {OutputDirectory}", outputDirectory);

        try
        {
        // Create Solution Model
        var solution = new SolutionModel(name, outputDirectory);

        if (slnx)
        {
            solution.SolutionExtension = ".slnx";
        }

        // Create Project Model
        var project = new ProjectModel(DotNetProjectType.Console, $"{name}.Cli", solution.SrcDirectory)
        {
            Files =
            {
                new ContentFileModel(
                    GenerateProgramContent(name),
                    "Program",
                    Path.Combine(solution.SrcDirectory, $"{name}.Cli"),
                    ".cs"),
                new ContentFileModel(
                    GenerateRootCommandContent(name),
                    "AppRootCommand",
                    Path.Combine(solution.SrcDirectory, $"{name}.Cli", "Commands"),
                    ".cs"),
                new ContentFileModel(
                    GenerateHelloWorldCommandContent(name),
                    "HelloWorldCommand",
                    Path.Combine(solution.SrcDirectory, $"{name}.Cli", "Commands"),
                    ".cs"),
                new ContentFileModel(
                    GenerateEnterpriseSolutionCommandContent(name),
                    "EnterpriseSolutionCommand",
                    Path.Combine(solution.SrcDirectory, $"{name}.Cli", "Commands"),
                    ".cs")
            }
        };

        // Add project to solution
        solution.Projects.Add(project);

        using (timer.TimeStep("Create directories"))
        {
            Directory.CreateDirectory(solution.SolutionDirectory);
            rollbackService.TrackDirectoryCreated(solution.SolutionDirectory);
            Directory.CreateDirectory(solution.SrcDirectory);
            rollbackService.TrackDirectoryCreated(solution.SrcDirectory);
            Directory.CreateDirectory(project.Directory);
            rollbackService.TrackDirectoryCreated(project.Directory);
            Directory.CreateDirectory(Path.Combine(project.Directory, "Commands"));
            rollbackService.TrackDirectoryCreated(Path.Combine(project.Directory, "Commands"));
            Directory.CreateDirectory(Path.Combine(solution.SolutionDirectory, "eng", "scripts"));
            rollbackService.TrackDirectoryCreated(Path.Combine(solution.SolutionDirectory, "eng", "scripts"));
        }

        // Create solution file
        using (timer.TimeStep("Create solution file"))
        {
            if (slnx)
            {
                commandService.Start($"dotnet new slnx -n {name}", solution.SolutionDirectory, ct: ct);
            }
            else
            {
                commandService.Start($"dotnet new sln -n {name}", solution.SolutionDirectory, ct: ct);
            }
        }

        // Generate custom .csproj (not using dotnet new since we need tool-specific settings)
        using (timer.TimeStep("Generate .csproj"))
        {
            await artifactGenerator.GenerateAsync(new ContentFileModel(
                GenerateCliProjectContent(name, framework, localSourceRoot, project.Directory),
                $"{name}.Cli",
                project.Directory,
                ".csproj"), ct);
        }

        // Generate project files
        using (timer.TimeStep("Generate project files"))
        {
            foreach (var file in project.Files)
            {
                await artifactGenerator.GenerateAsync(file, ct);
            }
        }

        // Add project to solution
        using (timer.TimeStep("Add project to solution"))
        {
            commandService.Start($"dotnet sln add {project.Path}", solution.SolutionDirectory, ct: ct);
        }

        // Generate install-cli.bat script
        using (timer.TimeStep("Generate install script"))
        {
            await artifactGenerator.GenerateAsync(new ContentFileModel(
                GenerateInstallCliBatContent(name),
                "install-cli",
                Path.Combine(solution.SolutionDirectory, "eng", "scripts"),
                ".bat"), ct);
        }

        rollbackService.Commit();

        logger.LogInformation("Solution created successfully at: {Path}", solution.SolutionDirectory);
        logger.LogInformation("");
        logger.LogInformation("Next steps:");
        logger.LogInformation("  cd {Name}", name);
        logger.LogInformation("  dotnet build");
        logger.LogInformation("  dotnet run --project src/{Name}.Cli -- hello -o ./output", name);
        logger.LogInformation("  dotnet run --project src/{Name}.Cli -- enterprise-solution -n SampleEnterprise -o ./output", name);
        logger.LogInformation("");
        logger.LogInformation("To install as a global tool:");
        logger.LogInformation("  eng\\scripts\\install-cli.bat");
        }
        catch (Exception ex) when (ex is not CliException and not OperationCanceledException)
        {
            logger.LogError(ex, "Generation failed, rolling back...");
            rollbackService.Rollback();
            throw;
        }
        catch (CliException)
        {
            rollbackService.Rollback();
            throw;
        }

        if (diagnostics)
        {
            var collector = _serviceProvider.GetRequiredService<DiagnosticsCollector>();
            var cliVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0";
            var report = new DiagnosticsReport
            {
                Environment = collector.CollectEnvironment(cliVersion),
                Steps = timer.GetEntries().ToList(),
                TotalDuration = timer.TotalElapsed,
            };
            var renderer = new DiagnosticsRenderer(AnsiConsole.Console);
            renderer.Render(report);
        }
    }

    private static string GenerateCliProjectContent(string name, string framework, string? localSourceRoot, string projectDirectory)
    {
        var codeGeneratorReferences = string.IsNullOrWhiteSpace(localSourceRoot)
            ? GetPackageReferences()
            : GetProjectReferences(localSourceRoot!, projectDirectory);

        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>{framework}</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>

    <!-- Tool Configuration -->
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>{name.ToLower()}-cli</ToolCommandName>
    <PackageOutputPath>./nupkg</PackageOutputPath>

    <!-- Package Metadata -->
    <PackageId>{name}.Cli</PackageId>
    <Title>{name} CLI</Title>
    <Description>Code generator CLI built with CodeGenerator framework</Description>
    <Version>1.0.0</Version>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.Extensions.Configuration"" Version=""9.0.0"" />
    <PackageReference Include=""Microsoft.Extensions.Configuration.Json"" Version=""9.0.0"" />
    <PackageReference Include=""Microsoft.Extensions.Configuration.EnvironmentVariables"" Version=""9.0.0"" />
    <PackageReference Include=""Microsoft.Extensions.DependencyInjection"" Version=""9.0.0"" />
    <PackageReference Include=""Microsoft.Extensions.Logging"" Version=""9.0.0"" />
    <PackageReference Include=""Microsoft.Extensions.Logging.Console"" Version=""9.0.0"" />
    <PackageReference Include=""System.CommandLine"" Version=""2.0.0-beta4.22272.1"" />
  </ItemGroup>

{codeGeneratorReferences}
</Project>
";
    }

    private static string GenerateProgramContent(string name) => $@"using {name}.Cli.Commands;
using CodeGenerator.Angular;
using CodeGenerator.Core;
using CodeGenerator.Flask;
using CodeGenerator.React;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(configuration);

services.AddLogging(builder =>
{{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
}});

services.AddCoreServices(typeof(Program).Assembly);
services.AddDotNetServices();
services.AddAngularServices();
services.AddReactServices();
services.AddFlaskServices();
services.AddPythonServices();
services.AddPlaywrightServices();
services.AddDetoxServices();
services.AddReactNativeServices();

var serviceProvider = services.BuildServiceProvider();

var rootCommand = new AppRootCommand(serviceProvider);

return await rootCommand.InvokeAsync(args);
";

    private static string GenerateRootCommandContent(string name) => $@"using System.CommandLine;

namespace {name}.Cli.Commands;

public class AppRootCommand : RootCommand
{{
    public AppRootCommand(IServiceProvider serviceProvider)
        : base(""{name} Code Generator CLI"")
    {{
        AddCommand(new EnterpriseSolutionCommand(serviceProvider));
        AddCommand(new HelloWorldCommand(serviceProvider));
    }}
}}
";

    private static string GenerateEnterpriseSolutionCommandContent(string name) => $@"using System.CommandLine;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Artifacts.FullStack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace {name}.Cli.Commands;

public class EnterpriseSolutionCommand : Command
{{
    private readonly IServiceProvider _serviceProvider;

    public EnterpriseSolutionCommand(IServiceProvider serviceProvider)
        : base(""enterprise-solution"", ""Generates an Angular + .NET enterprise solution"")
    {{
        _serviceProvider = serviceProvider;

        var nameOption = new Option<string>(
            aliases: [""-n"", ""--name""],
            description: ""The solution name to generate"")
        {{
            IsRequired = true
        }};

        var outputOption = new Option<string>(
            aliases: [""-o"", ""--output""],
            description: ""The output directory"",
            getDefaultValue: () => Directory.GetCurrentDirectory());

        AddOption(nameOption);
        AddOption(outputOption);

        this.SetHandler(HandleAsync, nameOption, outputOption);
    }}

    private async Task HandleAsync(string name, string outputDirectory)
    {{
        var logger = _serviceProvider.GetRequiredService<ILogger<EnterpriseSolutionCommand>>();
        var artifactGenerator = _serviceProvider.GetRequiredService<IArtifactGenerator>();
        var fullStackFactory = _serviceProvider.GetRequiredService<IFullStackFactory>();

        logger.LogInformation(""Generating enterprise solution {{Name}}..."", name);

        var fullStack = await fullStackFactory.CreateAsync(new FullStackCreateOptions
        {{
            Name = name,
            Directory = outputDirectory,
        }});

        await artifactGenerator.GenerateAsync(fullStack.Solution);

        logger.LogInformation(""Generated: {{Path}}"", fullStack.Solution.SolutionDirectory);
    }}
}}
";

    private static string GenerateHelloWorldCommandContent(string name) => $@"using System.CommandLine;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Artifacts.Files;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace {name}.Cli.Commands;

public class HelloWorldCommand : Command
{{
    private readonly IServiceProvider _serviceProvider;

    public HelloWorldCommand(IServiceProvider serviceProvider)
        : base(""hello"", ""Generates a HelloWorld.txt file using CodeGenerator"")
    {{
        _serviceProvider = serviceProvider;

        var outputOption = new Option<string>(
            aliases: [""-o"", ""--output""],
            description: ""The output directory"",
            getDefaultValue: () => Directory.GetCurrentDirectory());

        var messageOption = new Option<string>(
            aliases: [""-m"", ""--message""],
            description: ""The message to include in the file"",
            getDefaultValue: () => ""Hello from {name}!"");

        AddOption(outputOption);
        AddOption(messageOption);

        this.SetHandler(HandleAsync, outputOption, messageOption);
    }}

    private async Task HandleAsync(string outputDirectory, string message)
    {{
        var logger = _serviceProvider.GetRequiredService<ILogger<HelloWorldCommand>>();
        var artifactGenerator = _serviceProvider.GetRequiredService<IArtifactGenerator>();

        logger.LogInformation(""Generating HelloWorld.txt..."");

        Directory.CreateDirectory(outputDirectory);

        var content = $@""// =============================================================================
// Generated by {name} Code Generator
// Generated at: {{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}} UTC
// =============================================================================

{{message}}

This file was generated using the CodeGenerator framework.
Visit https://github.com/QuinntyneBrown/CodeGenerator for more information.
"";

        var fileModel = new ContentFileModel(content, ""HelloWorld"", outputDirectory, "".txt"");

        await artifactGenerator.GenerateAsync(fileModel);

        logger.LogInformation(""Generated: {{Path}}"", Path.Combine(outputDirectory, ""HelloWorld.txt""));
    }}
}}
";

    private static string GenerateInstallCliBatContent(string name) => $@"@echo off
setlocal

echo ============================================
echo Installing {name} CLI Tool
echo ============================================
echo.

REM Navigate to solution root
cd /d ""%~dp0..\..\""

REM Uninstall existing version (ignore errors if not installed)
echo Uninstalling existing version (if any)...
dotnet tool uninstall -g {name}.Cli 2>nul

REM Build the project
echo.
echo Building project...
dotnet build src\{name}.Cli\{name}.Cli.csproj -c Release
if errorlevel 1 (
    echo Build failed!
    exit /b 1
)

REM Pack the project
echo.
echo Packing project...
dotnet pack src\{name}.Cli\{name}.Cli.csproj -c Release -o src\{name}.Cli\nupkg
if errorlevel 1 (
    echo Pack failed!
    exit /b 1
)

REM Install the tool globally from local package
echo.
echo Installing tool globally...
dotnet tool install -g {name}.Cli --add-source src\{name}.Cli\nupkg
if errorlevel 1 (
    echo Install failed!
    exit /b 1
)

echo.
echo ============================================
echo Installation complete!
echo.
echo You can now use the CLI by running:
echo   {name.ToLower()}-cli
echo ============================================

endlocal
";

    private static string GetPackageReferences() => $@"  <ItemGroup>
    <PackageReference Include=""QuinntyneBrown.CodeGenerator.Core"" Version=""{PackageVersions.Core}"" />
    <PackageReference Include=""QuinntyneBrown.CodeGenerator.DotNet"" Version=""{PackageVersions.DotNet}"" />
    <PackageReference Include=""QuinntyneBrown.CodeGenerator.Angular"" Version=""{PackageVersions.Angular}"" />
    <PackageReference Include=""QuinntyneBrown.CodeGenerator.React"" Version=""{PackageVersions.React}"" />
    <PackageReference Include=""QuinntyneBrown.CodeGenerator.Flask"" Version=""{PackageVersions.Flask}"" />
    <PackageReference Include=""QuinntyneBrown.CodeGenerator.Python"" Version=""{PackageVersions.Python}"" />
    <PackageReference Include=""QuinntyneBrown.CodeGenerator.Playwright"" Version=""{PackageVersions.Playwright}"" />
    <PackageReference Include=""QuinntyneBrown.CodeGenerator.Detox"" Version=""{PackageVersions.Detox}"" />
    <PackageReference Include=""QuinntyneBrown.CodeGenerator.ReactNative"" Version=""{PackageVersions.ReactNative}"" />
  </ItemGroup>";

    private static string GetProjectReferences(string localSourceRoot, string projectDirectory)
    {
        var references = new[]
        {
            "CodeGenerator.Core",
            "CodeGenerator.DotNet",
            "CodeGenerator.Angular",
            "CodeGenerator.React",
            "CodeGenerator.Flask",
            "CodeGenerator.Python",
            "CodeGenerator.Playwright",
            "CodeGenerator.Detox",
            "CodeGenerator.ReactNative",
        };

        var builder = new StringBuilder();
        builder.AppendLine("  <ItemGroup>");

        foreach (var reference in references)
        {
            var projectPath = Path.Combine(localSourceRoot, reference, $"{reference}.csproj");
            var relativeProjectPath = Path.GetRelativePath(projectDirectory, projectPath);
            builder.AppendLine($@"    <ProjectReference Include=""{relativeProjectPath}"" />");
        }

        builder.Append("  </ItemGroup>");

        return builder.ToString();
    }
}
