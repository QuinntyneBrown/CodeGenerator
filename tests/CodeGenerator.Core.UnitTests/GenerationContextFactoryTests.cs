// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using CodeGenerator.Core.Artifacts;
using CodeGenerator.Core.Services;

namespace CodeGenerator.Core.UnitTests;

public class GenerationContextFactoryTests
{
    [Fact]
    public void Create_NotDryRun_ReturnsDryRunFalse()
    {
        var fs = new MockFileSystem();
        var cmd = new NoOpCommandService();

        var ctx = GenerationContextFactory.Create(false, fs, cmd);

        Assert.False(ctx.DryRun);
    }

    [Fact]
    public void Create_NotDryRun_UsesRealCommandService()
    {
        var fs = new MockFileSystem();
        var cmd = new NoOpCommandService();

        var ctx = GenerationContextFactory.Create(false, fs, cmd);

        Assert.Same(cmd, ctx.CommandService);
    }

    [Fact]
    public void Create_NotDryRun_UsesRealFileSystem()
    {
        var fs = new MockFileSystem();
        var cmd = new NoOpCommandService();

        var ctx = GenerationContextFactory.Create(false, fs, cmd);

        Assert.Same(fs, ctx.FileSystem);
    }

    [Fact]
    public void Create_DryRun_ReturnsDryRunTrue()
    {
        var fs = new MockFileSystem();
        var cmd = new NoOpCommandService();

        var ctx = GenerationContextFactory.Create(true, fs, cmd);

        Assert.True(ctx.DryRun);
    }

    [Fact]
    public void Create_DryRun_UsesDryRunCommandService()
    {
        var fs = new MockFileSystem();
        var cmd = new NoOpCommandService();

        var ctx = GenerationContextFactory.Create(true, fs, cmd);

        Assert.IsType<DryRunCommandService>(ctx.CommandService);
    }

    [Fact]
    public void Create_DryRun_UsesRealFileSystemForReads()
    {
        var fs = new MockFileSystem();
        var cmd = new NoOpCommandService();

        var ctx = GenerationContextFactory.Create(true, fs, cmd);

        Assert.Same(fs, ctx.FileSystem);
    }

    [Fact]
    public void Create_ReturnsIGenerationContext()
    {
        var fs = new MockFileSystem();
        var cmd = new NoOpCommandService();

        var ctx = GenerationContextFactory.Create(false, fs, cmd);

        Assert.IsAssignableFrom<Artifacts.IGenerationContext>(ctx);
    }
}
