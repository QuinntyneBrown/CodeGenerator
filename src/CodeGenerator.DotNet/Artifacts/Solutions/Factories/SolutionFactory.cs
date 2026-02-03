// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO;
using CodeGenerator.DotNet.Artifacts.Projects.Factories;
using CodeGenerator.DotNet.Options;
using CodeGenerator.DotNet.Services;

namespace CodeGenerator.DotNet.Artifacts.Solutions.Factories;

public class SolutionFactory : ISolutionFactory
{
    private readonly IProjectFactory projectFactory;
    private readonly IContext context;

    public SolutionFactory(IProjectFactory projectFactory, IContext context)
    {
        this.projectFactory = projectFactory ?? throw new ArgumentNullException(nameof(projectFactory));
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<SolutionModel> Create(string name)
    {
        var model = new SolutionModel() { Name = name };

        return model;
    }

    public async Task<SolutionModel> Create(string name, string projectName, string dotNetProjectTypeName, string folderName, string directory)
    {
        var model = new SolutionModel(name, directory);

        if (dotNetProjectTypeName?.ToLower() == "microservice")
        {
            // Create three projects for microservice architecture
            var coreProject = await projectFactory.CreateCore(projectName, model.SrcDirectory);
            var infrastructureProject = await projectFactory.CreateInfrastructure(projectName, model.SrcDirectory);
            var apiProject = await projectFactory.CreateApi(projectName, model.SrcDirectory);

            model.Projects.Add(apiProject);
            model.Projects.Add(coreProject);
            model.Projects.Add(infrastructureProject);

            // Set up project dependencies
            model.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
            model.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));
        }
        else
        {
            var project = await projectFactory.Create(dotNetProjectTypeName, projectName, model.SrcDirectory);

            model.Projects.Add(project);
        }

        return model;
    }

    public async Task<SolutionModel> CreateHttpSolution(CreateCodeGeneratorSolutionOptions options)
    {
        var solutionModel = new SolutionModel(options.Name, options.Directory);

        solutionModel.Projects.Add(await projectFactory.CreateHttpProject(options.Name, solutionModel.SrcDirectory));

        return solutionModel;
    }

    public async Task<SolutionModel> Minimal(CreateCodeGeneratorSolutionOptions options)
    {
        var model = string.IsNullOrEmpty(options.SolutionDirectory) ? new SolutionModel(options.Name, options.Directory) : new SolutionModel(options.Name, options.Directory, options.SolutionDirectory);

        var minimalApiProject = await projectFactory.CreateMinimalApiProject(new CreateMinimalApiProjectOptions
        {
            Name = $"{options.Name}.Api",
            ShortIdPropertyName = false,
            NumericIdPropertyDataType = false,
            Resource = options.Resource,
            Properties = options.Properties,
            Port = 5000,
            Directory = model.SrcDirectory,
            DbContextName = options.DbContextName,
        });

        var unitTestProject = await projectFactory.CreateMinimalApiUnitTestsProject(options.Name, model.TestDirectory, options.Resource);

        model.Projects.Add(minimalApiProject);

        model.Projects.Add(unitTestProject);

        model.DependOns.Add(new DependsOnModel(unitTestProject, minimalApiProject));

        return model;
    }

    public async Task<SolutionModel> CleanArchitectureMicroservice(CreateCleanArchitectureMicroserviceOptions options)
    {
        var model = string.IsNullOrEmpty(options.SolutionDirectory) ? new SolutionModel(options.Name, options.Directory) : new SolutionModel(options.Name, options.Directory, options.SolutionDirectory);

        var domain = await projectFactory.CreateLibrary($"{options.Name}.Domain", model.SrcDirectory);

        domain.Metadata.Add(Constants.ProjectType.Domain);

        var infrastructure = await projectFactory.CreateLibrary($"{options.Name}.Infrastructure", model.SrcDirectory);

        infrastructure.Metadata.Add(Constants.ProjectType.Infrastructure);

        var application = await projectFactory.CreateLibrary($"{options.Name}.Application", model.SrcDirectory);

        application.Metadata.Add(Constants.ProjectType.Application);

        var api = await projectFactory.CreateWebApi($"{options.Name}.Api", model.SrcDirectory);

        api.Metadata.Add(Constants.ProjectType.Api);

        model.Projects.Add(api);

        model.Projects.Add(domain);

        model.Projects.Add(infrastructure);

        model.Projects.Add(application);

        model.DependOns.Add(new DependsOnModel(infrastructure, domain));

        model.DependOns.Add(new DependsOnModel(application, domain));

        model.DependOns.Add(new DependsOnModel(api, application));

        model.DependOns.Add(new DependsOnModel(api, infrastructure));

        return model;
    }

    public async Task<SolutionModel> Workspace(ResolveOrCreateWorkspaceOptions options)
    {
        var model = new SolutionModel(nameof(Workspace), options.Directory, $"{options.Directory}{Path.DirectorySeparatorChar}{options.Name}");

        return model;
    }

    public async Task<SolutionModel> Resolve(ResolveOrCreateWorkspaceOptions options)
    {
        var model = new SolutionModel(nameof(Workspace), options.Directory, $"{options.Directory}{Path.DirectorySeparatorChar}{options.Name}");

        return model;
    }

    public async Task<SolutionModel> ResolveCleanArchitectureMicroservice(UpdateCleanArchitectureMicroserviceOptions options)
    {
        return new SolutionModel
        {
        };
    }

    public async Task<SolutionModel> DddCreateAync(string name, string directory)
    {
        throw new NotImplementedException();
    }
}
