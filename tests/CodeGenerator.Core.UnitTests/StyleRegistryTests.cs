// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Templates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CodeGenerator.Core.UnitTests;

public class StyleRegistryTests
{
    private readonly StyleRegistry _registry;

    public StyleRegistryTests()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<StyleRegistry>();
        _registry = new StyleRegistry(logger);
    }

    // ── Register / GetStyle ──

    [Fact]
    public void Register_AndGetStyle_ReturnsRegistered()
    {
        var style = new StyleDefinition
        {
            Name = "clean",
            Language = "csharp"
        };

        _registry.Register(style);
        var result = _registry.GetStyle("csharp", "clean");

        Assert.Same(style, result);
    }

    [Fact]
    public void Register_SameNameLanguage_OverwritesPrevious()
    {
        var style1 = new StyleDefinition { Name = "clean", Language = "csharp", Priority = 1 };
        var style2 = new StyleDefinition { Name = "clean", Language = "csharp", Priority = 2 };

        _registry.Register(style1);
        _registry.Register(style2);

        var result = _registry.GetStyle("csharp", "clean");
        Assert.Equal(2, result.Priority);
    }

    [Fact]
    public void GetStyle_NotFound_ThrowsKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(
            () => _registry.GetStyle("csharp", "nonexistent"));
    }

    [Fact]
    public void GetStyle_WrongLanguage_ThrowsKeyNotFoundException()
    {
        _registry.Register(new StyleDefinition { Name = "clean", Language = "csharp" });

        Assert.Throws<KeyNotFoundException>(
            () => _registry.GetStyle("python", "clean"));
    }

    // ── GetStyles ──

    [Fact]
    public void GetStyles_ReturnsAllForLanguage()
    {
        _registry.Register(new StyleDefinition { Name = "clean", Language = "csharp" });
        _registry.Register(new StyleDefinition { Name = "minimal", Language = "csharp" });
        _registry.Register(new StyleDefinition { Name = "django", Language = "python" });

        var csharpStyles = _registry.GetStyles("csharp");
        Assert.Equal(2, csharpStyles.Count);
    }

    [Fact]
    public void GetStyles_NoStyles_ReturnsEmpty()
    {
        var styles = _registry.GetStyles("unknown");
        Assert.Empty(styles);
    }

    // ── GetLanguages ──

    [Fact]
    public void GetLanguages_ReturnsAllRegisteredLanguages()
    {
        _registry.Register(new StyleDefinition { Name = "s1", Language = "csharp" });
        _registry.Register(new StyleDefinition { Name = "s2", Language = "python" });

        var languages = _registry.GetLanguages();
        Assert.Contains("csharp", languages);
        Assert.Contains("python", languages);
        Assert.Equal(2, languages.Count);
    }

    [Fact]
    public void GetLanguages_NoRegistrations_ReturnsEmpty()
    {
        var languages = _registry.GetLanguages();
        Assert.Empty(languages);
    }

    // ── DiscoverStyles(string) with temp directory ──

    [Fact]
    public void DiscoverStyles_FileSystem_RegistersFromDirectoryStructure()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"styles_test_{Guid.NewGuid():N}");
        try
        {
            // Create: templates/csharp/_common, templates/csharp/clean, templates/csharp/minimal
            var csharpDir = Path.Combine(tempRoot, "csharp");
            Directory.CreateDirectory(Path.Combine(csharpDir, "_common"));
            Directory.CreateDirectory(Path.Combine(csharpDir, "clean"));
            Directory.CreateDirectory(Path.Combine(csharpDir, "minimal"));

            _registry.DiscoverStyles(tempRoot);

            var styles = _registry.GetStyles("csharp");
            Assert.Equal(2, styles.Count); // _common is skipped
            Assert.Contains(styles, s => s.Name == "clean");
            Assert.Contains(styles, s => s.Name == "minimal");
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, true);
        }
    }

    [Fact]
    public void DiscoverStyles_FileSystem_SkipsUnderscoreDirs()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"styles_test_{Guid.NewGuid():N}");
        try
        {
            var langDir = Path.Combine(tempRoot, "js");
            Directory.CreateDirectory(Path.Combine(langDir, "_common"));
            Directory.CreateDirectory(Path.Combine(langDir, "_internal"));

            _registry.DiscoverStyles(tempRoot);

            var styles = _registry.GetStyles("js");
            Assert.Empty(styles);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, true);
        }
    }

    [Fact]
    public void DiscoverStyles_NonExistentRoot_DoesNotThrow()
    {
        _registry.DiscoverStyles(@"C:\nonexistent_styles_root_12345");
        Assert.Empty(_registry.GetLanguages());
    }

    // ── DiscoverStyles(Assembly, prefix) ──

    [Fact]
    public void DiscoverStyles_Assembly_DoesNotThrowForCurrentAssembly()
    {
        // We don't have embedded resources matching, but it should not throw
        _registry.DiscoverStyles(typeof(StyleRegistryTests).Assembly, "NonExistent.Prefix");
        // No styles should be registered since prefix does not match
    }
}
