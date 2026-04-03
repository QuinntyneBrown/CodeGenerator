// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class HierarchicalConfigurationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public HierarchicalConfigurationTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddCoreServices(typeof(HierarchicalConfigurationTests).Assembly);
        services.AddDotNetServices();

        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    #region DD-36: CodeGeneratorConfiguration merge precedence

    [Fact]
    public void MergePrecedence_CliOverridesAll()
    {
        var config = new CodeGeneratorConfiguration(
            defaults: new Dictionary<string, string> { ["key"] = "default" },
            fileConfig: new Dictionary<string, string> { ["key"] = "file" },
            envConfig: new Dictionary<string, string> { ["key"] = "env" },
            cliConfig: new Dictionary<string, string> { ["key"] = "cli" });

        Assert.Equal("cli", config.GetValue("key"));
    }

    [Fact]
    public void MergePrecedence_EnvOverridesFileAndDefaults()
    {
        var config = new CodeGeneratorConfiguration(
            defaults: new Dictionary<string, string> { ["key"] = "default" },
            fileConfig: new Dictionary<string, string> { ["key"] = "file" },
            envConfig: new Dictionary<string, string> { ["key"] = "env" },
            cliConfig: new Dictionary<string, string>());

        Assert.Equal("env", config.GetValue("key"));
    }

    [Fact]
    public void MergePrecedence_FileOverridesDefaults()
    {
        var config = new CodeGeneratorConfiguration(
            defaults: new Dictionary<string, string> { ["key"] = "default" },
            fileConfig: new Dictionary<string, string> { ["key"] = "file" },
            envConfig: new Dictionary<string, string>(),
            cliConfig: new Dictionary<string, string>());

        Assert.Equal("file", config.GetValue("key"));
    }

    [Fact]
    public void MergePrecedence_DefaultsUsedWhenNoOverride()
    {
        var config = new CodeGeneratorConfiguration(
            defaults: new Dictionary<string, string> { ["key"] = "default" },
            fileConfig: new Dictionary<string, string>(),
            envConfig: new Dictionary<string, string>(),
            cliConfig: new Dictionary<string, string>());

        Assert.Equal("default", config.GetValue("key"));
    }

    #endregion

    #region DD-36: GetValue, HasKey, GetAll, GetSection

    [Fact]
    public void GetValue_MissingKey_ReturnsNull()
    {
        var config = new CodeGeneratorConfiguration(
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            new Dictionary<string, string>());

        Assert.Null(config.GetValue("missing"));
    }

    [Fact]
    public void GetValueGeneric_ReturnsTypedValue()
    {
        var config = new CodeGeneratorConfiguration(
            defaults: new Dictionary<string, string> { ["port"] = "8080" },
            fileConfig: new Dictionary<string, string>(),
            envConfig: new Dictionary<string, string>(),
            cliConfig: new Dictionary<string, string>());

        Assert.Equal(8080, config.GetValue<int>("port"));
    }

    [Fact]
    public void HasKey_ReturnsTrueForExistingKey()
    {
        var config = new CodeGeneratorConfiguration(
            defaults: new Dictionary<string, string> { ["key"] = "val" },
            fileConfig: new Dictionary<string, string>(),
            envConfig: new Dictionary<string, string>(),
            cliConfig: new Dictionary<string, string>());

        Assert.True(config.HasKey("key"));
        Assert.False(config.HasKey("other"));
    }

    [Fact]
    public void GetSection_ReturnsMatchingPrefix()
    {
        var config = new CodeGeneratorConfiguration(
            defaults: new Dictionary<string, string>
            {
                ["dotnet.framework"] = "net9.0",
                ["dotnet.nullable"] = "enable",
                ["react.bundler"] = "vite"
            },
            fileConfig: new Dictionary<string, string>(),
            envConfig: new Dictionary<string, string>(),
            cliConfig: new Dictionary<string, string>());

        var section = config.GetSection("dotnet");

        Assert.Equal(2, section.Count);
        Assert.Equal("net9.0", section["framework"]);
        Assert.Equal("enable", section["nullable"]);
    }

    [Fact]
    public void CaseInsensitive_KeyLookup()
    {
        var config = new CodeGeneratorConfiguration(
            defaults: new Dictionary<string, string> { ["MyKey"] = "val" },
            fileConfig: new Dictionary<string, string>(),
            envConfig: new Dictionary<string, string>(),
            cliConfig: new Dictionary<string, string>());

        Assert.Equal("val", config.GetValue("mykey"));
        Assert.Equal("val", config.GetValue("MYKEY"));
    }

    #endregion

    #region DD-36: ICodeGeneratorConfiguration DI registration

    [Fact]
    public void ICodeGeneratorConfiguration_ResolvedFromDI()
    {
        var config = _serviceProvider.GetRequiredService<ICodeGeneratorConfiguration>();

        Assert.NotNull(config);
    }

    #endregion
}
