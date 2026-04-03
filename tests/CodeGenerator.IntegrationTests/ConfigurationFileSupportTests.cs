// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Configuration;
using CodeGenerator.IntegrationTests.Helpers;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class ConfigurationFileSupportTests : IDisposable
{
    private readonly TempDirectoryFixture _fixture;

    public ConfigurationFileSupportTests()
    {
        _fixture = new TempDirectoryFixture();
    }

    public void Dispose() => _fixture.Dispose();

    [Fact]
    public async Task LoadAsync_NoConfigFile_ReturnsDefaults()
    {
        var loader = new ConfigurationLoader();

        var config = await loader.LoadAsync(_fixture.Path);

        Assert.NotNull(config);
        Assert.Null(config.Defaults.Framework);
        Assert.Null(config.Defaults.SolutionFormat);
        Assert.Null(config.Defaults.Output);
    }

    [Fact]
    public async Task LoadAsync_WithConfigFile_ReturnsValues()
    {
        var loader = new ConfigurationLoader();
        var json = """
            {
              "defaults": {
                "framework": "net8.0",
                "solutionFormat": "slnx",
                "output": "./generated"
              },
              "templates": {
                "author": "Test Author",
                "license": "MIT"
              }
            }
            """;
        File.WriteAllText(Path.Combine(_fixture.Path, ".codegenerator.json"), json);

        var config = await loader.LoadAsync(_fixture.Path);

        Assert.Equal("net8.0", config.Defaults.Framework);
        Assert.Equal("slnx", config.Defaults.SolutionFormat);
        Assert.Equal("./generated", config.Defaults.Output);
        Assert.Equal("Test Author", config.Templates.Author);
        Assert.Equal("MIT", config.Templates.License);
    }

    [Fact]
    public async Task LoadAsync_ConfigInParentDirectory_IsDiscovered()
    {
        var loader = new ConfigurationLoader();
        var json = """{ "defaults": { "framework": "net9.0" } }""";
        File.WriteAllText(Path.Combine(_fixture.Path, ".codegenerator.json"), json);

        var childDir = Path.Combine(_fixture.Path, "src", "MyProject");
        Directory.CreateDirectory(childDir);

        var config = await loader.LoadAsync(childDir);

        Assert.Equal("net9.0", config.Defaults.Framework);
    }

    [Fact]
    public async Task LoadAsync_EmptyConfigFile_ReturnsDefaults()
    {
        var loader = new ConfigurationLoader();
        File.WriteAllText(Path.Combine(_fixture.Path, ".codegenerator.json"), "{}");

        var config = await loader.LoadAsync(_fixture.Path);

        Assert.NotNull(config);
        Assert.Null(config.Defaults.Framework);
    }

    [Fact]
    public void CodeGeneratorConfig_DefaultsSection_HasNullDefaults()
    {
        var config = new CodeGeneratorConfig();

        Assert.NotNull(config.Defaults);
        Assert.NotNull(config.Templates);
        Assert.Null(config.Defaults.Framework);
        Assert.Null(config.Defaults.SolutionFormat);
        Assert.Null(config.Defaults.Output);
        Assert.Null(config.Templates.Author);
        Assert.Null(config.Templates.License);
        Assert.Null(config.Templates.TemplatesDirectory);
    }

    [Fact]
    public async Task LoadAsync_CaseInsensitiveProperties_Works()
    {
        var loader = new ConfigurationLoader();
        var json = """{ "Defaults": { "Framework": "net8.0" } }""";
        File.WriteAllText(Path.Combine(_fixture.Path, ".codegenerator.json"), json);

        var config = await loader.LoadAsync(_fixture.Path);

        Assert.Equal("net8.0", config.Defaults.Framework);
    }

    [Fact]
    public async Task LoadAsync_WithSchemaProperty_DoesNotFail()
    {
        var loader = new ConfigurationLoader();
        var json = """
            {
              "$schema": "https://example.com/schema.json",
              "defaults": { "framework": "net9.0" }
            }
            """;
        File.WriteAllText(Path.Combine(_fixture.Path, ".codegenerator.json"), json);

        var config = await loader.LoadAsync(_fixture.Path);

        Assert.Equal("net9.0", config.Defaults.Framework);
    }
}
