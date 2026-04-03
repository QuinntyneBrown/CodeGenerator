// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Services;
using CodeGenerator.Core.Services;
using Humanizer;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace CodeGenerator.DotNet.Artifacts.Projects.Strategies;

public class AngularStandaloneProjectArtifactGenerationStrategy : IArtifactGenerationStrategy<ProjectModel>
{
    private readonly ILogger<AngularStandaloneProjectArtifactGenerationStrategy> logger;
    private readonly IFileSystem fileSystem;
    private readonly ICommandService commandService;

    public AngularStandaloneProjectArtifactGenerationStrategy(
        ILogger<AngularStandaloneProjectArtifactGenerationStrategy> logger,
        IFileSystem fileSystem,
        ICommandService commandService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
    }

    public async Task<bool> GenerateAsync(IArtifactGenerator generator, object target)
    {
        if (target is ProjectModel model && model.Extension == ".esproj")
        {
            await GenerateAsync(model);

            return true;
        }
        else
        {
            return false;
        }
    }

    public virtual int GetPriority() => 1;

    public async Task GenerateAsync(ProjectModel model)
    {
        logger.LogInformation("Generating Angular workspace for project {name}", model.Name);

        // Create the project directory if it doesn't exist (each Angular project gets its own workspace)
        if (!fileSystem.Directory.Exists(model.Directory))
        {
            fileSystem.Directory.CreateDirectory(model.Directory);
        }

        var angularProjectName = SanitizeAngularProjectName(model.Name);

        // Create Angular workspace without initial application in the project's own directory
        logger.LogInformation("Creating Angular workspace: ng new {projectName} --no-create-application --directory ./ --defaults --skip-git --skip-install", angularProjectName);
        commandService.Start($"ng new {angularProjectName} --no-create-application --directory ./ --defaults --skip-git --skip-install", model.Directory);

        // Create the single application with kebab-case name (run from workspace root where angular.json is located)
        logger.LogInformation("Creating Angular application: ng g application {projectName} --defaults --skip-tests --skip-install", angularProjectName);
        commandService.Start($"ng g application {angularProjectName} --defaults --skip-tests --skip-install", model.Directory);

        // Generate the .esproj file
        fileSystem.File.WriteAllText(model.Path, BuildEsProjContent(angularProjectName));
    }

    private static string SanitizeAngularProjectName(string name)
    {
        var sanitized = Regex.Replace(name.Replace('.', '-').Kebaberize(), @"[^a-zA-Z0-9\-]", "-")
            .Trim('-')
            .ToLowerInvariant();

        return string.IsNullOrWhiteSpace(sanitized) ? "app" : sanitized;
    }

    private static string BuildEsProjContent(string angularProjectName) =>
        $@"<Project Sdk=""Microsoft.VisualStudio.JavaScript.Sdk"">
  <PropertyGroup>
    <StartupCommand>npm start</StartupCommand>
    <JavaScriptTestRoot>src\</JavaScriptTestRoot>
    <BuildCommand>npm run build</BuildCommand>
    <BuildOutputFolder>dist\{angularProjectName}</BuildOutputFolder>
    <ShouldRunNpmInstall>false</ShouldRunNpmInstall>
  </PropertyGroup>
</Project>
";
}
