// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.CommandLine;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Services;
using CodeGenerator.DotNet.Artifacts.Files;
using CodeGenerator.DotNet.Artifacts.Projects;
using CodeGenerator.DotNet.Artifacts.Projects.Enums;
using CodeGenerator.DotNet.Artifacts.Solutions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Cli.Commands;

public class CreateCodeGeneratorCommand : RootCommand
{
    private readonly IServiceProvider _serviceProvider;

    public CreateCodeGeneratorCommand(IServiceProvider serviceProvider)
        : base("Creates a new code generator CLI project")
    {
        _serviceProvider = serviceProvider;

        var nameOption = new Option<string>(
            aliases: ["-n", "--name"],
            description: "The name of the solution to create")
        {
            IsRequired = true
        };

        var outputOption = new Option<string>(
            aliases: ["-o", "--output"],
            description: "The output directory (defaults to current directory)",
            getDefaultValue: () => Directory.GetCurrentDirectory());

        AddOption(nameOption);
        AddOption(outputOption);

        this.SetHandler(HandleAsync, nameOption, outputOption);
    }

    private async Task HandleAsync(string name, string outputDirectory)
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<CreateCodeGeneratorCommand>>();
        var artifactGenerator = _serviceProvider.GetRequiredService<IArtifactGenerator>();
        var commandService = _serviceProvider.GetRequiredService<ICommandService>();

        logger.LogInformation("Creating code generator solution: {Name}", name);
        logger.LogInformation("Output directory: {OutputDirectory}", outputDirectory);

        // Create Solution Model
        var solution = new SolutionModel(name, outputDirectory);

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
                    ".cs")
            }
        };

        // Add project to solution
        solution.Projects.Add(project);

        // Generate solution structure
        Directory.CreateDirectory(solution.SolutionDirectory);
        Directory.CreateDirectory(solution.SrcDirectory);
        Directory.CreateDirectory(project.Directory);
        Directory.CreateDirectory(Path.Combine(project.Directory, "Commands"));
        Directory.CreateDirectory(Path.Combine(solution.SolutionDirectory, "eng", "scripts"));

        // Create solution file
        commandService.Start($"dotnet new sln -n {name}", solution.SolutionDirectory);

        // Generate custom .csproj (not using dotnet new since we need tool-specific settings)
        await artifactGenerator.GenerateAsync(new ContentFileModel(
            GenerateCliProjectContent(name),
            $"{name}.Cli",
            project.Directory,
            ".csproj"));

        // Generate project files
        foreach (var file in project.Files)
        {
            await artifactGenerator.GenerateAsync(file);
        }

        // Add project to solution
        commandService.Start($"dotnet sln add {project.Path}", solution.SolutionDirectory);

        // Generate install-cli.bat script
        await artifactGenerator.GenerateAsync(new ContentFileModel(
            GenerateInstallCliBatContent(name),
            "install-cli",
            Path.Combine(solution.SolutionDirectory, "eng", "scripts"),
            ".bat"));

        logger.LogInformation("Solution created successfully at: {Path}", solution.SolutionDirectory);
        logger.LogInformation("");
        logger.LogInformation("Next steps:");
        logger.LogInformation("  cd {Name}", name);
        logger.LogInformation("  dotnet build");
        logger.LogInformation("  dotnet run --project src/{Name}.Cli -- hello -o ./output", name);
        logger.LogInformation("");
        logger.LogInformation("To install as a global tool:");
        logger.LogInformation("  eng\\scripts\\install-cli.bat");
    }

    private static string GenerateCliProjectContent(string name) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
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
    <PackageReference Include=""QuinntyneBrown.CodeGenerator.Core"" Version=""1.0.0"" />
    <PackageReference Include=""QuinntyneBrown.CodeGenerator.DotNet"" Version=""1.0.0"" />
    <PackageReference Include=""QuinntyneBrown.CodeGenerator.Angular"" Version=""1.0.0"" />
  </ItemGroup>
</Project>
";

    private static string GenerateProgramContent(string name) => $@"using {name}.Cli.Commands;
using CodeGenerator.Core;
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
        AddCommand(new HelloWorldCommand(serviceProvider));
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
}
