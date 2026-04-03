// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Services;
using CodeGenerator.Cli.Validation;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class InteractiveModeTests
{
    [Fact]
    public void NonInteractivePromptService_IsInteractive_ReturnsFalse()
    {
        var service = new NonInteractivePromptService();

        Assert.False(service.IsInteractive);
    }

    [Fact]
    public void NonInteractivePromptService_MissingName_Throws()
    {
        var service = new NonInteractivePromptService();
        var partial = new GenerationOptions
        {
            Name = "",
            OutputDirectory = @"C:\output",
            Framework = "net9.0",
            Slnx = false,
        };

        var ex = Assert.Throws<InvalidOperationException>(
            () => service.PromptForMissingOptions(partial));

        Assert.Contains("--name", ex.Message);
        Assert.Contains("interactive mode is not available", ex.Message);
    }

    [Fact]
    public void NonInteractivePromptService_WithName_ReturnsPartial()
    {
        var service = new NonInteractivePromptService();
        var partial = new GenerationOptions
        {
            Name = "MyApp",
            OutputDirectory = @"C:\output",
            Framework = "net9.0",
            Slnx = false,
        };

        var result = service.PromptForMissingOptions(partial);

        Assert.Same(partial, result);
    }

    [Fact]
    public void NonInteractivePromptService_WhitespaceName_Throws()
    {
        var service = new NonInteractivePromptService();
        var partial = new GenerationOptions
        {
            Name = "   ",
            OutputDirectory = @"C:\output",
            Framework = "net9.0",
            Slnx = false,
        };

        Assert.Throws<InvalidOperationException>(
            () => service.PromptForMissingOptions(partial));
    }

    [Fact]
    public void GenerationOptions_DefaultValues()
    {
        var options = new GenerationOptions
        {
            Name = "Test",
            OutputDirectory = @"C:\output",
            Framework = "net9.0",
            Slnx = false,
        };

        Assert.Equal("Test", options.Name);
        Assert.Null(options.LocalSourceRoot);
        Assert.False(options.Slnx);
    }

    [Fact]
    public void GenerationOptions_WithLocalSourceRoot()
    {
        var options = new GenerationOptions
        {
            Name = "Test",
            OutputDirectory = @"C:\output",
            Framework = "net9.0",
            Slnx = true,
            LocalSourceRoot = @"C:\src\CodeGenerator",
        };

        Assert.Equal(@"C:\src\CodeGenerator", options.LocalSourceRoot);
        Assert.True(options.Slnx);
    }
}
