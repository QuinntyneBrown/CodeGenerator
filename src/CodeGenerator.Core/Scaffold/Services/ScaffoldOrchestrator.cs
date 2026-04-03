// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Scaffold.Models;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Scaffold.Services;

public class ScaffoldOrchestrator : IScaffoldOrchestrator
{
    private readonly IArchitectureResolver _architectureResolver;
    private readonly IProjectScaffolder _projectScaffolder;
    private readonly ISolutionScaffolder _solutionScaffolder;
    private readonly ICrossProjectReferenceResolver _referenceResolver;
    private readonly ILogger<ScaffoldOrchestrator> _logger;

    public ScaffoldOrchestrator(
        IArchitectureResolver architectureResolver,
        IProjectScaffolder projectScaffolder,
        ISolutionScaffolder solutionScaffolder,
        ICrossProjectReferenceResolver referenceResolver,
        ILogger<ScaffoldOrchestrator> logger)
    {
        _architectureResolver = architectureResolver;
        _projectScaffolder = projectScaffolder;
        _solutionScaffolder = solutionScaffolder;
        _referenceResolver = referenceResolver;
        _logger = logger;
    }

    public async Task<List<PlannedFile>> OrchestrateAsync(ScaffoldConfiguration config, string outputPath, bool dryRun = false, bool force = false)
    {
        var allPlanned = new List<PlannedFile>();

        if (!dryRun)
        {
            Directory.CreateDirectory(outputPath);
        }

        // Resolve architectures and expand projects
        var expandedProjects = ResolveArchitectures(config);

        // Merge global + project variables
        var globalVars = new Dictionary<string, string>(config.GlobalVariables);

        // Scaffold each project
        foreach (var project in expandedProjects)
        {
            var mergedVars = new Dictionary<string, string>(globalVars);
            foreach (var (key, value) in project.Variables)
            {
                mergedVars[key] = value;
            }

            var projectFiles = await _projectScaffolder.ScaffoldAsync(project, outputPath, mergedVars, dryRun);
            allPlanned.AddRange(projectFiles);
        }

        // Generate solutions
        foreach (var solution in config.Solutions)
        {
            var solutionFiles = await _solutionScaffolder.ScaffoldAsync(solution, config, outputPath, dryRun);
            allPlanned.AddRange(solutionFiles);
        }

        // Resolve cross-project references
        if (!dryRun)
        {
            _referenceResolver.Resolve(config, outputPath);
        }

        return allPlanned;
    }

    private List<ProjectDefinition> ResolveArchitectures(ScaffoldConfiguration config)
    {
        var expandedProjects = new List<ProjectDefinition>();

        foreach (var project in config.Projects)
        {
            if (!string.IsNullOrEmpty(project.Architecture) || project.Layers.Count > 0)
            {
                var resolved = _architectureResolver.Resolve(project);
                _logger.LogInformation("Resolved architecture '{Pattern}' for project '{Name}' into {Count} layers",
                    resolved.Pattern, project.Name, resolved.Layers.Count);

                foreach (var layer in resolved.Layers)
                {
                    var layerType = Enum.TryParse<ScaffoldProjectType>(
                        layer.Type?.Replace("-", string.Empty),
                        ignoreCase: true,
                        out var parsed)
                        ? parsed
                        : project.Type;

                    expandedProjects.Add(new ProjectDefinition
                    {
                        Name = layer.Name,
                        Type = layerType,
                        Path = layer.Path,
                        Framework = project.Framework,
                        References = layer.References,
                        Entities = layer.Entities,
                        Endpoints = layer.Endpoints,
                        Services = layer.Services,
                        Variables = project.Variables,
                    });
                }
            }
            else
            {
                expandedProjects.Add(project);
            }
        }

        return expandedProjects;
    }
}
