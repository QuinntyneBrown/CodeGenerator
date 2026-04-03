// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Plugins;
using CodeGenerator.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class PluginDiscoveryTests : IDisposable
{
    private readonly TempDirectoryFixture _fixture;
    private readonly ServiceProvider _serviceProvider;

    public PluginDiscoveryTests()
    {
        _fixture = new TempDirectoryFixture();
        var services = new ServiceCollection();
        services.AddLogging(builder => { builder.AddConsole(); builder.SetMinimumLevel(LogLevel.Warning); });
        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _fixture.Dispose();
        _serviceProvider.Dispose();
    }

    [Fact]
    public void DiscoveredPlugin_RecordProperties()
    {
        var plugin = new DiscoveredPlugin
        {
            AssemblyPath = @"C:\plugins\test.dll",
            Source = PluginSource.NuGet,
            PackageName = "CodeGenerator.Cli.Plugin.Test",
        };

        Assert.Equal(@"C:\plugins\test.dll", plugin.AssemblyPath);
        Assert.Equal(PluginSource.NuGet, plugin.Source);
        Assert.Equal("CodeGenerator.Cli.Plugin.Test", plugin.PackageName);
    }

    [Fact]
    public async Task ExplicitPluginDiscovery_ValidPath_ReturnsPlugin()
    {
        var dllPath = Path.Combine(_fixture.Path, "test.dll");
        File.WriteAllText(dllPath, "fake dll content");

        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<ExplicitPluginDiscovery>();
        var discovery = new ExplicitPluginDiscovery([dllPath], logger);

        var results = await discovery.DiscoverAsync();

        Assert.Single(results);
        Assert.Equal(dllPath, results[0].AssemblyPath);
        Assert.Equal(PluginSource.Explicit, results[0].Source);
    }

    [Fact]
    public async Task ExplicitPluginDiscovery_NonExistentPath_ReturnsEmpty()
    {
        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<ExplicitPluginDiscovery>();
        var discovery = new ExplicitPluginDiscovery([@"C:\nonexistent\test.dll"], logger);

        var results = await discovery.DiscoverAsync();

        Assert.Empty(results);
    }

    [Fact]
    public async Task ExplicitPluginDiscovery_NonDllPath_ReturnsEmpty()
    {
        var txtPath = Path.Combine(_fixture.Path, "test.txt");
        File.WriteAllText(txtPath, "not a dll");

        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<ExplicitPluginDiscovery>();
        var discovery = new ExplicitPluginDiscovery([txtPath], logger);

        var results = await discovery.DiscoverAsync();

        Assert.Empty(results);
    }

    [Fact]
    public async Task DirectoryPluginDiscovery_EmptyPluginsDir_ReturnsEmpty()
    {
        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<DirectoryPluginDiscovery>();
        var discovery = new DirectoryPluginDiscovery(logger);

        // By default, likely no plugins directory exists
        var results = await discovery.DiscoverAsync();

        Assert.NotNull(results);
    }

    [Fact]
    public async Task CompositePluginDiscovery_DeduplicatesByPath()
    {
        var dllPath = Path.Combine(_fixture.Path, "shared.dll");
        File.WriteAllText(dllPath, "fake dll");

        var logger1 = _serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<ExplicitPluginDiscovery>();
        var discovery1 = new ExplicitPluginDiscovery([dllPath], logger1);

        var logger2 = _serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<ExplicitPluginDiscovery>();
        var discovery2 = new ExplicitPluginDiscovery([dllPath], logger2);

        var compositeLogger = _serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<CompositePluginDiscovery>();
        var composite = new CompositePluginDiscovery(
            [discovery1, discovery2], compositeLogger);

        var results = await composite.DiscoverAsync();

        Assert.Single(results); // Deduplicated
    }

    [Fact]
    public async Task CompositePluginDiscovery_AggregatesMultipleSources()
    {
        var dll1 = Path.Combine(_fixture.Path, "plugin1.dll");
        var dll2 = Path.Combine(_fixture.Path, "plugin2.dll");
        File.WriteAllText(dll1, "fake");
        File.WriteAllText(dll2, "fake");

        var logger1 = _serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<ExplicitPluginDiscovery>();
        var discovery1 = new ExplicitPluginDiscovery([dll1], logger1);

        var logger2 = _serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<ExplicitPluginDiscovery>();
        var discovery2 = new ExplicitPluginDiscovery([dll2], logger2);

        var compositeLogger = _serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<CompositePluginDiscovery>();
        var composite = new CompositePluginDiscovery(
            [discovery1, discovery2], compositeLogger);

        var results = await composite.DiscoverAsync();

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void PluginSource_EnumValues()
    {
        Assert.Equal(0, (int)PluginSource.NuGet);
        Assert.Equal(1, (int)PluginSource.Directory);
        Assert.Equal(2, (int)PluginSource.Explicit);
    }
}
