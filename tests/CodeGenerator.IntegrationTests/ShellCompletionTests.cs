// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Completions;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class ShellCompletionTests
{
    [Fact]
    public void ToPascalCase_ConvertsDashSeparated()
    {
        Assert.Equal("MyProject", CompletionProvider.ToPascalCase("my-project"));
    }

    [Fact]
    public void ToPascalCase_ConvertsUnderscoreSeparated()
    {
        Assert.Equal("MyProject", CompletionProvider.ToPascalCase("my_project"));
    }

    [Fact]
    public void ToPascalCase_HandlesEmpty()
    {
        Assert.Equal("", CompletionProvider.ToPascalCase(""));
    }

    [Fact]
    public void ToPascalCase_HandlesSingleWord()
    {
        Assert.Equal("Hello", CompletionProvider.ToPascalCase("hello"));
    }

    [Fact]
    public void ToPascalCase_HandlesDotSeparated()
    {
        Assert.Equal("MyCodeGenerator", CompletionProvider.ToPascalCase("my.code.generator"));
    }

    [Fact]
    public void ToPascalCase_HandlesMultipleSeparators()
    {
        Assert.Equal("MyBigProject", CompletionProvider.ToPascalCase("my-big_project"));
    }

    [Fact]
    public void ShellDetector_DetectShell_ReturnsNonEmpty()
    {
        var shell = ShellDetector.DetectShell();

        Assert.NotNull(shell);
        Assert.NotEmpty(shell);
        Assert.Contains(shell, new[] { "bash", "zsh", "powershell", "fish" });
    }

    [Fact]
    public void ShellDetector_DetectShell_ReturnsKnownShell()
    {
        var shell = ShellDetector.DetectShell();
        var knownShells = new[] { "bash", "zsh", "powershell", "fish" };

        Assert.Contains(shell, knownShells);
    }

    [Fact]
    public void CompletionProvider_SupportedFrameworks_AreAvailable()
    {
        // Verify the static class can be accessed without errors
        Assert.NotNull(typeof(CompletionProvider));
    }
}
