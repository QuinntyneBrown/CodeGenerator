// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.ReactNative.Artifacts;

public class ProjectGenerationStrategy : IArtifactGenerationStrategy<ProjectModel>
{
    private readonly ICommandService commandService;
    private readonly ILogger<ProjectGenerationStrategy> logger;

    public ProjectGenerationStrategy(
        ICommandService commandService,
        ILogger<ProjectGenerationStrategy> logger)
    {
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int Priority => 1;

    public bool CanHandle(ProjectModel model) => model is ProjectModel;

    public async Task GenerateAsync(ProjectModel model)
    {
        logger.LogInformation("Create React Native Project. {name}", model.Name);

        commandService.Start($"npx react-native@latest init {model.Name} --template react-native-template-typescript", model.RootDirectory);

        logger.LogInformation("Installing dependencies for {name}", model.Name);

        commandService.Start("npm install @react-navigation/native @react-navigation/stack zustand axios react-native-safe-area-context react-native-screens react-native-gesture-handler", model.Directory);
    }
}
