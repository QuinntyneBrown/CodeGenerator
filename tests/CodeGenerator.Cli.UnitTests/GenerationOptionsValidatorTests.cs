// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions.TestingHelpers;
using CodeGenerator.Cli.Validation;

namespace CodeGenerator.Cli.UnitTests;

public class GenerationOptionsValidatorTests
{
    private readonly GenerationOptionsValidator _validator;

    public GenerationOptionsValidatorTests()
    {
        var fs = new MockFileSystem();
        _validator = new GenerationOptionsValidator(fs);
    }

    [Fact]
    public void Validate_ValidOptions_ReturnsValid()
    {
        var options = new GenerationOptions
        {
            Name = "MyProject",
            OutputDirectory = "/output",
            Framework = "net9.0",
            Slnx = false,
        };

        var result = _validator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var options = new GenerationOptions
        {
            Name = "",
            OutputDirectory = "/output",
            Framework = "net9.0",
            Slnx = false,
        };

        var result = _validator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_InvalidCSharpName_ReturnsError()
    {
        var options = new GenerationOptions
        {
            Name = "123Invalid",
            OutputDirectory = "/output",
            Framework = "net9.0",
            Slnx = false,
        };

        var result = _validator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_EmptyFramework_ReturnsError()
    {
        var options = new GenerationOptions
        {
            Name = "MyProject",
            OutputDirectory = "/output",
            Framework = "",
            Slnx = false,
        };

        var result = _validator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Framework");
    }

    [Fact]
    public void Validate_InvalidFramework_ReturnsError()
    {
        var options = new GenerationOptions
        {
            Name = "MyProject",
            OutputDirectory = "/output",
            Framework = "invalid",
            Slnx = false,
        };

        var result = _validator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Framework");
    }

    [Fact]
    public void Validate_Net8Framework_IsValid()
    {
        var options = new GenerationOptions
        {
            Name = "MyProject",
            OutputDirectory = "/output",
            Framework = "net8.0",
            Slnx = false,
        };

        var result = _validator.Validate(options);

        Assert.True(result.IsValid);
    }
}
