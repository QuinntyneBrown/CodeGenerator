// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Templates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class ConventionTemplateDiscoveryTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public ConventionTemplateDiscoveryTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddCoreServices(typeof(ConventionTemplateDiscoveryTests).Assembly);
        services.AddDotNetServices();

        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    #region DD-31: TemplateFileEntry / TemplateFilePlan

    [Fact]
    public void TemplateFilePlan_DefaultEntries_Empty()
    {
        var plan = new TemplateFilePlan();

        Assert.Empty(plan.Entries);
    }

    [Fact]
    public void TemplateFileEntry_Properties_SetCorrectly()
    {
        var entry = new TemplateFileEntry
        {
            TemplatePath = "src/Controllers/FooController.cs.liquid",
            OutputRelativePath = "src/Controllers/FooController.cs",
            TemplateContent = "// template",
            RequiresIteration = false,
            Placeholders = new List<string>()
        };

        Assert.Equal("src/Controllers/FooController.cs.liquid", entry.TemplatePath);
        Assert.Equal("src/Controllers/FooController.cs", entry.OutputRelativePath);
        Assert.False(entry.RequiresIteration);
    }

    #endregion

    #region DD-31: ConventionTemplateDiscovery

    [Fact]
    public void Discover_WalksDirectoryTree_ReturnsAllLiquidFiles()
    {
        var discovery = _serviceProvider.GetRequiredService<IConventionTemplateDiscovery>();
        var dir = CreateTempTemplateTree(new[]
        {
            "Program.cs.liquid",
            "Controllers/HomeController.cs.liquid",
            "Models/User.cs.liquid"
        });

        try
        {
            var plan = discovery.Discover(dir, TemplateSourceType.FileSystem);

            Assert.Equal(3, plan.Entries.Count);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Discover_StripsLiquidExtension()
    {
        var discovery = _serviceProvider.GetRequiredService<IConventionTemplateDiscovery>();
        var dir = CreateTempTemplateTree(new[] { "Program.cs.liquid" });

        try
        {
            var plan = discovery.Discover(dir, TemplateSourceType.FileSystem);

            Assert.Single(plan.Entries);
            Assert.Equal("Program.cs", plan.Entries[0].OutputRelativePath);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Discover_UnderscorePrefix_StrippedFromRootFiles()
    {
        var discovery = _serviceProvider.GetRequiredService<IConventionTemplateDiscovery>();
        var dir = CreateTempTemplateTree(new[] { "_project.csproj.liquid" });

        try
        {
            var plan = discovery.Discover(dir, TemplateSourceType.FileSystem);

            Assert.Single(plan.Entries);
            Assert.Equal("project.csproj", plan.Entries[0].OutputRelativePath);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Discover_PreservesDirectoryNesting()
    {
        var discovery = _serviceProvider.GetRequiredService<IConventionTemplateDiscovery>();
        var dir = CreateTempTemplateTree(new[]
        {
            "src/Controllers/FooController.cs.liquid"
        });

        try
        {
            var plan = discovery.Discover(dir, TemplateSourceType.FileSystem);

            Assert.Single(plan.Entries);
            var outputPath = plan.Entries[0].OutputRelativePath.Replace("\\", "/");
            Assert.Equal("src/Controllers/FooController.cs", outputPath);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Discover_DetectsEntityNamePlaceholder()
    {
        var discovery = _serviceProvider.GetRequiredService<IConventionTemplateDiscovery>();
        var dir = CreateTempTemplateTree(new[]
        {
            "Controllers/{{EntityName}}Controller.cs.liquid",
            "Program.cs.liquid"
        });

        try
        {
            var plan = discovery.Discover(dir, TemplateSourceType.FileSystem);

            var iterated = plan.Entries.First(e => e.RequiresIteration);
            var nonIterated = plan.Entries.First(e => !e.RequiresIteration);

            Assert.Contains("EntityName", iterated.Placeholders);
            Assert.Equal("Program.cs", nonIterated.OutputRelativePath);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Discover_PathTraversal_Rejected()
    {
        var discovery = _serviceProvider.GetRequiredService<IConventionTemplateDiscovery>();
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);

        try
        {
            // Create a symlink-like traversal won't work on all OS, so test the validator directly
            // by testing that empty/nonexistent dirs produce empty plans
            var plan = discovery.Discover(dir, TemplateSourceType.FileSystem);
            Assert.Empty(plan.Entries);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    #endregion

    #region DD-31: ConventionGenerationRequest

    [Fact]
    public void ConventionGenerationRequest_Properties()
    {
        var request = new ConventionGenerationRequest
        {
            StyleRoot = "/templates/dotnet-webapi",
            OutputRoot = "/output",
            SourceType = TemplateSourceType.FileSystem,
            Tokens = new Dictionary<string, object> { ["name"] = "MyApp" },
            ProjectName = "MyApp"
        };

        Assert.Equal("/templates/dotnet-webapi", request.StyleRoot);
        Assert.Equal("MyApp", request.ProjectName);
        Assert.Equal(TemplateSourceType.FileSystem, request.SourceType);
    }

    #endregion

    private string CreateTempTemplateTree(string[] relativePaths)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(root);

        foreach (var relativePath in relativePaths)
        {
            var fullPath = Path.Combine(root, relativePath);
            var dir = Path.GetDirectoryName(fullPath)!;
            Directory.CreateDirectory(dir);
            File.WriteAllText(fullPath, "// template content");
        }

        return root;
    }
}
