// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;
using CodeGenerator.Core.Services;

namespace CodeGenerator.Core.UnitTests;

public class DryRunCommandServiceTests
{
    [Fact]
    public void Start_RecordsCommandInResult()
    {
        var result = new GenerationResult();
        var service = new DryRunCommandService(result);

        service.Start("dotnet new sln", "/work");

        Assert.Single(result.Commands);
        Assert.Equal("dotnet new sln", result.Commands[0].Command);
        Assert.Equal("/work", result.Commands[0].WorkingDirectory);
    }

    [Fact]
    public void Start_ReturnsZero()
    {
        var result = new GenerationResult();
        var service = new DryRunCommandService(result);

        var exitCode = service.Start("dotnet build");
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void Start_MultipleCommands_AllRecorded()
    {
        var result = new GenerationResult();
        var service = new DryRunCommandService(result);

        service.Start("cmd1", "/dir1");
        service.Start("cmd2", "/dir2");
        service.Start("cmd3");

        Assert.Equal(3, result.Commands.Count);
    }

    [Fact]
    public void Start_NullWorkingDirectory_Recorded()
    {
        var result = new GenerationResult();
        var service = new DryRunCommandService(result);

        service.Start("cmd", null);

        Assert.Null(result.Commands[0].WorkingDirectory);
    }

    [Fact]
    public void ImplementsICommandService()
    {
        var result = new GenerationResult();
        var service = new DryRunCommandService(result);
        Assert.IsAssignableFrom<ICommandService>(service);
    }
}
