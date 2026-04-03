// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Scaffold.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureScaffoldServices
{
    public static void AddScaffoldingServices(this IServiceCollection services)
    {
        services.AddSingleton<IYamlConfigParser, YamlConfigParser>();
        services.AddSingleton<IConfigValidator, ConfigValidator>();
        services.AddSingleton<ITypeMapper, TypeMapper>();
        services.AddSingleton<ISchemaExporter, SchemaExporter>();
        services.AddSingleton<IPostScaffoldExecutor, PostScaffoldExecutor>();
        services.AddSingleton<IArchitectureResolver, ArchitectureResolver>();
        services.AddSingleton<IEntityGenerator, EntityGenerator>();
        services.AddSingleton<IDtoGenerator, DtoGenerator>();
        services.AddSingleton<ITestProjectScaffolder, TestProjectScaffolder>();
        services.AddSingleton<IProjectScaffolder, ProjectScaffolder>();
        services.AddSingleton<ISolutionScaffolder, SolutionScaffolder>();
        services.AddSingleton<ICrossProjectReferenceResolver, CrossProjectReferenceResolver>();
        services.AddSingleton<IScaffoldOrchestrator, ScaffoldOrchestrator>();
        services.AddSingleton<IScaffoldEngine, ScaffoldEngine>();
    }
}
