// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Scaffold.Models;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Scaffold.Services;

public class SolutionScaffolder : ISolutionScaffolder
{
    private readonly ICommandService _commandService;
    private readonly ILogger<SolutionScaffolder> _logger;

    public SolutionScaffolder(ICommandService commandService, ILogger<SolutionScaffolder> logger)
    {
        _commandService = commandService;
        _logger = logger;
    }

    public Task<List<PlannedFile>> ScaffoldAsync(SolutionDefinition solution, ScaffoldConfiguration config, string outputPath, bool dryRun = false)
    {
        var planned = new List<PlannedFile>();

        if (dryRun)
        {
            var ext = solution.Format == "slnx" ? ".slnx" : ".sln";
            planned.Add(new PlannedFile { Path = Path.Combine(outputPath, $"{solution.Name}{ext}"), Action = PlannedFileAction.Create });
            return Task.FromResult(planned);
        }

        var isDotNet = solution.Projects
            .Select(pn => config.Projects.FirstOrDefault(p => p.Name.Equals(pn, StringComparison.OrdinalIgnoreCase)))
            .Any(p => p != null && IsDotNetProject(p.Type));

        if (isDotNet)
        {
            ScaffoldDotNetSolution(solution, config, outputPath, planned);
        }
        else
        {
            ScaffoldNpmWorkspace(solution, config, outputPath, planned);
        }

        return Task.FromResult(planned);
    }

    private void ScaffoldDotNetSolution(SolutionDefinition solution, ScaffoldConfiguration config, string outputPath, List<PlannedFile> planned)
    {
        _logger.LogInformation("Creating .NET solution: {Name}", solution.Name);

        var cmd = solution.Format == "slnx"
            ? $"dotnet new slnx -n {solution.Name}"
            : $"dotnet new sln -n {solution.Name}";

        _commandService.Start(cmd, outputPath);

        var ext = solution.Format == "slnx" ? ".slnx" : ".sln";
        planned.Add(new PlannedFile { Path = Path.Combine(outputPath, $"{solution.Name}{ext}"), Action = PlannedFileAction.Create });

        foreach (var projectName in solution.Projects)
        {
            var project = config.Projects.FirstOrDefault(p => p.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase));
            if (project != null)
            {
                var projectPath = Path.Combine(outputPath, project.Path);
                _commandService.Start($"dotnet sln add {projectPath}", outputPath);
            }
        }
    }

    private static void ScaffoldNpmWorkspace(SolutionDefinition solution, ScaffoldConfiguration config, string outputPath, List<PlannedFile> planned)
    {
        var workspaces = solution.Projects
            .Select(pn => config.Projects.FirstOrDefault(p => p.Name.Equals(pn, StringComparison.OrdinalIgnoreCase)))
            .Where(p => p != null)
            .Select(p => p!.Path)
            .ToList();

        var packageJson = $$"""
            {
              "name": "{{solution.Name.ToLowerInvariant()}}",
              "private": true,
              "workspaces": [
                {{string.Join(",\n    ", workspaces.Select(w => $"\"{w}\""))}}
              ]
            }
            """;

        var filePath = Path.Combine(outputPath, "package.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, packageJson);
        planned.Add(new PlannedFile { Path = filePath, Action = PlannedFileAction.Create });
    }

    private static bool IsDotNetProject(ScaffoldProjectType type) =>
        type is ScaffoldProjectType.DotnetWebapi or ScaffoldProjectType.DotnetClasslib or ScaffoldProjectType.DotnetConsole;
}
