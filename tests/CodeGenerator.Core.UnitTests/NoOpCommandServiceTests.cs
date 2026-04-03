// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Services;

namespace CodeGenerator.Core.UnitTests;

public class NoOpCommandServiceTests
{
    [Fact]
    public void Start_ReturnsZero()
    {
        var service = new NoOpCommandService();
        var result = service.Start("dotnet build");
        Assert.Equal(0, result);
    }

    [Fact]
    public void Start_WithWorkingDirectory_ReturnsZero()
    {
        var service = new NoOpCommandService();
        var result = service.Start("dotnet build", "/work");
        Assert.Equal(0, result);
    }

    [Fact]
    public void Start_WithWaitForExitFalse_ReturnsZero()
    {
        var service = new NoOpCommandService();
        var result = service.Start("dotnet build", null, false);
        Assert.Equal(0, result);
    }

    [Fact]
    public void ImplementsICommandService()
    {
        var service = new NoOpCommandService();
        Assert.IsAssignableFrom<ICommandService>(service);
    }

    [Fact]
    public void Start_NullCommand_DoesNotThrow()
    {
        var service = new NoOpCommandService();
        var result = service.Start(null!);
        Assert.Equal(0, result);
    }
}
