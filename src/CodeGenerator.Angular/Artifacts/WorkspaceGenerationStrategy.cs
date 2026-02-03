// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Angular.Artifacts;

public class WorkspaceGenerationStrategy : IArtifactGenerationStrategy<WorkspaceModel>
{
    private readonly ILogger<WorkspaceGenerationStrategy> logger;
    private readonly ICommandService commandService;

    public WorkspaceGenerationStrategy(ILogger<WorkspaceGenerationStrategy> logger, ICommandService commandService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
    }

    public async Task GenerateAsync(WorkspaceModel model)
    {
        logger.LogInformation("Generating Angular Workspace. {name}", model.Name);

        commandService.Start($"npm uninstall @angular/cli -g", model.RootDirectory);

        commandService.Start($"npm install --registry=https://registry.npmjs.org/ @angular/cli@{model.Version} -g", model.RootDirectory);

        commandService.Start($"ng new {model.Name} --no-create-application --defaults=true", model.RootDirectory);

        commandService.Start($"npm install --registry=https://registry.npmjs.org/ @ngrx/component-store --force", Path.Combine(model.RootDirectory, model.Name));

        commandService.Start($"npm install --registry=https://registry.npmjs.org/ @ngrx/component --force", Path.Combine(model.RootDirectory, model.Name));
    }
}
