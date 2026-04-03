// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Templates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class LanguageStyleTemplateMatrixTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public LanguageStyleTemplateMatrixTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddCoreServices(typeof(LanguageStyleTemplateMatrixTests).Assembly);
        services.AddDotNetServices();

        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    #region DD-32: StyleDefinition

    [Fact]
    public void StyleDefinition_Properties()
    {
        var style = new StyleDefinition
        {
            Name = "clean-architecture",
            Language = "dotnet",
            Description = "4-layer DDD style",
            TemplateRoot = "/templates/dotnet/clean-architecture",
            CommonRoot = "/templates/dotnet/_common",
            Priority = 10,
            IsDefault = true,
            SourceType = TemplateSourceType.FileSystem
        };

        Assert.Equal("clean-architecture", style.Name);
        Assert.Equal("dotnet", style.Language);
        Assert.Equal(10, style.Priority);
        Assert.True(style.IsDefault);
    }

    #endregion

    #region DD-32: StyleRegistry

    [Fact]
    public void StyleRegistry_Register_And_GetStyle()
    {
        var registry = _serviceProvider.GetRequiredService<IStyleRegistry>();

        registry.Register(new StyleDefinition
        {
            Name = "test-style",
            Language = "test-lang",
            TemplateRoot = "/tmp/test"
        });

        var style = registry.GetStyle("test-lang", "test-style");
        Assert.Equal("test-style", style.Name);
    }

    [Fact]
    public void StyleRegistry_GetStyles_ReturnsAllForLanguage()
    {
        var registry = _serviceProvider.GetRequiredService<IStyleRegistry>();

        registry.Register(new StyleDefinition { Name = "style-a", Language = "lang1", TemplateRoot = "/a" });
        registry.Register(new StyleDefinition { Name = "style-b", Language = "lang1", TemplateRoot = "/b" });
        registry.Register(new StyleDefinition { Name = "style-c", Language = "lang2", TemplateRoot = "/c" });

        var styles = registry.GetStyles("lang1");
        Assert.Equal(2, styles.Count);
    }

    [Fact]
    public void StyleRegistry_GetLanguages_ReturnsDistinct()
    {
        var registry = _serviceProvider.GetRequiredService<IStyleRegistry>();

        registry.Register(new StyleDefinition { Name = "a", Language = "dotnet", TemplateRoot = "/a" });
        registry.Register(new StyleDefinition { Name = "b", Language = "react", TemplateRoot = "/b" });

        var languages = registry.GetLanguages();
        Assert.Contains("dotnet", languages);
        Assert.Contains("react", languages);
    }

    [Fact]
    public void StyleRegistry_GetStyle_UnknownStyle_Throws()
    {
        var registry = _serviceProvider.GetRequiredService<IStyleRegistry>();

        Assert.Throws<KeyNotFoundException>(() =>
            registry.GetStyle("dotnet", "nonexistent-style"));
    }

    [Fact]
    public void StyleRegistry_DiscoverStyles_FromDirectory()
    {
        var registry = _serviceProvider.GetRequiredService<IStyleRegistry>();
        var root = CreateStyleTree();

        try
        {
            registry.DiscoverStyles(root);

            var styles = registry.GetStyles("testlang");
            Assert.Contains(styles, s => s.Name == "style-one");
            Assert.DoesNotContain(styles, s => s.Name == "_common");
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    #endregion

    #region DD-32: StyleResolver

    [Fact]
    public void StyleResolver_MergesCommonAndStyle()
    {
        var registry = _serviceProvider.GetRequiredService<IStyleRegistry>();
        var resolver = _serviceProvider.GetRequiredService<StyleResolver>();

        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var commonDir = Path.Combine(root, "testlang2", "_common");
        var styleDir = Path.Combine(root, "testlang2", "my-style");

        Directory.CreateDirectory(commonDir);
        Directory.CreateDirectory(styleDir);
        File.WriteAllText(Path.Combine(commonDir, "shared.cs.liquid"), "// common");
        File.WriteAllText(Path.Combine(styleDir, "specific.cs.liquid"), "// style");

        try
        {
            registry.Register(new StyleDefinition
            {
                Name = "my-style",
                Language = "testlang2",
                TemplateRoot = styleDir,
                CommonRoot = commonDir,
                SourceType = TemplateSourceType.FileSystem
            });

            var plan = resolver.ResolveTemplates("testlang2", "my-style");

            Assert.Equal(2, plan.Entries.Count);
            Assert.Contains(plan.Entries, e => e.OutputRelativePath == "shared.cs");
            Assert.Contains(plan.Entries, e => e.OutputRelativePath == "specific.cs");
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void StyleResolver_StyleOverridesCommon()
    {
        var registry = _serviceProvider.GetRequiredService<IStyleRegistry>();
        var resolver = _serviceProvider.GetRequiredService<StyleResolver>();

        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var commonDir = Path.Combine(root, "testlang3", "_common");
        var styleDir = Path.Combine(root, "testlang3", "override-style");

        Directory.CreateDirectory(commonDir);
        Directory.CreateDirectory(styleDir);
        File.WriteAllText(Path.Combine(commonDir, "file.cs.liquid"), "// common version");
        File.WriteAllText(Path.Combine(styleDir, "file.cs.liquid"), "// style version");

        try
        {
            registry.Register(new StyleDefinition
            {
                Name = "override-style",
                Language = "testlang3",
                TemplateRoot = styleDir,
                CommonRoot = commonDir,
                SourceType = TemplateSourceType.FileSystem
            });

            var plan = resolver.ResolveTemplates("testlang3", "override-style");

            Assert.Single(plan.Entries);
            Assert.Equal("// style version", plan.Entries[0].TemplateContent);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    #endregion

    private string CreateStyleTree()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var commonDir = Path.Combine(root, "testlang", "_common");
        var styleDir = Path.Combine(root, "testlang", "style-one");

        Directory.CreateDirectory(commonDir);
        Directory.CreateDirectory(styleDir);
        File.WriteAllText(Path.Combine(commonDir, "shared.liquid"), "// common");
        File.WriteAllText(Path.Combine(styleDir, "main.liquid"), "// style");

        return root;
    }
}
