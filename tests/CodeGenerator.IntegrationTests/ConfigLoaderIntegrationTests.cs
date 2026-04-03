// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class ConfigLoaderIntegrationTests
{
    // ─── EnvironmentVariableMapper ──────────────────────────────────────

    [Fact]
    public void EnvironmentVariableMapper_Map_ExtractsCodegenFramework()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CODEGEN_FRAMEWORK"] = "net8.0",
            })
            .Build();

        var result = EnvironmentVariableMapper.Map(configuration);

        Assert.True(result.ContainsKey("framework"));
        Assert.Equal("net8.0", result["framework"]);
    }

    [Fact]
    public void EnvironmentVariableMapper_Map_ReturnsEmptyWhenNoCodegenVars()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SOME_OTHER_VAR"] = "value",
            })
            .Build();

        var result = EnvironmentVariableMapper.Map(configuration);

        Assert.Empty(result);
    }

    [Fact]
    public void EnvironmentVariableMapper_Map_HandlesMultipleCodegenVars()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CODEGEN_FRAMEWORK"] = "net9.0",
                ["CODEGEN_OUTPUT"] = "/src",
                ["CODEGEN_SLNX"] = "true",
                ["CODEGEN_AUTHOR"] = "TestAuthor",
                ["CODEGEN_LICENSE"] = "MIT",
            })
            .Build();

        var result = EnvironmentVariableMapper.Map(configuration);

        Assert.Equal(5, result.Count);
        Assert.Equal("net9.0", result["framework"]);
        Assert.Equal("/src", result["output"]);
        Assert.Equal("true", result["slnx"]);
        Assert.Equal("TestAuthor", result["templates.author"]);
        Assert.Equal("MIT", result["templates.license"]);
    }

    // ─── ConfigFileMapper ───────────────────────────────────────────────

    [Fact]
    public void ConfigFileMapper_ToFlatDictionary_MapsDefaultsFramework()
    {
        var config = new CodeGeneratorConfig
        {
            Defaults = new DefaultsSection { Framework = "net9.0" },
        };

        var result = ConfigFileMapper.ToFlatDictionary(config);

        Assert.True(result.ContainsKey("framework"));
        Assert.Equal("net9.0", result["framework"]);
    }

    [Fact]
    public void ConfigFileMapper_ToFlatDictionary_OmitsNullValues()
    {
        var config = new CodeGeneratorConfig();

        var result = ConfigFileMapper.ToFlatDictionary(config);

        Assert.Empty(result);
    }

    [Fact]
    public void ConfigFileMapper_ToFlatDictionary_MapsTemplatesAuthor()
    {
        var config = new CodeGeneratorConfig
        {
            Templates = new TemplatesSection { Author = "Quinntyne Brown" },
        };

        var result = ConfigFileMapper.ToFlatDictionary(config);

        Assert.True(result.ContainsKey("templates.author"));
        Assert.Equal("Quinntyne Brown", result["templates.author"]);
    }

    // ─── ConfigBootstrap ────────────────────────────────────────────────

    [Fact]
    public void ConfigBootstrap_GetBuiltInDefaults_ContainsExpectedValues()
    {
        var defaults = ConfigBootstrap.GetBuiltInDefaults();

        Assert.Equal("net9.0", defaults["framework"]);
        Assert.Equal(".", defaults["output"]);
        Assert.Equal("false", defaults["slnx"]);
    }

    [Fact]
    public void ConfigBootstrap_ConfigResolution_PrefersFileConfigOverDefaults()
    {
        var defaults = ConfigBootstrap.GetBuiltInDefaults();

        var fileConfig = ConfigFileMapper.ToFlatDictionary(new CodeGeneratorConfig
        {
            Defaults = new DefaultsSection { Framework = "net8.0" },
        });

        // Simulate resolution: file config overrides defaults (same layering as CodeGeneratorConfiguration)
        var resolved = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in defaults)
            resolved[kvp.Key] = kvp.Value;

        foreach (var kvp in fileConfig)
            resolved[kvp.Key] = kvp.Value;

        Assert.Equal("net8.0", resolved["framework"]);
        Assert.Equal(".", resolved["output"]);
        Assert.Equal("false", resolved["slnx"]);
    }
}
