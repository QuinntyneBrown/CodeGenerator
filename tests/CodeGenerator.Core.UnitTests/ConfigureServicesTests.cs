// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.UnitTests;

public class ConfigureServicesTests
{
    [Fact]
    public void AddCoreServices_RegistersSyntaxGenerator()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole());
        services.AddCoreServices(typeof(DockerComposeSyntaxGenerationStrategy).Assembly);
        var provider = services.BuildServiceProvider();

        var generator = provider.GetService<ISyntaxGenerator>();
        Assert.NotNull(generator);
    }

    [Fact]
    public void AddCoreServices_RegistersArtifactGenerator()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole());
        services.AddCoreServices(typeof(DockerComposeSyntaxGenerationStrategy).Assembly);
        var provider = services.BuildServiceProvider();

        var generator = provider.GetService<IArtifactGenerator>();
        Assert.NotNull(generator);
    }

    [Fact]
    public void AddCoreServices_RegistersObjectCache()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole());
        services.AddCoreServices(typeof(DockerComposeSyntaxGenerationStrategy).Assembly);
        var provider = services.BuildServiceProvider();

        var cache = provider.GetService<IObjectCache>();
        Assert.NotNull(cache);
    }

    [Fact]
    public void AddCoreServices_AutoDiscoversStrategiesFromAssembly()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole());
        services.AddCoreServices(typeof(DockerComposeSyntaxGenerationStrategy).Assembly);
        var provider = services.BuildServiceProvider();

        var strategies = provider.GetService<IEnumerable<ISyntaxGenerationStrategy<DockerComposeModel>>>();
        Assert.NotNull(strategies);
        Assert.NotEmpty(strategies);
    }
}
