// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.DotNet;
using CodeGenerator.DotNet.Artifacts;
using CodeGenerator.DotNet.Artifacts.Files.Factories;
using CodeGenerator.DotNet.Artifacts.Files.Services;
using CodeGenerator.DotNet.Artifacts.FullStack;
using CodeGenerator.DotNet.Artifacts.Git;
using CodeGenerator.DotNet.Artifacts.PlantUml.Services;
using CodeGenerator.DotNet.Artifacts.Projects.Factories;
using CodeGenerator.DotNet.Artifacts.Projects.Services;
using CodeGenerator.DotNet.Artifacts.Services;
using CodeGenerator.DotNet.Artifacts.Solutions.Factories;
using CodeGenerator.DotNet.Artifacts.Solutions.Services;
using CodeGenerator.DotNet.Artifacts.SpecFlow;
using CodeGenerator.DotNet.Artifacts.Units;
using CodeGenerator.DotNet.Events;
using CodeGenerator.DotNet.Services;
using CodeGenerator.DotNet.Syntax.Classes.Factories;
using CodeGenerator.DotNet.Syntax.Classes.Services;
using CodeGenerator.DotNet.Syntax.Controllers;
using CodeGenerator.DotNet.Syntax.Entities;
using CodeGenerator.DotNet.Syntax.Expressions;
using CodeGenerator.DotNet.Syntax.Methods.Factories;
using CodeGenerator.DotNet.Syntax.Namespaces.Factories;
using CodeGenerator.DotNet.Syntax.Properties.Factories;
using CodeGenerator.DotNet.Syntax.RouteHandlers;
using CodeGenerator.DotNet.Syntax.Types;
using CodeGenerator.DotNet.Syntax.Units.Factories;
using CodeGenerator.DotNet.Syntax.Units.Services;
using CodeGenerator.Core.Internal;
using MediatR;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static void AddDotNetServices(this IServiceCollection services)
    {
        services.AddSingleton(new Observable<INotification>());
        services.AddSingleton<ICodeGeneratorEventContainer, CodeGeneratorEventContainer>();
        services.AddSingleton<IFullStackFactory, FullStackFactory>();
        services.AddSingleton<IDocumentFactory, DocumentFactory>();
        services.AddSingleton<IGitService, GitService>();
        services.AddSingleton<ICodeFormatterService, DotnetCodeFormatterService>();
        services.AddSingleton<ICodeAnalysisService, CodeAnalysisService>();
        services.AddSingleton<ISyntaxUnitFactory, SyntaxUnitFactory>();
        services.AddSingleton<INamespaceFactory, NamespaceFactory>();
        services.AddSingleton<IPropertyFactory, PropertyFactory>();
        services.AddSingleton<ICqrsFactory, CqrsFactory>();
        services.AddSingleton<ISyntaxFactory, SyntaxFactory>();
        services.AddSingleton<IExpressionFactory, ExpressionFactory>();
        services.AddSingleton<ITypeFactory, TypeFactory>();
        services.AddSingleton<IPlaywrightService, PlaywrightService>();
        services.AddSingleton<IMethodFactory, MethodFactory>();
        services.AddSingleton<ISpecFlowService, SpecFlowService>();
        services.AddSingleton<IDomainDrivenDesignService, DomainDrivenDesignService>();
        services.AddSingleton<IClassService, ClassService>();
        services.AddSingleton<IUtilityService, UtlitityService>();
        services.AddSingleton<ISignalRService, SignalRService>();
        services.AddSingleton<IReactService, ReactService>();
        services.AddSingleton<ICoreProjectService, CoreProjectService>();
        services.AddSingleton<ILitService, LitService>();
        services.AddSingleton<IInfrastructureProjectService, InfrastructureProjectService>();
        services.AddSingleton<IApiProjectService, ApiProjectService>();
        services.AddSingleton<IAggregateService, AggregateService>();
        services.AddSingleton<ICommandService, CommandService>();
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<ITemplateProcessor, LiquidTemplateProcessor>();
        services.AddSingleton<INamingConventionConverter, NamingConventionConverter>();
        services.AddSingleton<ISettingsProvider, SettingsProvider>();
        services.AddSingleton<ITenseConverter, TenseConverter>();
        services.AddSingleton<IContext, Context>();
        services.AddSingleton<INamespaceProvider, NamespaceProvider>();
        services.AddSingleton<IDomainDrivenDesignFileService, DomainDrivenDesignFileService>();
        services.AddSingleton<IDependencyInjectionService, DependencyInjectionService>();
        services.AddSingleton<IEntityFactory, EntityFactory>();
        services.AddSingleton<IFileNamespaceProvider, FileNamespaceProvider>();
        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<ISolutionService, SolutionService>();
        services.AddSingleton<IFileProvider, FileProvider>();
        services.AddSingleton<ISolutionNamespaceProvider, SolutionNamespaceProvider>();
        services.AddSingleton<ISolutionFactory, SolutionFactory>();
        services.AddSingleton<IControllerFactory, ControllerFactory>();
        services.AddSingleton<IMinimalApiService, MinimalApiService>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<IEntityFileFactory, EntityFileFactory>();
        services.AddSingleton<IProjectFactory, ProjectFactory>();
        services.AddSingleton<IRouteHandlerFactory, RouteHandlerFactory>();
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(CodeGenerator.DotNet.Constants).Assembly));
        services.AddSingleton<IClassFactory, ClassFactory>();
        services.AddSingleton<IArtifactParser, ArtifactParser>();
        services.AddSingleton<ISyntaxParser, SyntaxParser>();
        services.AddSingleton<CodeGenerator.DotNet.Artifacts.Files.Factories.IFileFactory, CodeGenerator.DotNet.Artifacts.Files.Factories.FileFactory>();

        // PlantUML services
        services.AddSingleton<IPlantUmlParserService, PlantUmlParserService>();
        services.AddSingleton<IPlantUmlSolutionModelFactory, PlantUmlSolutionModelFactory>();
        services.AddSingleton<IPlantUmlValidationService, PlantUmlValidationService>();
        services.AddSingleton<ISequenceToSolutionPlantUmlService, SequenceToSolutionPlantUmlService>();

        services.AddSingleton<ITemplateLocator, EmbeddedResourceTemplateLocatorBase<AssemblyMarker>>();

        services.AddSyntaxGenerator(typeof(CodeGenerator.DotNet.Constants).Assembly);
        services.AddArifactGenerator(typeof(CodeGenerator.DotNet.Constants).Assembly);
    }
}