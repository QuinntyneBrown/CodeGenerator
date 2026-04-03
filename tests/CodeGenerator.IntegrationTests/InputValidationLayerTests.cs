// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Validation;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Validation;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class InputValidationLayerTests
{
    private static MockFileSystem CreateFileSystemWithOutputDir()
    {
        var fs = new MockFileSystem();
        fs.AddDirectory(@"C:\projects");
        return fs;
    }

    [Fact]
    public void ValidName_ReturnsSuccess()
    {
        var fileSystem = CreateFileSystemWithOutputDir();
        var validator = new GenerationOptionsValidator(fileSystem);

        var options = new GenerationOptions
        {
            Name = "MyApp",
            OutputDirectory = @"C:\projects\output",
            Framework = "net9.0",
            Slnx = false,
        };

        var result = validator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("MyApp")]
    [InlineData("My.App")]
    [InlineData("_app")]
    [InlineData("A")]
    public void ValidNames_ReturnSuccess(string name)
    {
        var fileSystem = CreateFileSystemWithOutputDir();
        var validator = new GenerationOptionsValidator(fileSystem);

        var options = new GenerationOptions
        {
            Name = name,
            OutputDirectory = @"C:\projects\output",
            Framework = "net9.0",
            Slnx = false,
        };

        var result = validator.Validate(options);

        Assert.Empty(result.Errors.Where(e => e.PropertyName == nameof(GenerationOptions.Name)));
    }

    [Fact]
    public void InvalidName_StartsWithDigit_ReturnsError()
    {
        var fileSystem = CreateFileSystemWithOutputDir();
        var validator = new GenerationOptionsValidator(fileSystem);

        var options = new GenerationOptions
        {
            Name = "123App",
            OutputDirectory = @"C:\projects\output",
            Framework = "net9.0",
            Slnx = false,
        };

        var result = validator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GenerationOptions.Name));
    }

    [Fact]
    public void InvalidName_SpecialChars_ReturnsError()
    {
        var fileSystem = CreateFileSystemWithOutputDir();
        var validator = new GenerationOptionsValidator(fileSystem);

        var options = new GenerationOptions
        {
            Name = "My-App!",
            OutputDirectory = @"C:\projects\output",
            Framework = "net9.0",
            Slnx = false,
        };

        var result = validator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GenerationOptions.Name));
    }

    [Fact]
    public void EmptyName_ReturnsError()
    {
        var fileSystem = CreateFileSystemWithOutputDir();
        var validator = new GenerationOptionsValidator(fileSystem);

        var options = new GenerationOptions
        {
            Name = "",
            OutputDirectory = @"C:\projects\output",
            Framework = "net9.0",
            Slnx = false,
        };

        var result = validator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GenerationOptions.Name));
    }

    [Fact]
    public void LongName_ReturnsWarning()
    {
        var fileSystem = CreateFileSystemWithOutputDir();
        var validator = new GenerationOptionsValidator(fileSystem);

        var options = new GenerationOptions
        {
            Name = new string('A', 200),
            OutputDirectory = @"C:\projects\output",
            Framework = "net9.0",
            Slnx = false,
        };

        var result = validator.Validate(options);

        Assert.True(result.IsValid); // Warnings don't make it invalid
        Assert.Contains(result.Warnings, e => e.PropertyName == nameof(GenerationOptions.Name));
    }

    [Fact]
    public void OutputDir_ParentMissing_ReturnsError()
    {
        var fileSystem = new MockFileSystem();
        var validator = new GenerationOptionsValidator(fileSystem);

        var options = new GenerationOptions
        {
            Name = "MyApp",
            OutputDirectory = @"C:\nonexistent\output",
            Framework = "net9.0",
            Slnx = false,
        };

        var result = validator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GenerationOptions.OutputDirectory));
    }

    [Theory]
    [InlineData("net8.0")]
    [InlineData("net9.0")]
    public void Framework_Valid_ReturnsSuccess(string framework)
    {
        var fileSystem = CreateFileSystemWithOutputDir();
        var validator = new GenerationOptionsValidator(fileSystem);

        var options = new GenerationOptions
        {
            Name = "MyApp",
            OutputDirectory = @"C:\projects\output",
            Framework = framework,
            Slnx = false,
        };

        var result = validator.Validate(options);

        Assert.Empty(result.Errors.Where(e => e.PropertyName == nameof(GenerationOptions.Framework)));
    }

    [Theory]
    [InlineData("dotnet9")]
    [InlineData("")]
    public void Framework_Invalid_ReturnsError(string framework)
    {
        var fileSystem = CreateFileSystemWithOutputDir();
        var validator = new GenerationOptionsValidator(fileSystem);

        var options = new GenerationOptions
        {
            Name = "MyApp",
            OutputDirectory = @"C:\projects\output",
            Framework = framework,
            Slnx = false,
        };

        var result = validator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GenerationOptions.Framework));
    }

    [Fact]
    public void CliValidationException_WrapsValidationResult()
    {
        var validationResult = new ValidationResult();
        validationResult.AddError("Name", "Name is required");

        var ex = new CliValidationException(validationResult);

        Assert.Equal(CliExitCodes.ValidationError, ex.ExitCode);
        Assert.Same(validationResult, ex.ValidationResult);
        Assert.Contains("Name", ex.Message);
    }
}
