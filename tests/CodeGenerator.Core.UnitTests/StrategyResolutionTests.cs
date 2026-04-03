// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.UnitTests;

public class TestSyntaxModel : SyntaxModel
{
    public string Name { get; set; } = string.Empty;
}

public class TestSyntaxModelStrategy : ISyntaxGenerationStrategy<TestSyntaxModel>
{
    public int GetPriority() => 1;

    public Task<string> GenerateAsync(TestSyntaxModel target, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Generated: {target.Name}");
    }
}

public class HighPriorityTestSyntaxModelStrategy : ISyntaxGenerationStrategy<TestSyntaxModel>
{
    public int GetPriority() => 10;

    public Task<string> GenerateAsync(TestSyntaxModel target, CancellationToken cancellationToken)
    {
        return Task.FromResult($"HighPriority: {target.Name}");
    }
}

public class TestArtifactModel
{
    public string Name { get; set; } = string.Empty;
}

public class TestArtifactStrategy : IArtifactGenerationStrategy<TestArtifactModel>
{
    public int GetPriority() => 1;

    public Task GenerateAsync(TestArtifactModel target)
    {
        return Task.CompletedTask;
    }
}

public class StrategyResolutionTests
{
    [Fact]
    public async Task SyntaxGenerator_ResolvesStrategy_ViaCanHandleAndPriority()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole());
        services.AddSingleton<ISyntaxGenerationStrategy<TestSyntaxModel>, TestSyntaxModelStrategy>();
        services.AddSingleton<ISyntaxGenerator, SyntaxGenerator>();
        var provider = services.BuildServiceProvider();

        var generator = provider.GetRequiredService<ISyntaxGenerator>();
        var model = new TestSyntaxModel { Name = "Foo" };

        var result = await generator.GenerateAsync(model);

        Assert.Equal("Generated: Foo", result);
    }

    [Fact]
    public async Task SyntaxGenerator_SelectsHighestPriority_WhenMultipleStrategiesMatch()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole());
        services.AddSingleton<ISyntaxGenerationStrategy<TestSyntaxModel>, TestSyntaxModelStrategy>();
        services.AddSingleton<ISyntaxGenerationStrategy<TestSyntaxModel>, HighPriorityTestSyntaxModelStrategy>();
        services.AddSingleton<ISyntaxGenerator, SyntaxGenerator>();
        var provider = services.BuildServiceProvider();

        var generator = provider.GetRequiredService<ISyntaxGenerator>();
        var model = new TestSyntaxModel { Name = "Bar" };

        var result = await generator.GenerateAsync(model);

        Assert.Equal("HighPriority: Bar", result);
    }

    [Fact]
    public async Task ArtifactGenerator_ResolvesStrategy_ViaCanHandle()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole());
        services.AddSingleton<IArtifactGenerationStrategy<TestArtifactModel>, TestArtifactStrategy>();
        services.AddSingleton<IArtifactGenerator, ArtifactGenerator>();
        var provider = services.BuildServiceProvider();

        var generator = provider.GetRequiredService<IArtifactGenerator>();
        var model = new TestArtifactModel { Name = "Test" };

        // Should not throw — strategy is resolved via CanHandle
        await generator.GenerateAsync(model);
    }
}
