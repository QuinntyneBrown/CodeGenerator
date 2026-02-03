// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Services;
using CodeGenerator.Core.Services;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Artifacts.Projects.Strategies;

public class AngularStandaloneProjectArtifactGenerationStrategy : IArtifactGenerationStrategy<ProjectModel>
{
    private readonly ILogger<AngularStandaloneProjectArtifactGenerationStrategy> logger;
    private readonly IFileSystem fileSystem;
    private readonly ITemplateLocator templateLocator;
    private readonly ITemplateProcessor templateProcessor;
    private readonly ICommandService commandService;

    public AngularStandaloneProjectArtifactGenerationStrategy(
        ILogger<AngularStandaloneProjectArtifactGenerationStrategy> logger,
        IFileSystem fileSystem,
        ITemplateLocator templateLocator,
        ITemplateProcessor templateProcessor,
        ICommandService commandService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.templateLocator = templateLocator ?? throw new ArgumentNullException(nameof(templateLocator));
        this.templateProcessor = templateProcessor ?? throw new ArgumentNullException(nameof(templateProcessor));
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

        // Convert project name to kebab-case
        var kebabCaseProjectName = model.Name.Kebaberize();

        // Create Angular workspace without initial application in the project's own directory
        logger.LogInformation("Creating Angular workspace: ng new {projectName} --no-create-application --directory ./ --defaults", model.Name);
        commandService.Start($"ng new {model.Name} --no-create-application --directory ./ --defaults", model.Directory);

        // Create the single application with kebab-case name (run from workspace root where angular.json is located)
        logger.LogInformation("Creating Angular application: ng g application {kebabCaseProjectName} --defaults", kebabCaseProjectName);
        commandService.Start($"ng g application {kebabCaseProjectName} --defaults", model.Directory);

        // Generate the .esproj file
        var template = string.Join(Environment.NewLine, templateLocator.Get("EsProj"));

        var result = templateProcessor.Process(template, new TokensBuilder()
            .With("projectName", model.Name)
            .Build());

        fileSystem.File.WriteAllText(model.Path, result);
    }
}