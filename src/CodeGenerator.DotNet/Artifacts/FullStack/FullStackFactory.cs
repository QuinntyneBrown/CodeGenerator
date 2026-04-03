// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.DotNet.Artifacts.Projects;
using CodeGenerator.DotNet.Artifacts.Projects.Enums;
using CodeGenerator.DotNet.Artifacts.Solutions.Factories;
using CodeGenerator.DotNet.Options;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Artifacts.FullStack;

public class FullStackFactory : IFullStackFactory
{
    private readonly ILogger<FullStackFactory> _logger;
    private readonly ISolutionFactory _solutionFactory;

    public FullStackFactory(ILogger<FullStackFactory> logger, ISolutionFactory solutionFactory)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(solutionFactory);

        _logger = logger;
        _solutionFactory = solutionFactory;
    }

    public async Task<FullStackModel> CreateAsync(FullStackCreateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.Name))
        {
            throw new ArgumentException("A solution name is required.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.Directory))
        {
            throw new ArgumentException("An output directory is required.", nameof(options));
        }

        _logger.LogInformation("Creating full-stack solution model for {Name}", options.Name);

        var solution = await _solutionFactory.CleanArchitectureMicroservice(new CreateCleanArchitectureMicroserviceOptions
        {
            Name = options.Name,
            Directory = options.Directory,
            SolutionDirectory = options.SolutionDirectory,
        });

        var frontendProject = new ProjectModel(
            DotNetProjectType.TypeScriptStandalone,
            string.IsNullOrWhiteSpace(options.FrontendProjectName) ? $"{options.Name}.Web" : options.FrontendProjectName,
            solution.SrcDirectory);

        solution.Projects.Add(frontendProject);

        return new FullStackModel
        {
            Solution = solution,
            FrontendProject = frontendProject,
            BackendProject = solution.Projects.FirstOrDefault(x => x.Name.EndsWith(".Api", StringComparison.OrdinalIgnoreCase)),
        };
    }
}
