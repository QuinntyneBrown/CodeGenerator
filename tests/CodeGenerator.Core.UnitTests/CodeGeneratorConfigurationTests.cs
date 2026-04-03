// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Configuration;

namespace CodeGenerator.Core.UnitTests;

public class CodeGeneratorConfigurationTests
{
    private static CodeGeneratorConfiguration CreateConfig(
        Dictionary<string, string>? defaults = null,
        Dictionary<string, string>? fileConfig = null,
        Dictionary<string, string>? envConfig = null,
        Dictionary<string, string>? cliConfig = null)
    {
        return new CodeGeneratorConfiguration(
            defaults ?? new Dictionary<string, string>(),
            fileConfig ?? new Dictionary<string, string>(),
            envConfig ?? new Dictionary<string, string>(),
            cliConfig ?? new Dictionary<string, string>());
    }

    // ── Multi-layer resolution ──

    [Fact]
    public void Constructor_CliOverridesAllLayers()
    {
        var config = CreateConfig(
            defaults: new Dictionary<string, string> { ["key"] = "default" },
            fileConfig: new Dictionary<string, string> { ["key"] = "file" },
            envConfig: new Dictionary<string, string> { ["key"] = "env" },
            cliConfig: new Dictionary<string, string> { ["key"] = "cli" });

        Assert.Equal("cli", config.GetValue("key"));
    }

    [Fact]
    public void Constructor_EnvOverridesFileAndDefaults()
    {
        var config = CreateConfig(
            defaults: new Dictionary<string, string> { ["key"] = "default" },
            fileConfig: new Dictionary<string, string> { ["key"] = "file" },
            envConfig: new Dictionary<string, string> { ["key"] = "env" });

        Assert.Equal("env", config.GetValue("key"));
    }

    [Fact]
    public void Constructor_FileOverridesDefaults()
    {
        var config = CreateConfig(
            defaults: new Dictionary<string, string> { ["key"] = "default" },
            fileConfig: new Dictionary<string, string> { ["key"] = "file" });

        Assert.Equal("file", config.GetValue("key"));
    }

    [Fact]
    public void Constructor_DefaultIsUsedWhenNoOverride()
    {
        var config = CreateConfig(
            defaults: new Dictionary<string, string> { ["key"] = "default" });

        Assert.Equal("default", config.GetValue("key"));
    }

    // ── GetValue ──

    [Fact]
    public void GetValue_ExistingKey_ReturnsValue()
    {
        var config = CreateConfig(
            defaults: new Dictionary<string, string> { ["name"] = "test" });

        Assert.Equal("test", config.GetValue("name"));
    }

    [Fact]
    public void GetValue_MissingKey_ReturnsNull()
    {
        var config = CreateConfig();
        Assert.Null(config.GetValue("nonexistent"));
    }

    [Fact]
    public void GetValue_CaseInsensitive()
    {
        var config = CreateConfig(
            defaults: new Dictionary<string, string> { ["MyKey"] = "value" });

        Assert.Equal("value", config.GetValue("mykey"));
        Assert.Equal("value", config.GetValue("MYKEY"));
    }

    // ── GetValue<T> ──

    [Fact]
    public void GetValueGeneric_Int_ConvertsCorrectly()
    {
        var config = CreateConfig(
            defaults: new Dictionary<string, string> { ["count"] = "42" });

        Assert.Equal(42, config.GetValue<int>("count"));
    }

    [Fact]
    public void GetValueGeneric_Bool_ConvertsCorrectly()
    {
        var config = CreateConfig(
            defaults: new Dictionary<string, string> { ["enabled"] = "True" });

        Assert.True(config.GetValue<bool>("enabled"));
    }

    [Fact]
    public void GetValueGeneric_MissingKey_ReturnsDefault()
    {
        var config = CreateConfig();
        Assert.Equal(99, config.GetValue("missing", 99));
    }

    [Fact]
    public void GetValueGeneric_MissingKey_ReturnsDefaultBool()
    {
        var config = CreateConfig();
        Assert.True(config.GetValue("missing", true));
    }

    [Fact]
    public void GetValueGeneric_String_ReturnsRawValue()
    {
        var config = CreateConfig(
            defaults: new Dictionary<string, string> { ["key"] = "hello" });

        Assert.Equal("hello", config.GetValue<string>("key"));
    }

    // ── HasKey ──

    [Fact]
    public void HasKey_ExistingKey_ReturnsTrue()
    {
        var config = CreateConfig(
            defaults: new Dictionary<string, string> { ["key"] = "value" });

        Assert.True(config.HasKey("key"));
    }

    [Fact]
    public void HasKey_MissingKey_ReturnsFalse()
    {
        var config = CreateConfig();
        Assert.False(config.HasKey("nonexistent"));
    }

    [Fact]
    public void HasKey_CaseInsensitive()
    {
        var config = CreateConfig(
            defaults: new Dictionary<string, string> { ["MyKey"] = "value" });

        Assert.True(config.HasKey("mykey"));
    }

    // ── GetAll ──

    [Fact]
    public void GetAll_ReturnsAllResolvedKeys()
    {
        var config = CreateConfig(
            defaults: new Dictionary<string, string> { ["a"] = "1" },
            cliConfig: new Dictionary<string, string> { ["b"] = "2" });

        var all = config.GetAll();
        Assert.Equal(2, all.Count);
        Assert.Equal("1", all["a"]);
        Assert.Equal("2", all["b"]);
    }

    [Fact]
    public void GetAll_OverriddenKeysShowFinalValue()
    {
        var config = CreateConfig(
            defaults: new Dictionary<string, string> { ["key"] = "default" },
            cliConfig: new Dictionary<string, string> { ["key"] = "override" });

        var all = config.GetAll();
        Assert.Single(all);
        Assert.Equal("override", all["key"]);
    }

    // ── GetSection ──

    [Fact]
    public void GetSection_ReturnsMatchingPrefix()
    {
        var config = CreateConfig(
            defaults: new Dictionary<string, string>
            {
                ["logging.level"] = "Debug",
                ["logging.format"] = "json",
                ["output.path"] = "/out"
            });

        var section = config.GetSection("logging");
        Assert.Equal(2, section.Count);
        Assert.Equal("Debug", section["level"]);
        Assert.Equal("json", section["format"]);
    }

    [Fact]
    public void GetSection_NoMatchingPrefix_ReturnsEmpty()
    {
        var config = CreateConfig(
            defaults: new Dictionary<string, string> { ["key"] = "value" });

        var section = config.GetSection("nonexistent");
        Assert.Empty(section);
    }

    [Fact]
    public void GetSection_CaseInsensitive()
    {
        var config = CreateConfig(
            defaults: new Dictionary<string, string> { ["Logging.Level"] = "Info" });

        var section = config.GetSection("logging");
        Assert.Single(section);
        Assert.Equal("Info", section["Level"]);
    }
}
