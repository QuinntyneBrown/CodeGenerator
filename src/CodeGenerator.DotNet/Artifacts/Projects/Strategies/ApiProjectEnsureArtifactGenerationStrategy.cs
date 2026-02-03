// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Artifacts.Files.Factories;
using CodeGenerator.DotNet.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Artifacts.Projects.Strategies;

using IFileFactory = CodeGenerator.DotNet.Artifacts.Files.Factories.IFileFactory;
public class ApiProjectEnsureArtifactGenerationStrategy : IArtifactGenerationStrategy<ProjectReferenceModel>
{
    private readonly ILogger<ApiProjectEnsureArtifactGenerationStrategy> logger;
    private readonly IFileFactory fileFactory;
    private readonly IFileSystem fileSystem;
    private readonly IFileProvider fileProvider;
    private readonly ICommandService commandService;
    private readonly IArtifactGenerator artifactGenerator;

    public ApiProjectEnsureArtifactGenerationStrategy(
        IFileFactory fileFactory,
        IFileSystem fileSystem,
        IFileProvider fileProvider,
        ICommandService commandService,
        ILogger<ApiProjectEnsureArtifactGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        this.fileFactory = fileFactory ?? throw new ArgumentNullException(nameof(fileFactory));
    }

    public bool CanHandle(object model)
        => true; // => model is ProjectReferenceModel && context.Command is ApiProjectEnsure;

    public async Task GenerateAsync(ProjectReferenceModel model)
    {
        logger.LogInformation("Generating artifact for {0}.", model);

        var projectDirectory = fileSystem.Path.GetDirectoryName(fileProvider.Get("*.csproj", model.ReferenceDirectory));

        EnsureDefaultFilesRemoved(projectDirectory);

        EnsurePackagesInstalled(projectDirectory);

        EnsureProjectsReferenced(projectDirectory);

        await EnsureApiDefaultFilesAdd(artifactGenerator, projectDirectory);
    }

    private void EnsureDefaultFilesRemoved(string projectDirectory)
    {
        fileSystem.File.Delete($"{projectDirectory}{fileSystem.Path.DirectorySeparatorChar}Controllers{fileSystem.Path.DirectorySeparatorChar}WeatherForecastController.cs");
        fileSystem.File.Delete($"{projectDirectory}{fileSystem.Path.DirectorySeparatorChar}WeatherForecast.cs");
    }

    private async Task EnsureApiDefaultFilesAdd(IArtifactGenerator artifactGenerator, string projectDirectory)
    {
        var projectName = fileSystem.Path.GetFileNameWithoutExtension(projectDirectory).Split('.').First();

        var dbContext = $"{projectName}DbContext";

        if (!fileSystem.File.Exists($"{projectDirectory}{fileSystem.Path.DirectorySeparatorChar}Properties{fileSystem.Path.DirectorySeparatorChar}launchSettings.json"))
        {
            await artifactGenerator.GenerateAsync(fileFactory.LaunchSettingsJson(projectDirectory, projectName, 5000));
        }

        if (!fileSystem.File.Exists($"{projectDirectory}{fileSystem.Path.DirectorySeparatorChar}ConfigureServices.cs"))
        {
            await artifactGenerator.GenerateAsync(fileFactory.CreateTemplate("Api.ConfigureServices", "ConfigureServices", projectDirectory, tokens: new TokensBuilder()
                .With("DbContext", dbContext)
                .With("serviceName", projectName)
                .Build()));

            await artifactGenerator.GenerateAsync(fileFactory.CreateTemplate("Api.Program", "Program", projectDirectory, tokens: new TokensBuilder()
                .With("DbContext", dbContext)
                .With("serviceName", projectName)
                .Build()));
        }
    }

    private void EnsurePackagesInstalled(string projectDirectory)
    {
        var projectPath = fileProvider.Get("*.csproj", projectDirectory);

        foreach (var package in new string[]
        {
            "Serilog",
            "SerilogTimings",
            "Serilog.AspNetCore",
            "Swashbuckle.AspNetCore",
            "Swashbuckle.AspNetCore.Annotations",
            "Swashbuckle.AspNetCore.Swagger",
            "Swashbuckle.AspNetCore.Newtonsoft",
        })
        {
            var projectFileContents = fileSystem.File.ReadAllText(projectPath);

            if (!projectFileContents.Contains($"PackageReference Include=\"{package}\""))
            {
                commandService.Start($"dotnet add package {package}", projectDirectory);
            }
        }
    }

    private void EnsureProjectsReferenced(string projectDirectory)
    {
        var projectName = fileSystem.Path.GetFileNameWithoutExtension(projectDirectory).Split('.').First();

        var solutionDirectory = projectDirectory;

        while (!fileSystem.Directory.EnumerateFiles(solutionDirectory, "*.sln").Any(x => x.EndsWith("sln")) &&
               !fileSystem.Directory.EnumerateFiles(solutionDirectory, "*.slnx").Any(x => x.EndsWith("slnx")))
        {
            solutionDirectory = solutionDirectory.Substring(0, solutionDirectory.LastIndexOf(fileSystem.Path.DirectorySeparatorChar));
        }

        foreach (var infrastructureProjectPath in fileSystem.Directory.GetFiles(solutionDirectory, $"{projectName}.Infrastructure.csproj", SearchOption.AllDirectories))
        {
            commandService.Start($"dotnet add {projectDirectory} reference {infrastructureProjectPath}", projectDirectory);
        }
    }
}
