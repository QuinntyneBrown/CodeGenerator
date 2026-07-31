// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions.TestingHelpers;
using CodeGenerator.Cli.Configuration;
using CodeGenerator.Cli.Services;
using CodeGenerator.Cli.Validation;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Cli.UnitTests;

/// <summary>
/// Regression tests for the four exit-code contract defects fixed alongside the
/// documentation site. Each test names the behaviour that was previously wrong.
/// </summary>
public class ExitCodeContractTests
{
    // --- Defect 1: the default output directory was unusable -------------------

    [Fact]
    public void BuiltInDefaults_DoNotSupplyOutput()
    {
        // Supplying "." here shadowed the `?? Directory.GetCurrentDirectory()` fallback
        // on every --output option, and "." then failed ParentDirectoryExists.
        var defaults = ConfigBootstrap.GetBuiltInDefaults();

        Assert.False(defaults.ContainsKey("output"));
    }

    [Fact]
    public void BuiltInDefaults_StillSupplyFrameworkAndSlnx()
    {
        var defaults = ConfigBootstrap.GetBuiltInDefaults();

        Assert.Equal("net9.0", defaults["framework"]);
        Assert.Equal("false", defaults["slnx"]);
    }

    [Fact]
    public void ParentDirectoryExists_AcceptsCurrentDirectoryToken()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(fileSystem.Directory.GetCurrentDirectory());
        var rules = new FileSystemRules(fileSystem);

        // Previously false: Path.GetDirectoryName(".") is string.Empty.
        Assert.True(rules.ParentDirectoryExists("."));
    }

    [Fact]
    public void ParentDirectoryExists_AcceptsBareRelativeDirectory()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(fileSystem.Directory.GetCurrentDirectory());
        var rules = new FileSystemRules(fileSystem);

        // Previously false for the most natural form a user types.
        Assert.True(rules.ParentDirectoryExists("mydir"));
    }

    [Fact]
    public void ParentDirectoryExists_AcceptsDotSlashRelativeDirectory()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(fileSystem.Directory.GetCurrentDirectory());
        var rules = new FileSystemRules(fileSystem);

        Assert.True(rules.ParentDirectoryExists("./mydir"));
    }

    [Fact]
    public void ParentDirectoryExists_RejectsPathWhoseParentIsAbsent()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(fileSystem.Directory.GetCurrentDirectory());
        var rules = new FileSystemRules(fileSystem);

        var absent = fileSystem.Path.Combine(
            fileSystem.Directory.GetCurrentDirectory(), "no-such-parent", "child");

        Assert.False(rules.ParentDirectoryExists(absent));
    }

    [Fact]
    public void ParentDirectoryExists_RejectsNullOrWhitespace()
    {
        var rules = new FileSystemRules(new MockFileSystem());

        Assert.False(rules.ParentDirectoryExists(null));
        Assert.False(rules.ParentDirectoryExists("   "));
    }

    [Fact]
    public void GenerationOptionsValidator_AcceptsBareRelativeOutputDirectory()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(fileSystem.Directory.GetCurrentDirectory());
        var validator = new GenerationOptionsValidator(fileSystem);

        var result = validator.Validate(new GenerationOptions
        {
            Name = "Demo",
            OutputDirectory = "out",
            Framework = "net9.0",
            Slnx = false,
        });

        Assert.True(result.IsValid);
    }

    // --- Defect 4: defaults.solutionFormat crashed option construction ---------

    [Theory]
    [InlineData("slnx", "true")]
    [InlineData("SLNX", "true")]
    [InlineData("sln", "false")]
    [InlineData("SLN", "false")]
    public void ConfigFileMapper_MapsSolutionFormatToBoolean(string solutionFormat, string expected)
    {
        // Previously the raw value was stored, and Convert.ChangeType(raw, typeof(bool))
        // threw FormatException for both "sln" and "slnx", surfacing as exit code 99.
        var config = new CodeGeneratorConfig();
        config.Defaults.SolutionFormat = solutionFormat;

        var flat = ConfigFileMapper.ToFlatDictionary(config);

        Assert.Equal(expected, flat["slnx"]);
    }

    [Fact]
    public void ConfigFileMapper_MappedSolutionFormat_ConvertsWithoutThrowing()
    {
        var config = new CodeGeneratorConfig();
        config.Defaults.SolutionFormat = "slnx";

        var resolved = new Core.Configuration.CodeGeneratorConfiguration(
            defaults: ConfigBootstrap.GetBuiltInDefaults(),
            fileConfig: ConfigFileMapper.ToFlatDictionary(config),
            envConfig: new Dictionary<string, string>(),
            cliConfig: new Dictionary<string, string>());

        Assert.True(resolved.GetValue<bool>("slnx", false));
    }

    [Fact]
    public void ConfigFileMapper_OmitsSolutionFormatWhenUnset()
    {
        var flat = ConfigFileMapper.ToFlatDictionary(new CodeGeneratorConfig());

        Assert.False(flat.ContainsKey("slnx"));
    }

    // --- Defect 3: a missing --name exited 99 as an internal error -------------

    [Fact]
    public void NonInteractivePrompt_MissingName_ThrowsValidationExceptionWithExitCodeOne()
    {
        var service = new NonInteractivePromptService();

        var exception = Assert.Throws<CliValidationException>(
            () => service.PromptForMissingOptions(new GenerationOptions { Name = string.Empty, OutputDirectory = ".", Framework = "net9.0", Slnx = false }));

        Assert.Equal(CliExitCodes.ValidationError, exception.ExitCode);
    }

    [Fact]
    public void NonInteractivePrompt_MissingName_PreservesGuidanceMessage()
    {
        var service = new NonInteractivePromptService();

        var exception = Assert.Throws<CliValidationException>(
            () => service.PromptForMissingOptions(new GenerationOptions { Name = string.Empty, OutputDirectory = ".", Framework = "net9.0", Slnx = false }));

        Assert.NotNull(exception.ValidationResult);
        var error = Assert.Single(exception.ValidationResult!.Errors);
        Assert.Contains("--name", error.ErrorMessage);
        Assert.Contains("stdin is not a terminal", error.ErrorMessage);
    }

    [Fact]
    public void NonInteractivePrompt_WithName_ReturnsOptionsUnchanged()
    {
        var service = new NonInteractivePromptService();
        var options = new GenerationOptions { Name = "Demo", OutputDirectory = ".", Framework = "net9.0", Slnx = false };

        Assert.Same(options, service.PromptForMissingOptions(options));
    }

    [Fact]
    public void NonInteractivePrompt_ConfigFileSelection_ThrowsValidationExceptionWithExitCodeOne()
    {
        var service = new NonInteractivePromptService();

        var exception = Assert.Throws<CliValidationException>(
            () => service.PromptForConfigFile("/work", ["a.yaml", "b.yaml"]));

        Assert.Equal(CliExitCodes.ValidationError, exception.ExitCode);
        Assert.Contains("--config", exception.ValidationResult!.Errors[0].ErrorMessage);
    }

    [Fact]
    public void NonInteractivePrompt_IsNotInteractive()
    {
        Assert.False(new NonInteractivePromptService().IsInteractive);
    }
}
