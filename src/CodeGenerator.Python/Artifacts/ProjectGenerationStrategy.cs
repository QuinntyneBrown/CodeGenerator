// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Python.Artifacts;

public class ProjectGenerationStrategy : IArtifactGenerationStrategy<ProjectModel>
{
    private readonly ICommandService commandService;
    private readonly ILogger<ProjectGenerationStrategy> logger;
    private readonly IFileSystem fileSystem;

    public ProjectGenerationStrategy(
        ICommandService commandService,
        IFileSystem fileSystem,
        ILogger<ProjectGenerationStrategy> logger)
    {
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int Priority => 1;

    public bool CanHandle(ProjectModel model) => model is ProjectModel;

    public async Task GenerateAsync(ProjectModel model)
    {
        logger.LogInformation("Create Python Project. {name}", model.Name);

        fileSystem.Directory.CreateDirectory(model.Directory);

        var srcDirectory = Path.Combine(model.Directory, model.Name.Replace("-", "_"));

        fileSystem.Directory.CreateDirectory(srcDirectory);

        fileSystem.File.WriteAllText(
            Path.Combine(srcDirectory, "__init__.py"),
            string.Empty);

        if (model.ProjectType == Constants.ProjectType.Flask)
        {
            fileSystem.Directory.CreateDirectory(Path.Combine(model.Directory, "templates"));
            fileSystem.Directory.CreateDirectory(Path.Combine(model.Directory, "static"));
        }

        if (model.ProjectType == Constants.ProjectType.Django)
        {
            commandService.Start($"python -m django startproject {model.Name} .", model.Directory);
        }

        commandService.Start("python -m venv .venv", model.Directory);

        if (model.Packages.Count > 0)
        {
            var packages = string.Join(" ", model.Packages.Select(p =>
                string.IsNullOrEmpty(p.Version) ? p.Name : $"{p.Name}=={p.Version}"));

            commandService.Start($".venv/bin/pip install {packages}", model.Directory);
        }
    }
}
