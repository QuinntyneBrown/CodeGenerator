// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.React.Artifacts;

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
        logger.LogInformation("Generating React Workspace. {name}", model.Name);

        commandService.Start($"npm create vite@latest {model.Name} -- --template react-ts", model.RootDirectory);

        var workspaceDirectory = Path.Combine(model.RootDirectory, model.Name);

        commandService.Start($"npm install --registry=https://registry.npmjs.org/ @tanstack/react-query --force", workspaceDirectory);

        commandService.Start($"npm install --registry=https://registry.npmjs.org/ zustand --force", workspaceDirectory);

        commandService.Start($"npm install --registry=https://registry.npmjs.org/ react-router --force", workspaceDirectory);

        commandService.Start($"npm install --registry=https://registry.npmjs.org/ axios --force", workspaceDirectory);

        commandService.Start($"npm install --registry=https://registry.npmjs.org/ tailwindcss --force", workspaceDirectory);

        commandService.Start($"npm install --registry=https://registry.npmjs.org/ vitest --save-dev --force", workspaceDirectory);
    }
}
