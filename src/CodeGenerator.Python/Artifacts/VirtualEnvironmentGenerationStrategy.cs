// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Python.Artifacts;

public class VirtualEnvironmentGenerationStrategy : IArtifactGenerationStrategy<VirtualEnvironmentModel>
{
    private readonly ICommandService commandService;
    private readonly ILogger<VirtualEnvironmentGenerationStrategy> logger;

    public VirtualEnvironmentGenerationStrategy(
        ICommandService commandService,
        ILogger<VirtualEnvironmentGenerationStrategy> logger)
    {
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int Priority => 1;

    public bool CanHandle(VirtualEnvironmentModel model) => model is VirtualEnvironmentModel;

    public async Task GenerateAsync(VirtualEnvironmentModel model)
    {
        logger.LogInformation("Creating Python virtual environment. {name}", model.Name);

        commandService.Start($"python -m venv {model.Name}", model.Directory);
    }
}
