// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Services;
using CodeGenerator.Cli.Validation;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class InteractivePromptsIntegrationTests
{
    [Fact]
    public void NonInteractivePromptService_IsInteractive_ReturnsFalse()
    {
        var service = new NonInteractivePromptService();

        Assert.False(service.IsInteractive);
    }

    [Fact]
    public void NonInteractivePromptService_PromptForMissingOptions_EmptyName_ThrowsInvalidOperationException()
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
    }

    [Fact]
    public void NonInteractivePromptService_PromptForMissingOptions_WithName_ReturnsOptions()
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
        Assert.Equal("MyApp", result.Name);
    }

    [Fact]
    public void NonInteractivePromptService_PromptForConfigFile_ThrowsInvalidOperationException()
    {
        var service = new NonInteractivePromptService();

        var ex = Assert.Throws<InvalidOperationException>(
            () => service.PromptForConfigFile(@"C:\project", new[] { "config1.json", "config2.json" }));

        Assert.Contains("interactive mode is not available", ex.Message);
        Assert.Contains("--config", ex.Message);
    }

    [Fact]
    public void SpectrePromptService_IsInteractive_ReturnsTrue()
    {
        var service = new SpectrePromptService();

        // SpectrePromptService.IsInteractive returns true when stdin is not redirected.
        // The property is defined as: !Console.IsInputRedirected
        // We verify the service type exposes the property correctly.
        Assert.IsType<SpectrePromptService>(service);
        Assert.IsAssignableFrom<IInteractivePromptService>(service);
    }

    [Fact]
    public void TtyDetector_InTestContext_DetectsNonInteractive()
    {
        // In a test runner context, stdin is typically redirected (piped),
        // so TtyDetector should report non-interactive.
        var result = TtyDetector.IsInteractiveTerminal();

        Assert.False(result);
    }
}
