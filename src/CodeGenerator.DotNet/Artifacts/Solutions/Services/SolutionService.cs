// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO.Abstractions;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Internal;
using CodeGenerator.DotNet.Artifacts.Files;
using CodeGenerator.DotNet.Artifacts.Projects;
using CodeGenerator.DotNet.Artifacts.Projects.Enums;
using CodeGenerator.DotNet.Artifacts.Projects.Factories;
using CodeGenerator.DotNet.Artifacts.Units;
using CodeGenerator.DotNet.Services;
using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Classes.Services;
using MediatR;

namespace CodeGenerator.DotNet.Artifacts.Solutions.Services;

public class SolutionService : ISolutionService
{
    private readonly IArtifactGenerator artifactGenerator;
    private readonly IProjectFactory projectFactory;
    private readonly IDomainDrivenDesignFileService domainDrivenDesignFileService;
    private readonly IDomainDrivenDesignService domainDrivenDesignService;
    private readonly Observable<INotification> notificationListener;
    private readonly IFileProvider fileProvider;
    private readonly ICommandService commandService;
    private readonly IFileSystem _fileSystem;

    public SolutionService(
        IArtifactGenerator artifactGenerator,
        IProjectFactory projectFactory,
        IDomainDrivenDesignFileService domainDrivenDesignFileService,
        IDomainDrivenDesignService domainDrivenDesignService,
        Observable<INotification> notificationListener,
        IFileProvider fileProvider,
        ICommandService commandService,
        IFileSystem fileSystem)
    {
        this.artifactGenerator = artifactGenerator;
        this.projectFactory = projectFactory;
        this.domainDrivenDesignFileService = domainDrivenDesignFileService;
        this.domainDrivenDesignService = domainDrivenDesignService;
        this.notificationListener = notificationListener;
        this.fileProvider = fileProvider;
        this.commandService = commandService;
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task AddSolutionItem(string path)
    {
        throw new NotImplementedException();
    }

    public async Task Create(SolutionModel model)
    {
        await artifactGenerator.GenerateAsync(model);
    }

    public async Task EventDrivenMicroservicesCreate(string name, string services, string directory)
    {
        var notifications = new List<INotification>();

        var solutionModel = new SolutionModel(name, directory);

        var buildingBlocksFolder = await BuildingBlocksCreate(solutionModel.SolutionDirectory);
        solutionModel.Folders.Add(buildingBlocksFolder);
        
        // Add building blocks projects to the solution
        foreach (var project in buildingBlocksFolder.Projects)
        {
            solutionModel.Projects.Add(project);
        }

        var servicesFolder = await ServicesCreate(services, solutionModel.SolutionDirectory, notifications);
        solutionModel.Folders.Add(servicesFolder);
        
        // Add services projects to the solution
        foreach (var project in servicesFolder.Projects)
        {
            solutionModel.Projects.Add(project);
        }

        await artifactGenerator.GenerateAsync(solutionModel);

        foreach (var notification in notifications)
        {
            notificationListener.Broadcast(notification);
        }
    }

    public async Task Create(string name, string plantUmlSourcePath, string directory)
    {
    }

    public async Task MessagingBuildingBlockAdd(string directory)
    {
        var solutionPath = fileProvider.Get("*.sln", directory);

        var solutionName = _fileSystem.Path.GetFileName(solutionPath);

        var solutionDirectory = _fileSystem.Path.GetDirectoryName(solutionPath);

        var buildingBlocksDirectory = _fileSystem.Path.Combine(solutionDirectory, "src", "BuildingBlocks");

        var messagingDirectory = _fileSystem.Path.Combine(buildingBlocksDirectory, "Messaging");

        if (!_fileSystem.Directory.Exists(buildingBlocksDirectory))
        {
            _fileSystem.Directory.CreateDirectory(buildingBlocksDirectory);
        }

        if (!_fileSystem.Directory.Exists(messagingDirectory))
        {
            _fileSystem.Directory.CreateDirectory(messagingDirectory);
        }

        var messagingProjectModel = await projectFactory.CreateMessagingProject(messagingDirectory);

        await artifactGenerator.GenerateAsync(messagingProjectModel);

        commandService.Start($"dotnet sln {solutionName} add {messagingProjectModel.Path}", solutionDirectory);

        var messagingUdpProjectModel = await projectFactory.CreateMessagingUdpProject(messagingDirectory);

        await artifactGenerator.GenerateAsync(messagingUdpProjectModel);

        commandService.Start($"dotnet sln {solutionName} add {messagingUdpProjectModel.Path}", solutionDirectory);
    }

    public async Task IOCompressionBuildingBlockAdd(string directory)
    {
        var solutionPath = fileProvider.Get("*.sln", directory);

        var solutionName = _fileSystem.Path.GetFileName(solutionPath);

        var solutionDirectory = _fileSystem.Path.GetDirectoryName(solutionPath);

        var buildingBlocksDirectory = _fileSystem.Path.Combine(solutionDirectory, "src", "BuildingBlocks");

        var messagingDirectory = _fileSystem.Path.Combine(buildingBlocksDirectory, "IO.Compression");

        if (!_fileSystem.Directory.Exists(buildingBlocksDirectory))
        {
            _fileSystem.Directory.CreateDirectory(buildingBlocksDirectory);
        }

        if (!_fileSystem.Directory.Exists(messagingDirectory))
        {
            _fileSystem.Directory.CreateDirectory(messagingDirectory);
        }

        var messagingProjectModel = await projectFactory.CreateMessagingProject(messagingDirectory);

        await artifactGenerator.GenerateAsync(messagingProjectModel);

        commandService.Start($"dotnet sln {solutionName} add {messagingProjectModel.Path}", solutionDirectory);

        var messagingUdpProjectModel = await projectFactory.CreateMessagingUdpProject(messagingDirectory);

        await artifactGenerator.GenerateAsync(messagingUdpProjectModel);

        commandService.Start($"dotnet sln {solutionName} add {messagingUdpProjectModel.Path}", solutionDirectory);
    }

    private async Task<dynamic> BuildingBlocksCreate(string directory)
    {
        var buildingBlocksDirectory = _fileSystem.Path.Combine(directory, "src", "BuildingBlocks");
        
        var messagingDirectory = _fileSystem.Path.Combine(buildingBlocksDirectory, "Messaging");
        
        _fileSystem.Directory.CreateDirectory(messagingDirectory);
        
        var projects = new List<ProjectModel>();
        
        var messagingProjectModel = await projectFactory.CreateMessagingProject(messagingDirectory);
        projects.Add(messagingProjectModel);
        
        var messagingUdpProjectModel = await projectFactory.CreateMessagingUdpProject(messagingDirectory);
        projects.Add(messagingUdpProjectModel);
        
        return new { Name = "BuildingBlocks", Directory = buildingBlocksDirectory, Projects = projects };
    }

    private async Task<dynamic> ServicesCreate(string services, string directory, List<INotification> notifications)
    {
        var servicesDirectory = _fileSystem.Path.Combine(directory, "src", "Services");
        
        _fileSystem.Directory.CreateDirectory(servicesDirectory);
        
        var projects = new List<ProjectModel>();
        
        if (string.IsNullOrEmpty(services))
        {
            return new { Name = "Services", Directory = servicesDirectory, Projects = projects };
        }
        
        var serviceNames = services.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var serviceName in serviceNames)
        {
            var trimmedServiceName = serviceName.Trim();
            var serviceDirectory = _fileSystem.Path.Combine(servicesDirectory, trimmedServiceName);
            
            _fileSystem.Directory.CreateDirectory(serviceDirectory);
            
            // Create Core project
            var coreDirectory = _fileSystem.Path.Combine(serviceDirectory, $"{trimmedServiceName}.Core");
            var coreProject = await projectFactory.CreateCore(trimmedServiceName, coreDirectory);
            projects.Add(coreProject);
            
            // Create Infrastructure project
            var infrastructureDirectory = _fileSystem.Path.Combine(serviceDirectory, $"{trimmedServiceName}.Infrastructure");
            var infrastructureProject = await projectFactory.CreateInfrastructure(trimmedServiceName, infrastructureDirectory);
            projects.Add(infrastructureProject);
            
            // Create Api project
            var apiDirectory = _fileSystem.Path.Combine(serviceDirectory, $"{trimmedServiceName}.Api");
            var apiProject = await projectFactory.CreateApi(trimmedServiceName, apiDirectory);
            projects.Add(apiProject);
        }
        
        return new { Name = "Services", Directory = servicesDirectory, Projects = projects };
    }

    private dynamic AppsCreate(string directory)
    {
        throw new NotImplementedException();
    }
}
