// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using CodeGenerator.Core.Artifacts;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Services;

namespace CodeGenerator.Core.UnitTests;

public class GenerationContextTests
{
    [Fact]
    public void Constructor_SetsDryRun()
    {
        var fs = new MockFileSystem();
        var cmd = new NoOpCommandService();

        var ctx = new GenerationContext(true, fs, cmd, NullRollbackService.Instance);

        Assert.True(ctx.DryRun);
    }

    [Fact]
    public void Constructor_SetsDryRunFalse()
    {
        var fs = new MockFileSystem();
        var cmd = new NoOpCommandService();

        var ctx = new GenerationContext(false, fs, cmd, NullRollbackService.Instance);

        Assert.False(ctx.DryRun);
    }

    [Fact]
    public void Constructor_InitializesResult()
    {
        var fs = new MockFileSystem();
        var cmd = new NoOpCommandService();

        var ctx = new GenerationContext(false, fs, cmd, NullRollbackService.Instance);

        Assert.NotNull(ctx.Result);
        Assert.True(ctx.Result.IsSuccess);
        Assert.Equal(0, ctx.Result.TotalFileCount);
    }

    [Fact]
    public void Constructor_SetsFileSystem()
    {
        var fs = new MockFileSystem();
        var cmd = new NoOpCommandService();

        var ctx = new GenerationContext(false, fs, cmd, NullRollbackService.Instance);

        Assert.Same(fs, ctx.FileSystem);
    }

    [Fact]
    public void Constructor_SetsCommandService()
    {
        var fs = new MockFileSystem();
        var cmd = new NoOpCommandService();

        var ctx = new GenerationContext(false, fs, cmd, NullRollbackService.Instance);

        Assert.Same(cmd, ctx.CommandService);
    }

    [Fact]
    public void ImplementsIGenerationContext()
    {
        var fs = new MockFileSystem();
        var cmd = new NoOpCommandService();

        var ctx = new GenerationContext(false, fs, cmd, NullRollbackService.Instance);

        Assert.IsAssignableFrom<Artifacts.IGenerationContext>(ctx);
    }
}
