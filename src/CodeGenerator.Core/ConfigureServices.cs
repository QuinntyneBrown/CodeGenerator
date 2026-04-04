// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Reflection;
using CodeGenerator.Core.Artifacts.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CodeGenerator.Core.Configuration;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Dependencies;
using CodeGenerator.Core.Incremental.Services;
using CodeGenerator.Core.Schema;
using CodeGenerator.Core.Services;
using CodeGenerator.Core.Validation;
using CodeGenerator.Core.Syntax;
using CodeGenerator.Core.Templates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core;

public static class ConfigureServices
{
    public static void AddCoreServices(this IServiceCollection services, Assembly assembly)
    {
        services.AddScoped<IGenerationContext, TemplateGenerationContext>();
        services.TryAddSingleton<ICodeGeneratorConfiguration>(
            new CodeGeneratorConfiguration(
                defaults: new Dictionary<string, string>(),
                fileConfig: new Dictionary<string, string>(),
                envConfig: new Dictionary<string, string>(),
                cliConfig: new Dictionary<string, string>()));
        services.AddSingleton<ITemplateSetInfoLoader, TemplateSetInfoLoader>();
        services.AddSingleton<NamingFilterParser>();
        services.AddSingleton<IConventionTemplateDiscovery, ConventionTemplateDiscovery>();
        services.AddSingleton<IStyleRegistry, StyleRegistry>();
        services.AddSingleton<StyleResolver>();
        services.AddSingleton<IFilenamePlaceholderResolver, FilenamePlaceholderResolver>();
        services.AddSingleton<TemplatePartitioner>();
        services.AddSingleton<ISchemaFormatDetector, SchemaFormatDetector>();
        services.AddSingleton<SchemaNormalizerDispatcher>();
        services.AddSingleton<ISchemaNormalizer, JsonSchemaNormalizer>();
        services.AddSingleton<ISchemaRegistry, SchemaRegistry>();
        services.AddSingleton<IInputValidator, JsonSchemaInputValidator>();
        services.AddScoped<IGenerationRollbackService, Errors.GenerationRollbackService>();
        services.AddSingleton<IDependencyResolver, DependencyResolver>();
        services.AddSingleton<SharedTemplateFileSystem>(sp =>
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name?.StartsWith("CodeGenerator") == true)
                .ToList();
            return new SharedTemplateFileSystem(assemblies);
        });
        services.AddSingleton<IUserInputService, UserInputService>();
        services.AddSingleton<IProjectContextFactory, ProjectContextFactory>();
        services.AddSingleton<IConflictResolver, DefaultConflictResolver>();
        services.AddSingleton<Artifacts.StrategyExecutor>();
        AddArifactGenerator(services, assembly);
        AddSyntaxGenerator(services, assembly);
    }

    public static void AddArifactGenerator(this IServiceCollection services, Assembly assembly)
    {
        var @interface = typeof(IArtifactGenerationStrategy<>);

        var implementations = SafeGetTypes(assembly)
            .Where(type =>
                !type.IsAbstract &&
                type.GetInterfaces().Any(interfaceType =>
                    interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == @interface))
            .ToList();

        foreach (var implementation in implementations)
        {
            foreach (var implementedInterface in implementation.GetInterfaces())
            {
                if (implementedInterface.IsGenericType && implementedInterface.GetGenericTypeDefinition() == @interface)
                {
                    services.AddSingleton(implementedInterface, implementation);
                }
            }
        }

        services.AddSingleton<IObjectCache, ObjectCache>();
        services.AddSingleton<IArtifactGenerator, ArtifactGenerator>();
    }

    public static void AddSyntaxGenerator(this IServiceCollection services, Assembly assembly)
    {
        var @interface = typeof(ISyntaxGenerationStrategy<>);

        var implementations = SafeGetTypes(assembly)
            .Where(type =>
                !type.IsAbstract &&
                type.GetInterfaces().Any(interfaceType =>
                    interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == @interface))
            .ToList();

        foreach (var implementation in implementations)
        {
            foreach (var implementedInterface in implementation.GetInterfaces())
            {
                if (implementedInterface.IsGenericType && implementedInterface.GetGenericTypeDefinition() == @interface)
                {
                    services.AddSingleton(implementedInterface, implementation);
                }
            }
        }

        services.AddSingleton<ISyntaxGenerator, SyntaxGenerator>();
    }

    internal static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }
}
