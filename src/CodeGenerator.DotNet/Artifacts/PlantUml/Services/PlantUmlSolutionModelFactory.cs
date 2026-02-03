// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeGenerator.DotNet.Artifacts.Files;
using CodeGenerator.DotNet.Artifacts.PlantUml.Models;
using CodeGenerator.DotNet.Artifacts.Projects;
using CodeGenerator.DotNet.Artifacts.Projects.Enums;
using CodeGenerator.DotNet.Artifacts.Solutions;
using CodeGenerator.DotNet.Services;
using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Classes.Factories;
using CodeGenerator.DotNet.Syntax.Constructors;
using CodeGenerator.DotNet.Syntax.Entities;
using CodeGenerator.DotNet.Syntax.Methods;
using CodeGenerator.DotNet.Syntax.Params;
using CodeGenerator.DotNet.Syntax.Properties;
using CodeGenerator.DotNet.Syntax.Types;
using CodeGenerator.DotNet.Syntax.Units;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Artifacts.PlantUml.Services;

using IFileFactory = CodeGenerator.DotNet.Artifacts.Files.Factories.IFileFactory;
using TypeModel = CodeGenerator.DotNet.Syntax.Types.TypeModel;

public class PlantUmlSolutionModelFactory : IPlantUmlSolutionModelFactory
{
    private readonly ILogger<PlantUmlSolutionModelFactory> logger;
    private readonly IClassFactory classFactory;
    private readonly IFileFactory fileFactory;
    private readonly INamingConventionConverter namingConventionConverter;

    public PlantUmlSolutionModelFactory(
        ILogger<PlantUmlSolutionModelFactory> logger,
        IClassFactory classFactory,
        IFileFactory fileFactory,
        INamingConventionConverter namingConventionConverter)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.classFactory = classFactory ?? throw new ArgumentNullException(nameof(classFactory));
        this.fileFactory = fileFactory ?? throw new ArgumentNullException(nameof(fileFactory));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<SolutionModel> CreateAsync(PlantUmlSolutionModel plantUmlModel, string solutionName, string outputDirectory, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating solution model from PlantUML: {SolutionName}", solutionName);

        var solution = new SolutionModel(solutionName, outputDirectory);
        var boundedContexts = plantUmlModel.GetBoundedContexts().ToList();

        int projectOrder = 1;

        // Create Angular projects from components with Angular stereotype
        var angularComponents = plantUmlModel.GetAllComponents()
            .Where(c => string.Equals(c.Stereotype, "Angular", StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        // Also look at participants with Angular or ts type
        var angularParticipants = plantUmlModel.GetAllParticipants()
            .Where(p => string.Equals(p.DotNetType, "Angular", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(p.DotNetType, "ts", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Create projects for Angular components
        foreach (var component in angularComponents)
        {
            logger.LogInformation("Creating Angular project for component: {Name}", component.Name);
            var angularProject = CreateAngularProject(component.Name, solutionName, solution.SrcDirectory, ref projectOrder);
            solution.Projects.Add(angularProject);
        }

        // Create projects for Angular participants
        foreach (var participant in angularParticipants)
        {
            logger.LogInformation("Creating Angular project for participant: {Name}", participant.Name);
            var angularProject = CreateAngularProject(participant.Name, solutionName, solution.SrcDirectory, ref projectOrder);
            solution.Projects.Add(angularProject);
        }

        // Create Worker projects from components with Worker stereotype
        var workerComponents = plantUmlModel.GetAllComponents()
            .Where(c => string.Equals(c.Stereotype, "Worker", StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        // Also look at participants with Worker type
        var workerParticipants = plantUmlModel.GetAllParticipants()
            .Where(p => string.Equals(p.DotNetType, "Worker", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Create projects for Worker components
        foreach (var component in workerComponents)
        {
            logger.LogInformation("Creating Worker project for component: {Name}", component.Name);
            var workerProject = CreateWorkerProject(component.Name, solutionName, solution.SrcDirectory, ref projectOrder);
            solution.Projects.Add(workerProject);
        }

        // Create projects for Worker participants
        foreach (var participant in workerParticipants)
        {
            logger.LogInformation("Creating Worker project for participant: {Name}", participant.Name);
            var workerProject = CreateWorkerProject(participant.Name, solutionName, solution.SrcDirectory, ref projectOrder);
            solution.Projects.Add(workerProject);
        }

        if (boundedContexts.Any())
        {
            // Multi-bounded context solution
            logger.LogInformation("Detected {Count} bounded context(s): {BoundedContexts}",
                boundedContexts.Count, string.Join(", ", boundedContexts));

            foreach (var boundedContext in boundedContexts)
            {
                var bcEntities = plantUmlModel.GetEntitiesByBoundedContext(boundedContext).ToList();
                var bcEnums = plantUmlModel.GetEnumsByBoundedContext(boundedContext).ToList();

                logger.LogInformation("Generating projects for bounded context: {BoundedContext} ({EntityCount} entities)",
                    boundedContext, bcEntities.Count);

                // Create Core project for this bounded context
                var coreProject = CreateBoundedContextCoreProject(bcEntities, bcEnums, solutionName, boundedContext, solution.SrcDirectory, ref projectOrder);
                solution.Projects.Add(coreProject);

                // Create Infrastructure project for this bounded context
                var infrastructureProject = CreateBoundedContextInfrastructureProject(bcEntities, solutionName, boundedContext, solution.SrcDirectory, ref projectOrder);
                solution.Projects.Add(infrastructureProject);

                // Create Api project for this bounded context
                var apiProject = CreateBoundedContextApiProject(bcEntities, solutionName, boundedContext, solution.SrcDirectory, ref projectOrder);
                solution.Projects.Add(apiProject);

                // Set up project dependencies within bounded context
                solution.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));
                solution.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
            }
        }
        else
        {
            // Single context solution (legacy behavior)
            logger.LogInformation("No bounded contexts detected, using single context approach");

            // Create Core project
            var coreProject = CreateCoreProject(plantUmlModel, solutionName, solution.SrcDirectory);
            solution.Projects.Add(coreProject);

            // Create Infrastructure project
            var infrastructureProject = CreateInfrastructureProject(plantUmlModel, solutionName, solution.SrcDirectory);
            solution.Projects.Add(infrastructureProject);

            // Create Api project
            var apiProject = CreateApiProject(plantUmlModel, solutionName, solution.SrcDirectory);
            solution.Projects.Add(apiProject);

            // Set up project dependencies
            solution.DependOns.Add(new DependsOnModel(apiProject, infrastructureProject));
            solution.DependOns.Add(new DependsOnModel(infrastructureProject, coreProject));
        }

        return solution;
    }

    private ProjectModel CreateCoreProject(PlantUmlSolutionModel plantUmlModel, string solutionName, string srcDirectory)
    {
        var projectName = $"{solutionName}.Core";
        var project = new ProjectModel(DotNetProjectType.ClassLib, projectName, srcDirectory)
        {
            Order = 1
        };

        // Add NuGet packages
        project.Packages.Add(new PackageModel("MediatR", "12.2.0"));
        project.Packages.Add(new PackageModel("FluentValidation", "11.9.0"));
        project.Packages.Add(new PackageModel("FluentValidation.DependencyInjectionExtensions", "11.9.0"));
        project.Packages.Add(new PackageModel("Microsoft.EntityFrameworkCore", "8.0.0"));
        project.Packages.Add(new PackageModel("Microsoft.Extensions.Logging.Abstractions", "8.0.0"));

        // Generate entity models
        foreach (var entity in plantUmlModel.GetEntities())
        {
            GenerateEntityFiles(project, entity, plantUmlModel, solutionName);
        }

        // Generate enum files
        foreach (var enumModel in plantUmlModel.GetAllEnums())
        {
            GenerateEnumFile(project, enumModel);
        }

        // Generate IContext interface in Core project
        GenerateContextInterface(project, plantUmlModel, solutionName);

        return project;
    }

    private void GenerateContextInterface(ProjectModel project, PlantUmlSolutionModel solutionModel, string solutionName)
    {
        var contextDirectory = Path.Combine(project.Directory, "Data");
        var entities = solutionModel.GetEntities().ToList();
        var interfaceContent = GenerateCoreContextInterfaceContent(entities, solutionName, project.Namespace);
        project.Files.Add(new Files.ContentFileModel(interfaceContent, $"I{solutionName}Context", contextDirectory, ".cs"));
    }

    private string GenerateCoreContextInterfaceContent(List<PlantUmlClassModel> entities, string solutionName, string projectNamespace)
    {
        // Use type aliases to avoid namespace collision when entity name matches namespace segment
        var typeAliases = string.Join("\n", entities.Select(e => $"using {e.Name}Entity = {projectNamespace}.Aggregates.{e.Name}.{e.Name};"));
        var dbSets = string.Join("\n    ", entities.Select(e => $"DbSet<{e.Name}Entity> {GetPluralName(e.Name)} {{ get; set; }}"));

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
{typeAliases}

namespace {projectNamespace}.Data;

public interface I{solutionName}Context
{{
    {dbSets}

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}}
";
    }

    private ProjectModel CreateInfrastructureProject(PlantUmlSolutionModel plantUmlModel, string solutionName, string srcDirectory)
    {
        var projectName = $"{solutionName}.Infrastructure";
        var project = new ProjectModel(DotNetProjectType.ClassLib, projectName, srcDirectory)
        {
            Order = 2
        };

        // Add NuGet packages
        project.Packages.Add(new PackageModel("Microsoft.EntityFrameworkCore", "8.0.0"));
        project.Packages.Add(new PackageModel("Microsoft.EntityFrameworkCore.SqlServer", "8.0.0"));
        project.Packages.Add(new PackageModel("Microsoft.EntityFrameworkCore.Design", "8.0.0"));
        project.Packages.Add(new PackageModel("Microsoft.EntityFrameworkCore.Tools", "8.0.0"));

        // Add reference to Core project
        project.References.Add($"..{Path.DirectorySeparatorChar}{solutionName}.Core{Path.DirectorySeparatorChar}{solutionName}.Core.csproj");

        // Generate DbContext
        GenerateDbContextFile(project, plantUmlModel, solutionName);

        return project;
    }

    private ProjectModel CreateApiProject(PlantUmlSolutionModel plantUmlModel, string solutionName, string srcDirectory)
    {
        var projectName = $"{solutionName}.Api";
        var project = new ProjectModel(DotNetProjectType.WebApi, projectName, srcDirectory)
        {
            Order = 3
        };

        // Add NuGet packages
        project.Packages.Add(new PackageModel("MediatR", "12.2.0"));
        project.Packages.Add(new PackageModel("Swashbuckle.AspNetCore", "6.5.0"));
        project.Packages.Add(new PackageModel("Swashbuckle.AspNetCore.Annotations", "6.5.0"));

        // Add reference to Infrastructure project
        project.References.Add($"..{Path.DirectorySeparatorChar}{solutionName}.Infrastructure{Path.DirectorySeparatorChar}{solutionName}.Infrastructure.csproj");

        // Generate controllers for aggregate roots
        foreach (var aggregate in plantUmlModel.GetAggregates())
        {
            GenerateControllerFile(project, aggregate, solutionName);
        }

        // Generate Program.cs
        GenerateProgramFile(project, solutionName);

        // Generate appsettings.json
        GenerateAppSettingsFile(project, solutionName);

        return project;
    }

    private ProjectModel CreateAngularProject(string participantName, string solutionName, string srcDirectory, ref int projectOrder)
    {
        // Convert participant name to PascalCase project name (e.g., "Admin Dashboard" -> "AdminDashboard")
        var projectName = SanitizeName(participantName);
        
        var project = new ProjectModel(DotNetProjectType.TypeScriptStandalone, projectName, srcDirectory)
        {
            Order = projectOrder++,
            Extension = ".esproj"
        };

        logger.LogInformation("Created Angular project: {ProjectName}", projectName);

        return project;
    }

    private ProjectModel CreateWorkerProject(string participantName, string solutionName, string srcDirectory, ref int projectOrder)
    {
        // Convert participant name to PascalCase project name (e.g., "Email Sender" -> "EmailSender")
        var projectName = SanitizeName(participantName);
        
        var project = new ProjectModel(DotNetProjectType.Worker, projectName, srcDirectory)
        {
            Order = projectOrder++
        };

        // Add common Worker NuGet packages
        project.Packages.Add(new PackageModel("Microsoft.Extensions.Hosting", "8.0.0"));

        logger.LogInformation("Created Worker project: {ProjectName}", projectName);

        return project;
    }

    private string SanitizeName(string name)
    {
        const string DefaultProjectName = "DefaultName";
        
        // Remove spaces and special characters, convert to PascalCase
        var parts = name.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return DefaultProjectName;
        }
        
        return string.Join("", parts.Select(p => 
        {
            if (string.IsNullOrEmpty(p)) return string.Empty;
            return char.ToUpper(p[0]) + (p.Length > 1 ? p.Substring(1).ToLower() : string.Empty);
        }));
    }

    #region Bounded Context Project Creation

    private ProjectModel CreateBoundedContextCoreProject(
        List<PlantUmlClassModel> entities,
        List<PlantUmlEnumModel> enums,
        string solutionName,
        string boundedContext,
        string srcDirectory,
        ref int projectOrder)
    {
        var projectName = $"{solutionName}.{boundedContext}.Core";
        var project = new ProjectModel(DotNetProjectType.ClassLib, projectName, srcDirectory)
        {
            Order = projectOrder++
        };

        // Add NuGet packages
        project.Packages.Add(new PackageModel("MediatR", "12.2.0"));
        project.Packages.Add(new PackageModel("FluentValidation", "11.9.0"));
        project.Packages.Add(new PackageModel("FluentValidation.DependencyInjectionExtensions", "11.9.0"));
        project.Packages.Add(new PackageModel("Microsoft.EntityFrameworkCore", "8.0.0"));
        project.Packages.Add(new PackageModel("Microsoft.Extensions.Logging.Abstractions", "8.0.0"));

        // Generate entity models
        foreach (var entity in entities)
        {
            GenerateBoundedContextEntityFiles(project, entity, solutionName, boundedContext);
        }

        // Generate enum files
        foreach (var enumModel in enums)
        {
            GenerateBoundedContextEnumFile(project, enumModel, boundedContext);
        }

        // Generate IContext interface in Core project
        GenerateBoundedContextContextInterface(project, entities, solutionName, boundedContext);

        return project;
    }

    private ProjectModel CreateBoundedContextInfrastructureProject(
        List<PlantUmlClassModel> entities,
        string solutionName,
        string boundedContext,
        string srcDirectory,
        ref int projectOrder)
    {
        var projectName = $"{solutionName}.{boundedContext}.Infrastructure";
        var coreProjectName = $"{solutionName}.{boundedContext}.Core";
        var project = new ProjectModel(DotNetProjectType.ClassLib, projectName, srcDirectory)
        {
            Order = projectOrder++
        };

        // Add NuGet packages
        project.Packages.Add(new PackageModel("Microsoft.EntityFrameworkCore", "8.0.0"));
        project.Packages.Add(new PackageModel("Microsoft.EntityFrameworkCore.SqlServer", "8.0.0"));
        project.Packages.Add(new PackageModel("Microsoft.EntityFrameworkCore.Design", "8.0.0"));
        project.Packages.Add(new PackageModel("Microsoft.EntityFrameworkCore.Tools", "8.0.0"));

        // Add reference to Core project
        project.References.Add($"..{Path.DirectorySeparatorChar}{coreProjectName}{Path.DirectorySeparatorChar}{coreProjectName}.csproj");

        // Generate DbContext
        GenerateBoundedContextDbContextFile(project, entities, solutionName, boundedContext);

        return project;
    }

    private ProjectModel CreateBoundedContextApiProject(
        List<PlantUmlClassModel> entities,
        string solutionName,
        string boundedContext,
        string srcDirectory,
        ref int projectOrder)
    {
        var projectName = $"{solutionName}.{boundedContext}.Api";
        var infrastructureProjectName = $"{solutionName}.{boundedContext}.Infrastructure";
        var project = new ProjectModel(DotNetProjectType.WebApi, projectName, srcDirectory)
        {
            Order = projectOrder++
        };

        // Add NuGet packages
        project.Packages.Add(new PackageModel("MediatR", "12.2.0"));
        project.Packages.Add(new PackageModel("Swashbuckle.AspNetCore", "6.5.0"));
        project.Packages.Add(new PackageModel("Swashbuckle.AspNetCore.Annotations", "6.5.0"));

        // Add reference to Infrastructure project
        project.References.Add($"..{Path.DirectorySeparatorChar}{infrastructureProjectName}{Path.DirectorySeparatorChar}{infrastructureProjectName}.csproj");

        // Generate controllers for aggregate roots
        var aggregates = entities.Where(e => e.IsAggregate).ToList();
        foreach (var aggregate in aggregates)
        {
            GenerateBoundedContextControllerFile(project, aggregate, solutionName, boundedContext);
        }

        // Generate Program.cs
        GenerateBoundedContextProgramFile(project, solutionName, boundedContext);

        // Generate appsettings.json
        GenerateBoundedContextAppSettingsFile(project, solutionName, boundedContext);

        return project;
    }

    private void GenerateBoundedContextEntityFiles(ProjectModel project, PlantUmlClassModel plantUmlClass, string solutionName, string boundedContext)
    {
        var entityName = plantUmlClass.Name;
        var entityDirectory = Path.Combine(project.Directory, "Aggregates", entityName);

        // Generate entity class
        var entityClass = CreateEntityClassModel(plantUmlClass, project.Namespace);
        project.Files.Add(new CodeFileModel<ClassModel>(entityClass, entityClass.Usings, entityName, entityDirectory, ".cs"));

        // Generate DTO class
        var dtoClass = CreateDtoClassModel(plantUmlClass, project.Namespace);
        project.Files.Add(new CodeFileModel<ClassModel>(dtoClass, dtoClass.Usings, $"{entityName}Dto", entityDirectory, ".cs"));

        // Generate Extensions class
        var extensionsClass = CreateExtensionsClassModel(plantUmlClass);
        project.Files.Add(new CodeFileModel<ClassModel>(extensionsClass, extensionsClass.Usings, $"{entityName}Extensions", entityDirectory, ".cs"));

        // Generate CQRS files in Features folder
        var featuresDirectory = Path.Combine(project.Directory, "Features", entityName);

        if (plantUmlClass.IsAggregate)
        {
            GenerateBoundedContextCrudOperations(project, plantUmlClass, featuresDirectory, solutionName, boundedContext);
        }
    }

    private void GenerateBoundedContextEnumFile(ProjectModel project, PlantUmlEnumModel plantUmlEnum, string boundedContext)
    {
        // Place enum in the same folder as its parent aggregate if we can detect it
        string enumDirectory;
        string enumNamespace;

        if (!string.IsNullOrEmpty(plantUmlEnum.Namespace))
        {
            var namespaceParts = plantUmlEnum.Namespace.Split('.');
            var aggregatesIndex = Array.FindIndex(namespaceParts, p => p.Equals("Aggregates", StringComparison.OrdinalIgnoreCase));

            if (aggregatesIndex >= 0 && aggregatesIndex < namespaceParts.Length - 1)
            {
                var aggregateName = namespaceParts[aggregatesIndex + 1];
                enumDirectory = Path.Combine(project.Directory, "Aggregates", aggregateName);
                enumNamespace = $"{project.Namespace}.Aggregates.{aggregateName}";
            }
            else
            {
                enumDirectory = Path.Combine(project.Directory, "Models");
                enumNamespace = $"{project.Namespace}.Models";
            }
        }
        else
        {
            enumDirectory = Path.Combine(project.Directory, "Models");
            enumNamespace = $"{project.Namespace}.Models";
        }

        var content = GenerateEnumContent(plantUmlEnum, enumNamespace);
        project.Files.Add(new Files.ContentFileModel(content, plantUmlEnum.Name, enumDirectory, ".cs"));
    }

    private void GenerateBoundedContextContextInterface(ProjectModel project, List<PlantUmlClassModel> entities, string solutionName, string boundedContext)
    {
        var contextDirectory = Path.Combine(project.Directory, "Data");
        var contextName = $"{boundedContext}";
        var interfaceContent = GenerateBoundedContextContextInterfaceContent(entities, contextName, project.Namespace);
        project.Files.Add(new Files.ContentFileModel(interfaceContent, $"I{contextName}Context", contextDirectory, ".cs"));
    }

    private string GenerateBoundedContextContextInterfaceContent(List<PlantUmlClassModel> entities, string contextName, string projectNamespace)
    {
        // Use type aliases to avoid namespace collision when entity name matches namespace segment
        // e.g., OrderManagement class in ECommercePlatform.OrderManagement.Core.Aggregates.OrderManagement namespace
        var typeAliases = string.Join("\n", entities.Select(e => $"using {e.Name}Entity = {projectNamespace}.Aggregates.{e.Name}.{e.Name};"));
        var dbSets = string.Join("\n    ", entities.Select(e => $"DbSet<{e.Name}Entity> {GetPluralName(e.Name)} {{ get; set; }}"));

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
{typeAliases}

namespace {projectNamespace}.Data;

public interface I{contextName}Context
{{
    {dbSets}

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}}
";
    }

    private void GenerateBoundedContextDbContextFile(ProjectModel project, List<PlantUmlClassModel> entities, string solutionName, string boundedContext)
    {
        var dbContextDirectory = Path.Combine(project.Directory, "Data");
        var contextName = $"{boundedContext}";
        var coreNamespace = $"{solutionName}.{boundedContext}.Core";
        var content = GenerateBoundedContextDbContextContent(entities, contextName, project.Namespace, coreNamespace);
        project.Files.Add(new Files.ContentFileModel(content, $"{contextName}DbContext", dbContextDirectory, ".cs"));
    }

    private string GenerateBoundedContextDbContextContent(List<PlantUmlClassModel> entities, string contextName, string projectNamespace, string coreNamespace)
    {
        // Use type aliases to avoid namespace collision when entity name matches namespace segment
        // e.g., OrderManagement class in ECommercePlatform.OrderManagement.Core.Aggregates.OrderManagement namespace
        var typeAliases = string.Join("\n", entities.Select(e => $"using {e.Name}Entity = {coreNamespace}.Aggregates.{e.Name}.{e.Name};"));
        var dbSets = string.Join("\n    ", entities.Select(e => $"public DbSet<{e.Name}Entity> {GetPluralName(e.Name)} {{ get; set; }}"));

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using {coreNamespace}.Data;
{typeAliases}

namespace {projectNamespace}.Data;

public class {contextName}DbContext : DbContext, I{contextName}Context
{{
    public {contextName}DbContext(DbContextOptions<{contextName}DbContext> options)
        : base(options)
    {{
    }}

    {dbSets}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {{
        base.OnModelCreating(modelBuilder);
        // Configure entity relationships and constraints here
    }}
}}
";
    }

    private void GenerateBoundedContextControllerFile(ProjectModel project, PlantUmlClassModel aggregate, string solutionName, string boundedContext)
    {
        var entityName = aggregate.Name;
        var controllerDirectory = Path.Combine(project.Directory, "Controllers");
        var coreNamespace = $"{solutionName}.{boundedContext}.Core";
        var content = GenerateBoundedContextControllerContent(entityName, project.Namespace, coreNamespace);
        project.Files.Add(new Files.ContentFileModel(content, $"{entityName}Controller", controllerDirectory, ".cs"));
    }

    private string GenerateBoundedContextControllerContent(string entityName, string projectNamespace, string coreNamespace)
    {
        var entityNameCamelCase = char.ToLowerInvariant(entityName[0]) + entityName[1..];
        var keyPropertyName = $"{entityName}Id";

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.AspNetCore.Mvc;
using {coreNamespace}.Features.{entityName};

namespace {projectNamespace}.Controllers;

[ApiController]
[Route(""api/[controller]"")]
public class {entityName}Controller : ControllerBase
{{
    private readonly IMediator _mediator;

    public {entityName}Controller(IMediator mediator)
    {{
        _mediator = mediator;
    }}

    [HttpGet]
    public async Task<ActionResult<Get{entityName}sResponse>> Get([FromQuery] Get{entityName}sRequest request, CancellationToken cancellationToken)
    {{
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }}

    [HttpGet(""{{{entityNameCamelCase}Id}}"")]
    public async Task<ActionResult<Get{entityName}ByIdResponse>> GetById([FromRoute] string {entityNameCamelCase}Id, CancellationToken cancellationToken)
    {{
        var request = new Get{entityName}ByIdRequest {{ {keyPropertyName} = {entityNameCamelCase}Id }};
        var response = await _mediator.Send(request, cancellationToken);
        return response.{entityName} == null ? NotFound() : Ok(response);
    }}

    [HttpPost]
    public async Task<ActionResult<Create{entityName}Response>> Create([FromBody] Create{entityName}Request request, CancellationToken cancellationToken)
    {{
        var response = await _mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new {{ {entityNameCamelCase}Id = response.{entityName}?.{keyPropertyName} }}, response);
    }}

    [HttpPut(""{{{entityNameCamelCase}Id}}"")]
    public async Task<ActionResult<Update{entityName}Response>> Update([FromRoute] string {entityNameCamelCase}Id, [FromBody] Update{entityName}Request request, CancellationToken cancellationToken)
    {{
        request.{keyPropertyName} = {entityNameCamelCase}Id;
        var response = await _mediator.Send(request, cancellationToken);
        return response.{entityName} == null ? NotFound() : Ok(response);
    }}

    [HttpDelete(""{{{entityNameCamelCase}Id}}"")]
    public async Task<ActionResult<Delete{entityName}Response>> Delete([FromRoute] string {entityNameCamelCase}Id, CancellationToken cancellationToken)
    {{
        var request = new Delete{entityName}Request {{ {keyPropertyName} = {entityNameCamelCase}Id }};
        var response = await _mediator.Send(request, cancellationToken);
        return response.Success ? Ok(response) : NotFound(response);
    }}
}}
";
    }

    private void GenerateBoundedContextProgramFile(ProjectModel project, string solutionName, string boundedContext)
    {
        var coreNamespace = $"{solutionName}.{boundedContext}.Core";
        var infrastructureNamespace = $"{solutionName}.{boundedContext}.Infrastructure";
        var contextName = boundedContext;
        var content = $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using {coreNamespace}.Data;
using {infrastructureNamespace}.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCodeGeneratorsApiExplorer();
builder.Services.AddSwaggerGen();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(I{contextName}Context).Assembly));

// Add DbContext
builder.Services.AddDbContext<{contextName}DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString(""{contextName}Connection"")));

builder.Services.AddScoped<I{contextName}Context>(sp => sp.GetRequiredService<{contextName}DbContext>());

// CORS
builder.Services.AddCors(options =>
{{
    options.AddPolicy(""AllowSpecificOrigins"", policy =>
    {{
        var origins = builder.Configuration[""WithOrigins""]?.Split(',') ?? new[] {{ ""http://localhost:4200"" }};
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    }});
}});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{{
    app.UseSwagger();
    app.UseSwaggerUI();
}}

app.UseHttpsRedirection();
app.UseCors(""AllowSpecificOrigins"");
app.UseAuthorization();
app.MapControllers();

app.Run();
";
        project.Files.Add(new Files.ContentFileModel(content, "Program", project.Directory, ".cs"));
    }

    private void GenerateBoundedContextAppSettingsFile(ProjectModel project, string solutionName, string boundedContext)
    {
        var databaseName = $"{solutionName}{boundedContext}Db";
        var connectionName = $"{boundedContext}Connection";
        var content = $@"{{
  ""Logging"": {{
    ""LogLevel"": {{
      ""Default"": ""Information"",
      ""Microsoft.AspNetCore"": ""Warning""
    }}
  }},
  ""AllowedHosts"": ""*"",
  ""ConnectionStrings"": {{
    ""{connectionName}"": ""Server=.\\SQLEXPRESS;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true""
  }},
  ""WithOrigins"": ""http://localhost:4200""
}}";
        project.Files.Add(new Files.ContentFileModel(content, "appsettings", project.Directory, ".json"));
    }

    private void GenerateBoundedContextCrudOperations(ProjectModel project, PlantUmlClassModel entity, string directory, string solutionName, string boundedContext)
    {
        var entityName = entity.Name;
        var coreNamespace = $"{solutionName}.{boundedContext}.Core";

        // Generate Create operation
        GenerateBoundedContextCreateOperation(project, entity, directory, coreNamespace);

        // Generate GetById operation
        GenerateBoundedContextGetByIdOperation(project, entity, directory, coreNamespace);

        // Generate GetAll operation
        GenerateBoundedContextGetAllOperation(project, entity, directory, coreNamespace);

        // Generate Update operation
        GenerateBoundedContextUpdateOperation(project, entity, directory, coreNamespace);

        // Generate Delete operation
        GenerateBoundedContextDeleteOperation(project, entity, directory, coreNamespace);
    }

    private void GenerateBoundedContextCreateOperation(ProjectModel project, PlantUmlClassModel entity, string directory, string coreNamespace)
    {
        var entityName = entity.Name;
        var aggregateNamespace = $"{coreNamespace}.Aggregates.{entityName}";
        var featuresNamespace = $"{coreNamespace}.Features.{entityName}";
        var keyProperty = GetKeyProperty(entity);
        var keyPropertyName = keyProperty?.Name ?? $"{entityName}Id";

        // Create Request
        var requestClass = new ClassModel($"Create{entityName}Request");
        requestClass.Usings.Add(new UsingModel("MediatR"));
        requestClass.Usings.Add(new UsingModel(aggregateNamespace));
        requestClass.Implements.Add(new TypeModel($"IRequest<Create{entityName}Response>"));
        foreach (var property in entity.Properties.Where(p => !p.IsKey && !IsAuditProperty(p.Name)))
        {
            requestClass.Properties.Add(CreatePropertyModel(requestClass, property, coreNamespace));
        }
        project.Files.Add(new CodeFileModel<ClassModel>(requestClass, requestClass.Usings, $"Create{entityName}Request", directory, ".cs"));

        // Create Response
        var responseClass = new ClassModel($"Create{entityName}Response");
        responseClass.Usings.Add(new UsingModel(aggregateNamespace));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel($"{entityName}Dto"), entityName, PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(responseClass, responseClass.Usings, $"Create{entityName}Response", directory, ".cs"));

        // Create Handler with implementation
        var handlerContent = GenerateBoundedContextCreateHandlerContent(entity, coreNamespace, keyPropertyName);
        project.Files.Add(new Files.ContentFileModel(handlerContent, $"Create{entityName}Handler", directory, ".cs"));
    }

    private string GenerateBoundedContextCreateHandlerContent(PlantUmlClassModel entity, string coreNamespace, string keyPropertyName)
    {
        var entityName = entity.Name;
        var entityNameCamelCase = char.ToLowerInvariant(entityName[0]) + entityName[1..];

        var propertyAssignments = entity.Properties
            .Where(p => !p.IsKey && !IsAuditProperty(p.Name))
            .Select(p => $"            {p.Name} = request.{p.Name},")
            .ToList();

        var assignmentBlock = string.Join("\n", propertyAssignments);

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using {coreNamespace}.Data;
using {entityName}Entity = {coreNamespace}.Aggregates.{entityName}.{entityName};
using {entityName}Ext = {coreNamespace}.Aggregates.{entityName}.{entityName}Extensions;

namespace {coreNamespace}.Features.{entityName};

public class Create{entityName}Handler : IRequestHandler<Create{entityName}Request, Create{entityName}Response>
{{
    private readonly I{entity.BoundedContext}Context _context;

    public Create{entityName}Handler(I{entity.BoundedContext}Context context)
    {{
        _context = context;
    }}

    public async Task<Create{entityName}Response> Handle(Create{entityName}Request request, CancellationToken cancellationToken)
    {{
        var {entityNameCamelCase} = new {entityName}Entity
        {{
            {keyPropertyName} = Guid.NewGuid().ToString(),
{assignmentBlock}
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        }};

        _context.{GetPluralName(entityName)}.Add({entityNameCamelCase});
        await _context.SaveChangesAsync(cancellationToken);

        return new Create{entityName}Response
        {{
            {entityName} = {entityName}Ext.ToDto({entityNameCamelCase})
        }};
    }}
}}
";
    }

    private void GenerateBoundedContextGetByIdOperation(ProjectModel project, PlantUmlClassModel entity, string directory, string coreNamespace)
    {
        var entityName = entity.Name;
        var aggregateNamespace = $"{coreNamespace}.Aggregates.{entityName}";
        var keyProperty = GetKeyProperty(entity);
        var keyPropertyName = keyProperty?.Name ?? $"{entityName}Id";

        // GetById Request
        var requestClass = new ClassModel($"Get{entityName}ByIdRequest");
        requestClass.Usings.Add(new UsingModel("MediatR"));
        requestClass.Implements.Add(new TypeModel($"IRequest<Get{entityName}ByIdResponse>"));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("string"), keyPropertyName, PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(requestClass, requestClass.Usings, $"Get{entityName}ByIdRequest", directory, ".cs"));

        // GetById Response
        var responseClass = new ClassModel($"Get{entityName}ByIdResponse");
        responseClass.Usings.Add(new UsingModel(aggregateNamespace));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel($"{entityName}Dto"), entityName, PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(responseClass, responseClass.Usings, $"Get{entityName}ByIdResponse", directory, ".cs"));

        // GetById Handler
        var handlerContent = GenerateBoundedContextGetByIdHandlerContent(entity, coreNamespace, keyPropertyName);
        project.Files.Add(new Files.ContentFileModel(handlerContent, $"Get{entityName}ByIdHandler", directory, ".cs"));
    }

    private string GenerateBoundedContextGetByIdHandlerContent(PlantUmlClassModel entity, string coreNamespace, string keyPropertyName)
    {
        var entityName = entity.Name;
        var entityNameCamelCase = char.ToLowerInvariant(entityName[0]) + entityName[1..];

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using {coreNamespace}.Data;
using {entityName}Ext = {coreNamespace}.Aggregates.{entityName}.{entityName}Extensions;

namespace {coreNamespace}.Features.{entityName};

public class Get{entityName}ByIdHandler : IRequestHandler<Get{entityName}ByIdRequest, Get{entityName}ByIdResponse>
{{
    private readonly I{entity.BoundedContext}Context _context;

    public Get{entityName}ByIdHandler(I{entity.BoundedContext}Context context)
    {{
        _context = context;
    }}

    public async Task<Get{entityName}ByIdResponse> Handle(Get{entityName}ByIdRequest request, CancellationToken cancellationToken)
    {{
        var {entityNameCamelCase} = await _context.{GetPluralName(entityName)}
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.{keyPropertyName} == request.{keyPropertyName}, cancellationToken);

        return new Get{entityName}ByIdResponse
        {{
            {entityName} = {entityNameCamelCase} != null ? {entityName}Ext.ToDto({entityNameCamelCase}) : null
        }};
    }}
}}
";
    }

    private void GenerateBoundedContextGetAllOperation(ProjectModel project, PlantUmlClassModel entity, string directory, string coreNamespace)
    {
        var entityName = entity.Name;
        var aggregateNamespace = $"{coreNamespace}.Aggregates.{entityName}";

        // GetAll Request
        var requestClass = new ClassModel($"Get{entityName}sRequest");
        requestClass.Usings.Add(new UsingModel("MediatR"));
        requestClass.Implements.Add(new TypeModel($"IRequest<Get{entityName}sResponse>"));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("int"), "PageIndex", PropertyAccessorModel.GetSet));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("int"), "PageSize", PropertyAccessorModel.GetSet));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("string"), "SearchTerm", PropertyAccessorModel.GetSet));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("string"), "SortBy", PropertyAccessorModel.GetSet));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("bool"), "SortDescending", PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(requestClass, requestClass.Usings, $"Get{entityName}sRequest", directory, ".cs"));

        // GetAll Response
        var responseClass = new ClassModel($"Get{entityName}sResponse");
        responseClass.Usings.Add(new UsingModel(aggregateNamespace));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel($"List<{entityName}Dto>"), $"{entityName}s", PropertyAccessorModel.GetSet));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel("int"), "TotalCount", PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(responseClass, responseClass.Usings, $"Get{entityName}sResponse", directory, ".cs"));

        // GetAll Handler
        var handlerContent = GenerateBoundedContextGetAllHandlerContent(entity, coreNamespace);
        project.Files.Add(new Files.ContentFileModel(handlerContent, $"Get{entityName}sHandler", directory, ".cs"));
    }

    private string GenerateBoundedContextGetAllHandlerContent(PlantUmlClassModel entity, string coreNamespace)
    {
        var entityName = entity.Name;
        var entityNameCamelCase = char.ToLowerInvariant(entityName[0]) + entityName[1..];

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using {coreNamespace}.Data;
using {entityName}Ext = {coreNamespace}.Aggregates.{entityName}.{entityName}Extensions;

namespace {coreNamespace}.Features.{entityName};

public class Get{entityName}sHandler : IRequestHandler<Get{entityName}sRequest, Get{entityName}sResponse>
{{
    private readonly I{entity.BoundedContext}Context _context;

    public Get{entityName}sHandler(I{entity.BoundedContext}Context context)
    {{
        _context = context;
    }}

    public async Task<Get{entityName}sResponse> Handle(Get{entityName}sRequest request, CancellationToken cancellationToken)
    {{
        var query = _context.{GetPluralName(entityName)}.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var pageIndex = request.PageIndex > 0 ? request.PageIndex : 0;

        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new Get{entityName}sResponse
        {{
            {entityName}s = items.Select({entityName}Ext.ToDto).ToList(),
            TotalCount = totalCount
        }};
    }}
}}
";
    }

    private void GenerateBoundedContextUpdateOperation(ProjectModel project, PlantUmlClassModel entity, string directory, string coreNamespace)
    {
        var entityName = entity.Name;
        var aggregateNamespace = $"{coreNamespace}.Aggregates.{entityName}";
        var keyProperty = GetKeyProperty(entity);
        var keyPropertyName = keyProperty?.Name ?? $"{entityName}Id";

        // Update Request
        var requestClass = new ClassModel($"Update{entityName}Request");
        requestClass.Usings.Add(new UsingModel("MediatR"));
        requestClass.Usings.Add(new UsingModel(aggregateNamespace));
        requestClass.Implements.Add(new TypeModel($"IRequest<Update{entityName}Response>"));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("string"), keyPropertyName, PropertyAccessorModel.GetSet));
        foreach (var property in entity.Properties.Where(p => !p.IsKey && !IsAuditProperty(p.Name)))
        {
            requestClass.Properties.Add(CreatePropertyModel(requestClass, property, coreNamespace));
        }
        project.Files.Add(new CodeFileModel<ClassModel>(requestClass, requestClass.Usings, $"Update{entityName}Request", directory, ".cs"));

        // Update Response
        var responseClass = new ClassModel($"Update{entityName}Response");
        responseClass.Usings.Add(new UsingModel(aggregateNamespace));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel($"{entityName}Dto"), entityName, PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(responseClass, responseClass.Usings, $"Update{entityName}Response", directory, ".cs"));

        // Update Handler
        var handlerContent = GenerateBoundedContextUpdateHandlerContent(entity, coreNamespace, keyPropertyName);
        project.Files.Add(new Files.ContentFileModel(handlerContent, $"Update{entityName}Handler", directory, ".cs"));
    }

    private string GenerateBoundedContextUpdateHandlerContent(PlantUmlClassModel entity, string coreNamespace, string keyPropertyName)
    {
        var entityName = entity.Name;
        var entityNameCamelCase = char.ToLowerInvariant(entityName[0]) + entityName[1..];

        var propertyUpdates = entity.Properties
            .Where(p => !p.IsKey && !IsAuditProperty(p.Name))
            .Select(p => $"        {entityNameCamelCase}.{p.Name} = request.{p.Name};")
            .ToList();

        var updateBlock = string.Join("\n", propertyUpdates);

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using {coreNamespace}.Data;
using {entityName}Ext = {coreNamespace}.Aggregates.{entityName}.{entityName}Extensions;

namespace {coreNamespace}.Features.{entityName};

public class Update{entityName}Handler : IRequestHandler<Update{entityName}Request, Update{entityName}Response>
{{
    private readonly I{entity.BoundedContext}Context _context;

    public Update{entityName}Handler(I{entity.BoundedContext}Context context)
    {{
        _context = context;
    }}

    public async Task<Update{entityName}Response> Handle(Update{entityName}Request request, CancellationToken cancellationToken)
    {{
        var {entityNameCamelCase} = await _context.{GetPluralName(entityName)}
            .FirstOrDefaultAsync(x => x.{keyPropertyName} == request.{keyPropertyName}, cancellationToken);

        if ({entityNameCamelCase} == null)
        {{
            return new Update{entityName}Response {{ {entityName} = null }};
        }}

{updateBlock}
        {entityNameCamelCase}.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new Update{entityName}Response
        {{
            {entityName} = {entityName}Ext.ToDto({entityNameCamelCase})
        }};
    }}
}}
";
    }

    private void GenerateBoundedContextDeleteOperation(ProjectModel project, PlantUmlClassModel entity, string directory, string coreNamespace)
    {
        var entityName = entity.Name;
        var keyProperty = GetKeyProperty(entity);
        var keyPropertyName = keyProperty?.Name ?? $"{entityName}Id";

        // Delete Request
        var requestClass = new ClassModel($"Delete{entityName}Request");
        requestClass.Usings.Add(new UsingModel("MediatR"));
        requestClass.Implements.Add(new TypeModel($"IRequest<Delete{entityName}Response>"));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("string"), keyPropertyName, PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(requestClass, requestClass.Usings, $"Delete{entityName}Request", directory, ".cs"));

        // Delete Response
        var responseClass = new ClassModel($"Delete{entityName}Response");
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel("bool"), "Success", PropertyAccessorModel.GetSet));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel("string"), "Message", PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(responseClass, responseClass.Usings, $"Delete{entityName}Response", directory, ".cs"));

        // Delete Handler
        var handlerContent = GenerateBoundedContextDeleteHandlerContent(entity, coreNamespace, keyPropertyName);
        project.Files.Add(new Files.ContentFileModel(handlerContent, $"Delete{entityName}Handler", directory, ".cs"));
    }

    private string GenerateBoundedContextDeleteHandlerContent(PlantUmlClassModel entity, string coreNamespace, string keyPropertyName)
    {
        var entityName = entity.Name;
        var entityNameCamelCase = char.ToLowerInvariant(entityName[0]) + entityName[1..];

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using {coreNamespace}.Data;

namespace {coreNamespace}.Features.{entityName};

public class Delete{entityName}Handler : IRequestHandler<Delete{entityName}Request, Delete{entityName}Response>
{{
    private readonly I{entity.BoundedContext}Context _context;

    public Delete{entityName}Handler(I{entity.BoundedContext}Context context)
    {{
        _context = context;
    }}

    public async Task<Delete{entityName}Response> Handle(Delete{entityName}Request request, CancellationToken cancellationToken)
    {{
        var {entityNameCamelCase} = await _context.{GetPluralName(entityName)}
            .FirstOrDefaultAsync(x => x.{keyPropertyName} == request.{keyPropertyName}, cancellationToken);

        if ({entityNameCamelCase} == null)
        {{
            return new Delete{entityName}Response
            {{
                Success = false,
                Message = $""{entityName} with ID '{{request.{keyPropertyName}}}' was not found.""
            }};
        }}

        _context.{GetPluralName(entityName)}.Remove({entityNameCamelCase});
        await _context.SaveChangesAsync(cancellationToken);

        return new Delete{entityName}Response
        {{
            Success = true,
            Message = $""{entityName} with ID '{{request.{keyPropertyName}}}' was successfully deleted.""
        }};
    }}
}}
";
    }

    #endregion

    private void GenerateEntityFiles(ProjectModel project, PlantUmlClassModel plantUmlClass, PlantUmlSolutionModel solutionModel, string solutionName)
    {
        var entityName = plantUmlClass.Name;
        var entityDirectory = Path.Combine(project.Directory, "Aggregates", entityName);

        // Generate entity class
        var entityClass = CreateEntityClassModel(plantUmlClass, project.Namespace);
        project.Files.Add(new CodeFileModel<ClassModel>(entityClass, entityClass.Usings, entityName, entityDirectory, ".cs"));

        // Generate DTO class
        var dtoClass = CreateDtoClassModel(plantUmlClass, project.Namespace);
        project.Files.Add(new CodeFileModel<ClassModel>(dtoClass, dtoClass.Usings, $"{entityName}Dto", entityDirectory, ".cs"));

        // Generate Extensions class
        var extensionsClass = CreateExtensionsClassModel(plantUmlClass);
        project.Files.Add(new CodeFileModel<ClassModel>(extensionsClass, extensionsClass.Usings, $"{entityName}Extensions", entityDirectory, ".cs"));

        // Only generate CRUD operations for aggregate roots
        if (plantUmlClass.IsAggregate)
        {
            // Use "Features" prefix to avoid namespace collision with model names
            var cqrsDirectory = Path.Combine(project.Directory, "Features", entityName);
            GenerateCrudOperations(project, plantUmlClass, cqrsDirectory, solutionName);
        }
    }

    private ClassModel CreateEntityClassModel(PlantUmlClassModel plantUmlClass, string projectNamespace)
    {
        var classModel = new ClassModel(plantUmlClass.Name);

        foreach (var property in plantUmlClass.Properties)
        {
            var propertyModel = CreatePropertyModel(classModel, property, projectNamespace);
            classModel.Properties.Add(propertyModel);
        }

        return classModel;
    }

    private static bool IsPrimitiveType(string typeName)
    {
        var primitives = new[] { "string", "int", "long", "bool", "decimal", "double", "float", "DateTime", "DateTimeOffset", "Guid", "byte", "short" };
        return primitives.Contains(typeName, StringComparer.OrdinalIgnoreCase);
    }

    private ClassModel CreateDtoClassModel(PlantUmlClassModel plantUmlClass, string projectNamespace)
    {
        var classModel = new ClassModel($"{plantUmlClass.Name}Dto");

        foreach (var property in plantUmlClass.Properties)
        {
            var propertyModel = CreatePropertyModel(classModel, property, projectNamespace);
            classModel.Properties.Add(propertyModel);
        }

        return classModel;
    }

    private ClassModel CreateExtensionsClassModel(PlantUmlClassModel plantUmlClass)
    {
        var entityName = plantUmlClass.Name;
        var classModel = new ClassModel($"{entityName}Extensions")
        {
            Static = true
        };

        // Generate property mappings for ToDto
        var propertyMappings = new List<string>();
        foreach (var property in plantUmlClass.Properties)
        {
            propertyMappings.Add($"            {property.Name} = entity.{property.Name}");
        }

        var toDtoBody = $"return new {entityName}Dto\n        {{\n{string.Join(",\n", propertyMappings)}\n        }};";

        // Add ToDto method
        var toDtoMethod = new MethodModel
        {
            Name = "ToDto",
            ReturnType = new TypeModel($"{entityName}Dto"),
            Static = true,
            AccessModifier = AccessModifier.Public,
            Body = new Syntax.Expressions.ExpressionModel(toDtoBody)
        };
        toDtoMethod.Params.Add(new ParamModel
        {
            Name = "entity",
            Type = new TypeModel(entityName),
            ExtensionMethodParam = true
        });
        classModel.AddMethod(toDtoMethod);

        return classModel;
    }

    private PropertyModel CreatePropertyModel(TypeDeclarationModel parent, PlantUmlPropertyModel plantUmlProperty, string projectNamespace = null)
    {
        var typeModel = CreateTypeModel(plantUmlProperty, projectNamespace);
        var accessors = PropertyAccessorModel.GetSet;

        return new PropertyModel(
            parent,
            ConvertVisibility(plantUmlProperty.Visibility),
            typeModel,
            plantUmlProperty.Name,
            accessors,
            plantUmlProperty.IsRequired,
            plantUmlProperty.IsKey);
    }

    private TypeModel CreateTypeModel(PlantUmlPropertyModel property, string projectNamespace = null)
    {
        TypeModel typeModel;

        if (property.IsCollection && !string.IsNullOrEmpty(property.GenericTypeArgument))
        {
            // Use fully qualified name for non-primitive types to avoid namespace collision
            var genericTypeName = property.GenericTypeArgument;
            if (!IsPrimitiveType(genericTypeName) && !string.IsNullOrEmpty(projectNamespace))
            {
                genericTypeName = $"Aggregates.{property.GenericTypeArgument}.{property.GenericTypeArgument}";
            }

            typeModel = new TypeModel(property.CollectionType ?? "List")
            {
                GenericTypeParameters = new List<TypeModel>
                {
                    new TypeModel(genericTypeName)
                }
            };
        }
        else
        {
            typeModel = new TypeModel(MapTypeName(property.Type))
            {
                Nullable = property.IsNullable
            };
        }

        return typeModel;
    }

    private void GenerateCrudOperations(ProjectModel project, PlantUmlClassModel entity, string directory, string solutionName)
    {
        var entityName = entity.Name;

        // Create Request/Response/Handler for each CRUD operation
        GenerateCreateOperation(project, entity, directory, solutionName);
        GenerateGetByIdOperation(project, entity, directory, solutionName);
        GenerateGetAllOperation(project, entity, directory, solutionName);
        GenerateUpdateOperation(project, entity, directory, solutionName);
        GenerateDeleteOperation(project, entity, directory, solutionName);
    }

    private static PlantUmlPropertyModel? GetKeyProperty(PlantUmlClassModel entity)
    {
        return entity.Properties.FirstOrDefault(p => p.IsKey);
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private void GenerateCreateOperation(ProjectModel project, PlantUmlClassModel entity, string directory, string solutionName)
    {
        var entityName = entity.Name;
        var aggregateNamespace = $"{project.Namespace}.Aggregates.{entityName}";
        var keyProperty = GetKeyProperty(entity);
        var keyPropertyName = keyProperty?.Name ?? $"{entityName}Id";

        // Create Request
        var requestClass = new ClassModel($"Create{entityName}Request");
        requestClass.Usings.Add(new UsingModel("MediatR"));
        requestClass.Usings.Add(new UsingModel(aggregateNamespace));
        requestClass.Implements.Add(new TypeModel($"IRequest<Create{entityName}Response>"));
        foreach (var property in entity.Properties.Where(p => !p.IsKey && !IsAuditProperty(p.Name)))
        {
            requestClass.Properties.Add(CreatePropertyModel(requestClass, property, project.Namespace));
        }
        project.Files.Add(new CodeFileModel<ClassModel>(requestClass, requestClass.Usings, $"Create{entityName}Request", directory, ".cs"));

        // Create Response
        var responseClass = new ClassModel($"Create{entityName}Response");
        responseClass.Usings.Add(new UsingModel(aggregateNamespace));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel($"{entityName}Dto"), entityName, PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(responseClass, responseClass.Usings, $"Create{entityName}Response", directory, ".cs"));

        // Create Handler with implementation
        var handlerContent = GenerateCreateHandlerContent(entity, solutionName, project.Namespace, keyPropertyName);
        project.Files.Add(new Files.ContentFileModel(handlerContent, $"Create{entityName}Handler", directory, ".cs"));

        // Create Validator
        var validatorContent = GenerateCreateValidatorContent(entity, project.Namespace);
        project.Files.Add(new Files.ContentFileModel(validatorContent, $"Create{entityName}Validator", directory, ".cs"));
    }

    private string GenerateCreateHandlerContent(PlantUmlClassModel entity, string solutionName, string projectNamespace, string keyPropertyName)
    {
        var entityName = entity.Name;
        var entityNameCamel = ToCamelCase(entityName);
        var nonKeyProperties = entity.Properties.Where(p => !p.IsKey && !IsAuditProperty(p.Name)).ToList();

        var propertyAssignments = string.Join("\n            ", nonKeyProperties.Select(p => $"{p.Name} = request.{p.Name},"));

        // Use type alias to avoid namespace collision between Features.{EntityName} namespace and Aggregates.{EntityName}.{EntityName} type
        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using {projectNamespace}.Data;
using {entityName}Entity = {projectNamespace}.Aggregates.{entityName}.{entityName};
using {entityName}Ext = {projectNamespace}.Aggregates.{entityName}.{entityName}Extensions;

namespace {projectNamespace}.Features.{entityName};

public class Create{entityName}Handler : IRequestHandler<Create{entityName}Request, Create{entityName}Response>
{{
    private readonly I{solutionName}Context _context;

    public Create{entityName}Handler(I{solutionName}Context context)
    {{
        _context = context;
    }}

    public async Task<Create{entityName}Response> Handle(Create{entityName}Request request, CancellationToken cancellationToken)
    {{
        var {entityNameCamel} = new {entityName}Entity
        {{
            {keyPropertyName} = Guid.NewGuid().ToString(),
            {propertyAssignments}
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        }};

        _context.{GetPluralName(entityName)}.Add({entityNameCamel});
        await _context.SaveChangesAsync(cancellationToken);

        return new Create{entityName}Response
        {{
            {entityName} = {entityName}Ext.ToDto({entityNameCamel})
        }};
    }}
}}
";
    }

    private string GenerateCreateValidatorContent(PlantUmlClassModel entity, string projectNamespace)
    {
        var entityName = entity.Name;
        var requiredProperties = entity.Properties.Where(p => !p.IsKey && !IsAuditProperty(p.Name) && p.IsRequired && p.Type.ToLower() == "string").ToList();

        var rules = string.Join("\n        ", requiredProperties.Select(p => $"RuleFor(x => x.{p.Name}).NotEmpty();"));

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using FluentValidation;

namespace {projectNamespace}.Features.{entityName};

public class Create{entityName}Validator : AbstractValidator<Create{entityName}Request>
{{
    public Create{entityName}Validator()
    {{
        {rules}
    }}
}}
";
    }

    private void GenerateGetByIdOperation(ProjectModel project, PlantUmlClassModel entity, string directory, string solutionName)
    {
        var entityName = entity.Name;
        var aggregateNamespace = $"{project.Namespace}.Aggregates.{entityName}";
        var keyProperty = GetKeyProperty(entity);
        var keyPropertyName = keyProperty?.Name ?? $"{entityName}Id";

        // Create Request
        var requestClass = new ClassModel($"Get{entityName}ByIdRequest");
        requestClass.Usings.Add(new UsingModel("MediatR"));
        requestClass.Implements.Add(new TypeModel($"IRequest<Get{entityName}ByIdResponse>"));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("string"), keyPropertyName, PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(requestClass, requestClass.Usings, $"Get{entityName}ByIdRequest", directory, ".cs"));

        // Create Response
        var responseClass = new ClassModel($"Get{entityName}ByIdResponse");
        responseClass.Usings.Add(new UsingModel(aggregateNamespace));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel($"{entityName}Dto"), entityName, PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(responseClass, responseClass.Usings, $"Get{entityName}ByIdResponse", directory, ".cs"));

        // Create Handler with implementation
        var handlerContent = GenerateGetByIdHandlerContent(entity, solutionName, project.Namespace, keyPropertyName);
        project.Files.Add(new Files.ContentFileModel(handlerContent, $"Get{entityName}ByIdHandler", directory, ".cs"));
    }

    private string GenerateGetByIdHandlerContent(PlantUmlClassModel entity, string solutionName, string projectNamespace, string keyPropertyName)
    {
        var entityName = entity.Name;
        var entityNameCamel = ToCamelCase(entityName);

        // Use type alias to avoid namespace collision between Features.{EntityName} namespace and Aggregates.{EntityName}.{EntityName} type
        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using {projectNamespace}.Data;
using {entityName}Entity = {projectNamespace}.Aggregates.{entityName}.{entityName};
using {entityName}Ext = {projectNamespace}.Aggregates.{entityName}.{entityName}Extensions;

namespace {projectNamespace}.Features.{entityName};

public class Get{entityName}ByIdHandler : IRequestHandler<Get{entityName}ByIdRequest, Get{entityName}ByIdResponse>
{{
    private readonly I{solutionName}Context _context;

    public Get{entityName}ByIdHandler(I{solutionName}Context context)
    {{
        _context = context;
    }}

    public async Task<Get{entityName}ByIdResponse> Handle(Get{entityName}ByIdRequest request, CancellationToken cancellationToken)
    {{
        var {entityNameCamel} = await _context.{GetPluralName(entityName)}
            .FirstOrDefaultAsync(x => x.{keyPropertyName} == request.{keyPropertyName}, cancellationToken);

        if ({entityNameCamel} == null)
        {{
            throw new KeyNotFoundException($""{entityName} with ID '{{request.{keyPropertyName}}}' was not found."");
        }}

        return new Get{entityName}ByIdResponse
        {{
            {entityName} = {entityName}Ext.ToDto({entityNameCamel})
        }};
    }}
}}
";
    }

    private void GenerateGetAllOperation(ProjectModel project, PlantUmlClassModel entity, string directory, string solutionName)
    {
        var entityName = entity.Name;
        var pluralName = GetPluralName(entityName);
        var aggregateNamespace = $"{project.Namespace}.Aggregates.{entityName}";

        // Create Request
        var requestClass = new ClassModel($"Get{pluralName}Request");
        requestClass.Usings.Add(new UsingModel("MediatR"));
        requestClass.Implements.Add(new TypeModel($"IRequest<Get{pluralName}Response>"));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("int"), "PageIndex", PropertyAccessorModel.GetSet) { DefaultValue = "0" });
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("int"), "PageSize", PropertyAccessorModel.GetSet) { DefaultValue = "20" });
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "SearchTerm", PropertyAccessorModel.GetSet));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "SortBy", PropertyAccessorModel.GetSet));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("bool"), "SortDescending", PropertyAccessorModel.GetSet) { DefaultValue = "false" });
        project.Files.Add(new CodeFileModel<ClassModel>(requestClass, requestClass.Usings, $"Get{pluralName}Request", directory, ".cs"));

        // Create Response
        var responseClass = new ClassModel($"Get{pluralName}Response");
        responseClass.Usings.Add(new UsingModel(aggregateNamespace));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, TypeModel.ListOf($"{entityName}Dto"), pluralName, PropertyAccessorModel.GetSet) { DefaultValue = "new()" });
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel("int"), "TotalCount", PropertyAccessorModel.GetSet));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel("int"), "PageIndex", PropertyAccessorModel.GetSet));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel("int"), "PageSize", PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(responseClass, responseClass.Usings, $"Get{pluralName}Response", directory, ".cs"));

        // Create Handler with implementation
        var handlerContent = GenerateGetAllHandlerContent(entity, solutionName, project.Namespace);
        project.Files.Add(new Files.ContentFileModel(handlerContent, $"Get{pluralName}Handler", directory, ".cs"));
    }

    private string GenerateGetAllHandlerContent(PlantUmlClassModel entity, string solutionName, string projectNamespace)
    {
        var entityName = entity.Name;
        var pluralName = GetPluralName(entityName);
        var searchableProperties = entity.Properties.Where(p => p.Type.ToLower() == "string" && !p.IsKey).ToList();

        var searchConditions = searchableProperties.Any()
            ? string.Join(" ||\n                    ", searchableProperties.Select(p => $"x.{p.Name}.Contains(request.SearchTerm)"))
            : "false";

        // Use type alias to avoid namespace collision
        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using {projectNamespace}.Data;
using {entityName}Entity = {projectNamespace}.Aggregates.{entityName}.{entityName};
using {entityName}Ext = {projectNamespace}.Aggregates.{entityName}.{entityName}Extensions;

namespace {projectNamespace}.Features.{entityName};

public class Get{pluralName}Handler : IRequestHandler<Get{pluralName}Request, Get{pluralName}Response>
{{
    private readonly I{solutionName}Context _context;

    public Get{pluralName}Handler(I{solutionName}Context context)
    {{
        _context = context;
    }}

    public async Task<Get{pluralName}Response> Handle(Get{pluralName}Request request, CancellationToken cancellationToken)
    {{
        var query = _context.{pluralName}.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {{
            query = query.Where(x =>
                {searchConditions});
        }}

        // Get total count before paging
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {{
            ""createdat"" => request.SortDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            ""modifiedat"" => request.SortDescending ? query.OrderByDescending(x => x.ModifiedAt) : query.OrderBy(x => x.ModifiedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        }};

        // Apply paging
        var items = await query
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new Get{pluralName}Response
        {{
            {pluralName} = items.Select(x => {entityName}Ext.ToDto(x)).ToList(),
            TotalCount = totalCount,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        }};
    }}
}}
";
    }

    private void GenerateUpdateOperation(ProjectModel project, PlantUmlClassModel entity, string directory, string solutionName)
    {
        var entityName = entity.Name;
        var aggregateNamespace = $"{project.Namespace}.Aggregates.{entityName}";
        var keyProperty = GetKeyProperty(entity);
        var keyPropertyName = keyProperty?.Name ?? $"{entityName}Id";

        // Create Request
        var requestClass = new ClassModel($"Update{entityName}Request");
        requestClass.Usings.Add(new UsingModel("MediatR"));
        requestClass.Usings.Add(new UsingModel(aggregateNamespace));
        requestClass.Implements.Add(new TypeModel($"IRequest<Update{entityName}Response>"));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("string"), keyPropertyName, PropertyAccessorModel.GetSet));
        foreach (var property in entity.Properties.Where(p => !p.IsKey && !IsAuditProperty(p.Name)))
        {
            requestClass.Properties.Add(CreatePropertyModel(requestClass, property, project.Namespace));
        }
        project.Files.Add(new CodeFileModel<ClassModel>(requestClass, requestClass.Usings, $"Update{entityName}Request", directory, ".cs"));

        // Create Response
        var responseClass = new ClassModel($"Update{entityName}Response");
        responseClass.Usings.Add(new UsingModel(aggregateNamespace));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel($"{entityName}Dto"), entityName, PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(responseClass, responseClass.Usings, $"Update{entityName}Response", directory, ".cs"));

        // Create Handler with implementation
        var handlerContent = GenerateUpdateHandlerContent(entity, solutionName, project.Namespace, keyPropertyName);
        project.Files.Add(new Files.ContentFileModel(handlerContent, $"Update{entityName}Handler", directory, ".cs"));

        // Create Validator
        var validatorContent = GenerateUpdateValidatorContent(entity, project.Namespace, keyPropertyName);
        project.Files.Add(new Files.ContentFileModel(validatorContent, $"Update{entityName}Validator", directory, ".cs"));
    }

    private string GenerateUpdateHandlerContent(PlantUmlClassModel entity, string solutionName, string projectNamespace, string keyPropertyName)
    {
        var entityName = entity.Name;
        var entityNameCamel = ToCamelCase(entityName);
        var nonKeyProperties = entity.Properties.Where(p => !p.IsKey && !IsAuditProperty(p.Name)).ToList();

        var propertyUpdates = string.Join("\n        ", nonKeyProperties.Select(p => $"{entityNameCamel}.{p.Name} = request.{p.Name};"));

        // Use type alias to avoid namespace collision
        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using {projectNamespace}.Data;
using {entityName}Entity = {projectNamespace}.Aggregates.{entityName}.{entityName};
using {entityName}Ext = {projectNamespace}.Aggregates.{entityName}.{entityName}Extensions;

namespace {projectNamespace}.Features.{entityName};

public class Update{entityName}Handler : IRequestHandler<Update{entityName}Request, Update{entityName}Response>
{{
    private readonly I{solutionName}Context _context;

    public Update{entityName}Handler(I{solutionName}Context context)
    {{
        _context = context;
    }}

    public async Task<Update{entityName}Response> Handle(Update{entityName}Request request, CancellationToken cancellationToken)
    {{
        var {entityNameCamel} = await _context.{GetPluralName(entityName)}
            .FirstOrDefaultAsync(x => x.{keyPropertyName} == request.{keyPropertyName}, cancellationToken);

        if ({entityNameCamel} == null)
        {{
            throw new KeyNotFoundException($""{entityName} with ID '{{request.{keyPropertyName}}}' was not found."");
        }}

        // Update all properties
        {propertyUpdates}
        {entityNameCamel}.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new Update{entityName}Response
        {{
            {entityName} = {entityName}Ext.ToDto({entityNameCamel})
        }};
    }}
}}
";
    }

    private string GenerateUpdateValidatorContent(PlantUmlClassModel entity, string projectNamespace, string keyPropertyName)
    {
        var entityName = entity.Name;
        var requiredProperties = entity.Properties.Where(p => !p.IsKey && !IsAuditProperty(p.Name) && p.IsRequired && p.Type.ToLower() == "string").ToList();

        var rules = new List<string> { $"RuleFor(x => x.{keyPropertyName}).NotEmpty();" };
        rules.AddRange(requiredProperties.Select(p => $"RuleFor(x => x.{p.Name}).NotEmpty();"));
        var rulesStr = string.Join("\n        ", rules);

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using FluentValidation;

namespace {projectNamespace}.Features.{entityName};

public class Update{entityName}Validator : AbstractValidator<Update{entityName}Request>
{{
    public Update{entityName}Validator()
    {{
        {rulesStr}
    }}
}}
";
    }

    private void GenerateDeleteOperation(ProjectModel project, PlantUmlClassModel entity, string directory, string solutionName)
    {
        var entityName = entity.Name;
        var keyProperty = GetKeyProperty(entity);
        var keyPropertyName = keyProperty?.Name ?? $"{entityName}Id";

        // Create Request
        var requestClass = new ClassModel($"Delete{entityName}Request");
        requestClass.Usings.Add(new UsingModel("MediatR"));
        requestClass.Implements.Add(new TypeModel($"IRequest<Delete{entityName}Response>"));
        requestClass.Properties.Add(new PropertyModel(requestClass, AccessModifier.Public, new TypeModel("string"), keyPropertyName, PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(requestClass, requestClass.Usings, $"Delete{entityName}Request", directory, ".cs"));

        // Create Response
        var responseClass = new ClassModel($"Delete{entityName}Response");
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel("bool"), "Success", PropertyAccessorModel.GetSet));
        responseClass.Properties.Add(new PropertyModel(responseClass, AccessModifier.Public, new TypeModel("string"), "Message", PropertyAccessorModel.GetSet));
        project.Files.Add(new CodeFileModel<ClassModel>(responseClass, responseClass.Usings, $"Delete{entityName}Response", directory, ".cs"));

        // Create Handler with implementation
        var handlerContent = GenerateDeleteHandlerContent(entity, solutionName, project.Namespace, keyPropertyName);
        project.Files.Add(new Files.ContentFileModel(handlerContent, $"Delete{entityName}Handler", directory, ".cs"));
    }

    private string GenerateDeleteHandlerContent(PlantUmlClassModel entity, string solutionName, string projectNamespace, string keyPropertyName)
    {
        var entityName = entity.Name;
        var entityNameCamel = ToCamelCase(entityName);

        // Use type alias to avoid namespace collision
        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using {projectNamespace}.Data;
using {entityName}Entity = {projectNamespace}.Aggregates.{entityName}.{entityName};

namespace {projectNamespace}.Features.{entityName};

public class Delete{entityName}Handler : IRequestHandler<Delete{entityName}Request, Delete{entityName}Response>
{{
    private readonly I{solutionName}Context _context;

    public Delete{entityName}Handler(I{solutionName}Context context)
    {{
        _context = context;
    }}

    public async Task<Delete{entityName}Response> Handle(Delete{entityName}Request request, CancellationToken cancellationToken)
    {{
        var {entityNameCamel} = await _context.{GetPluralName(entityName)}
            .FirstOrDefaultAsync(x => x.{keyPropertyName} == request.{keyPropertyName}, cancellationToken);

        if ({entityNameCamel} == null)
        {{
            return new Delete{entityName}Response
            {{
                Success = false,
                Message = $""{entityName} with ID '{{request.{keyPropertyName}}}' was not found.""
            }};
        }}

        _context.{GetPluralName(entityName)}.Remove({entityNameCamel});
        await _context.SaveChangesAsync(cancellationToken);

        return new Delete{entityName}Response
        {{
            Success = true,
            Message = $""{entityName} with ID '{{request.{keyPropertyName}}}' was successfully deleted.""
        }};
    }}
}}
";
    }

    private void GenerateEnumFile(ProjectModel project, PlantUmlEnumModel plantUmlEnum)
    {
        // If the enum has a namespace (from a package), use that to determine the folder
        // Otherwise, place it in the Models folder
        string enumDirectory;
        string enumNamespace;

        if (!string.IsNullOrEmpty(plantUmlEnum.Namespace))
        {
            // Extract the last part of the namespace to determine the folder
            // e.g., "ToDo.Core.Aggregates.ToDoItem" -> place in Aggregates/ToDoItem
            var namespaceParts = plantUmlEnum.Namespace.Split('.');
            if (namespaceParts.Length >= 2)
            {
                // Check if this is inside an Aggregates namespace
                var aggregatesIndex = Array.IndexOf(namespaceParts, "Aggregates");
                if (aggregatesIndex >= 0 && aggregatesIndex < namespaceParts.Length - 1)
                {
                    var aggregateName = namespaceParts[aggregatesIndex + 1];
                    enumDirectory = Path.Combine(project.Directory, "Aggregates", aggregateName);
                    enumNamespace = $"{project.Namespace}.Aggregates.{aggregateName}";
                }
                else
                {
                    enumDirectory = Path.Combine(project.Directory, "Models");
                    enumNamespace = $"{project.Namespace}.Models";
                }
            }
            else
            {
                enumDirectory = Path.Combine(project.Directory, "Models");
                enumNamespace = $"{project.Namespace}.Models";
            }
        }
        else
        {
            enumDirectory = Path.Combine(project.Directory, "Models");
            enumNamespace = $"{project.Namespace}.Models";
        }

        var content = GenerateEnumContent(plantUmlEnum, enumNamespace);
        project.Files.Add(new Files.ContentFileModel(content, plantUmlEnum.Name, enumDirectory, ".cs"));
    }

    private string GenerateEnumContent(PlantUmlEnumModel enumModel, string enumNamespace)
    {
        var values = string.Join(",\n    ", enumModel.Values);
        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace {enumNamespace};

public enum {enumModel.Name}
{{
    {values}
}}
";
    }

    private void GenerateDbContextFile(ProjectModel project, PlantUmlSolutionModel solutionModel, string solutionName)
    {
        var dbContextDirectory = Path.Combine(project.Directory, "Data");
        var entities = solutionModel.GetEntities().ToList();
        var content = GenerateDbContextContent(entities, solutionName, project.Namespace);
        project.Files.Add(new Files.ContentFileModel(content, $"{solutionName}DbContext", dbContextDirectory, ".cs"));
        // Note: IContext interface is now generated in Core project
    }

    private string GenerateDbContextContent(List<PlantUmlClassModel> entities, string solutionName, string projectNamespace)
    {
        // Use type aliases to avoid namespace collision when entity name matches namespace segment
        var typeAliases = string.Join("\n", entities.Select(e => $"using {e.Name}Entity = {solutionName}.Core.Aggregates.{e.Name}.{e.Name};"));
        var dbSets = string.Join("\n    ", entities.Select(e => $"public DbSet<{e.Name}Entity> {GetPluralName(e.Name)} {{ get; set; }}"));

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using {solutionName}.Core.Data;
{typeAliases}

namespace {projectNamespace}.Data;

public class {solutionName}DbContext : DbContext, I{solutionName}Context
{{
    public {solutionName}DbContext(DbContextOptions<{solutionName}DbContext> options)
        : base(options)
    {{
    }}

    {dbSets}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {{
        base.OnModelCreating(modelBuilder);
    }}
}}
";
    }

    private void GenerateControllerFile(ProjectModel project, PlantUmlClassModel aggregate, string solutionName)
    {
        var entityName = aggregate.Name;
        var pluralName = GetPluralName(entityName);
        var controllerDirectory = Path.Combine(project.Directory, "Controllers");
        var content = GenerateControllerContent(aggregate, solutionName, project.Namespace);
        project.Files.Add(new Files.ContentFileModel(content, $"{entityName}Controller", controllerDirectory, ".cs"));
    }

    private string GenerateControllerContent(PlantUmlClassModel entity, string solutionName, string projectNamespace)
    {
        var entityName = entity.Name;
        var entityNameLower = char.ToLowerInvariant(entityName[0]) + entityName[1..];
        var pluralName = GetPluralName(entityName);

        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.AspNetCore.Mvc;
using {solutionName}.Core.Features.{entityName};

namespace {projectNamespace}.Controllers;

[ApiController]
[Route(""api/[controller]"")]
public class {entityName}Controller : ControllerBase
{{
    private readonly IMediator _mediator;

    public {entityName}Controller(IMediator mediator)
    {{
        _mediator = mediator;
    }}

    [HttpGet]
    [ProducesResponseType(typeof(Get{pluralName}Response), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {{
        var request = new Get{pluralName}Request
        {{
            PageIndex = pageIndex,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDescending = sortDescending
        }};

        var response = await _mediator.Send(request);
        return Ok(response);
    }}

    [HttpGet(""{{id}}"")]
    [ProducesResponseType(typeof(Get{entityName}ByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {{
        var request = new Get{entityName}ByIdRequest {{ {entityName}Id = id }};
        var response = await _mediator.Send(request);
        return Ok(response);
    }}

    [HttpPost]
    [ProducesResponseType(typeof(Create{entityName}Response), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Create{entityName}Request request)
    {{
        var response = await _mediator.Send(request);
        return CreatedAtAction(
            nameof(GetById),
            new {{ id = response.{entityName}.{entityName}Id }},
            response);
    }}

    [HttpPut(""{{id}}"")]
    [ProducesResponseType(typeof(Update{entityName}Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(string id, [FromBody] Update{entityName}Request request)
    {{
        request.{entityName}Id = id;
        var response = await _mediator.Send(request);
        return Ok(response);
    }}

    [HttpDelete(""{{id}}"")]
    [ProducesResponseType(typeof(Delete{entityName}Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {{
        var request = new Delete{entityName}Request {{ {entityName}Id = id }};
        var response = await _mediator.Send(request);
        return Ok(response);
    }}
}}
";
    }

    private void GenerateProgramFile(ProjectModel project, string solutionName)
    {
        var content = GenerateProgramContent(solutionName, project.Namespace);
        project.Files.Add(new Files.ContentFileModel(content, "Program", project.Directory, ".cs"));
    }

    private string GenerateProgramContent(string solutionName, string projectNamespace)
    {
        return $@"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using {solutionName}.Core.Data;
using {solutionName}.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddCodeGeneratorsApiExplorer();
builder.Services.AddSwaggerGen();

// MediatR - register from Core project
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<I{solutionName}Context>());

// FluentValidation - register from Core project
builder.Services.AddValidatorsFromAssemblyContaining<I{solutionName}Context>();

// Database
builder.Services.AddDbContext<{solutionName}DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString(""DefaultConnection"")));

builder.Services.AddScoped<I{solutionName}Context>(provider => provider.GetRequiredService<{solutionName}DbContext>());

// CORS
builder.Services.AddCors(options =>
{{
    options.AddPolicy(""AllowSpecificOrigins"", policy =>
    {{
        var origins = builder.Configuration[""WithOrigins""]?.Split(',') ?? new[] {{ ""http://localhost:4200"" }};
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    }});
}});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{{
    app.UseSwagger();
    app.UseSwaggerUI();
}}

app.UseHttpsRedirection();
app.UseCors(""AllowSpecificOrigins"");
app.UseAuthorization();
app.MapControllers();

app.Run();
";
    }

    private void GenerateAppSettingsFile(ProjectModel project, string solutionName)
    {
        var databaseName = $"{solutionName}Db";
        var content = $@"{{
  ""Logging"": {{
    ""LogLevel"": {{
      ""Default"": ""Information"",
      ""Microsoft.AspNetCore"": ""Warning""
    }}
  }},
  ""AllowedHosts"": ""*"",
  ""ConnectionStrings"": {{
    ""DefaultConnection"": ""Server=.\\SQLEXPRESS;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true""
  }},
  ""WithOrigins"": ""http://localhost:4200""
}}";
        project.Files.Add(new Files.ContentFileModel(content, "appsettings", project.Directory, ".json"));
    }

    private static AccessModifier ConvertVisibility(PlantUmlVisibility visibility)
    {
        return visibility switch
        {
            PlantUmlVisibility.Public => AccessModifier.Public,
            PlantUmlVisibility.Private => AccessModifier.Private,
            PlantUmlVisibility.Protected => AccessModifier.Protected,
            PlantUmlVisibility.Package => AccessModifier.Internal,
            _ => AccessModifier.Public
        };
    }

    private static string MapTypeName(string plantUmlType)
    {
        return plantUmlType?.ToLower() switch
        {
            "datetime" => "DateTime",
            "datetimeoffset" => "DateTimeOffset",
            "guid" => "Guid",
            "int" => "int",
            "long" => "long",
            "decimal" => "decimal",
            "double" => "double",
            "bool" => "bool",
            "string" => "string",
            _ => plantUmlType
        };
    }

    private static bool IsAuditProperty(string propertyName)
    {
        var auditProperties = new[] { "CreatedAt", "ModifiedAt", "CreatedBy", "ModifiedBy" };
        return auditProperties.Any(p => p.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetPluralName(string name)
    {
        if (name.EndsWith("y"))
        {
            return name[..^1] + "ies";
        }

        if (name.EndsWith("s") || name.EndsWith("x") || name.EndsWith("ch") || name.EndsWith("sh"))
        {
            return name + "es";
        }

        return name + "s";
    }
}
