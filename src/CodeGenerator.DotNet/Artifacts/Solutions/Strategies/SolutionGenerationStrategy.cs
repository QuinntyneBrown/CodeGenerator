// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.DotNet.Artifacts.Projects.Services;
using CodeGenerator.DotNet.Services;

namespace CodeGenerator.DotNet.Artifacts.Solutions.Strategies;

using ContentFileModel = CodeGenerator.DotNet.Artifacts.Files.ContentFileModel;

public class SolutionGenerationStrategy : IArtifactGenerationStrategy<SolutionModel>
{
    private readonly ICommandService _commandService;
    private readonly IFileSystem _fileSystem;
    private readonly IProjectService _projectService;
    private readonly IArtifactGenerator _artifactGenerator;

    public SolutionGenerationStrategy(ICommandService commandService, IFileSystem fileSystem, IProjectService projectService, IArtifactGenerator artifactGenerator)
    {
        ArgumentNullException.ThrowIfNull(commandService);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(projectService);
        ArgumentNullException.ThrowIfNull(artifactGenerator);

        _commandService = commandService;
        _fileSystem = fileSystem;
        _projectService = projectService;
        _artifactGenerator = artifactGenerator;
    }

    public async Task GenerateAsync(SolutionModel model)
    {
        if (_fileSystem.Directory.Exists(model.SolutionDirectory))
        {
            try
            {
                _fileSystem.Directory.Delete(model.SolutionDirectory, true);
            }
            catch
            {
            }
        }

        _fileSystem.Directory.CreateDirectory(model.SolutionDirectory);

        _fileSystem.Directory.CreateDirectory(model.SrcDirectory);

        _commandService.Start($"dotnet new sln -n {model.Name}", model.SolutionDirectory);

        var solutionDocsDirectory = _fileSystem.Path.Combine(model.SolutionDirectory, "docs");

        _fileSystem.Directory.CreateDirectory(solutionDocsDirectory);

        await _artifactGenerator.GenerateAsync(new ContentFileModel($"# {model.Name}", "README", solutionDocsDirectory, ".md"));

        await _artifactGenerator.GenerateAsync(new ContentFileModel($"# {model.Name}", "README", model.SolutionDirectory, ".md"));

        foreach (var project in model.Projects)
        {
            await _artifactGenerator.GenerateAsync(project);

            await _projectService.AddToSolution(project);
        }

        foreach (var dependOn in model.DependOns)
        {
            _commandService.Start($"dotnet add {dependOn.Client.Directory} reference {dependOn.Service.Path}");
        }
    }
}
