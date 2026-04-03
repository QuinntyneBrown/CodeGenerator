// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class ResourceTrackingTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public ResourceTrackingTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddCoreServices(typeof(ResourceTrackingTests).Assembly);
        services.AddDotNetServices();

        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    #region DD-29: Resource Tracking - MarkHandled / IsHandled

    [Fact]
    public void MarkHandled_ThenIsHandled_ReturnsTrue()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IGenerationContext>();

        context.MarkHandled("ClassModel:UserController", "ControllerStrategy");

        Assert.True(context.IsHandled("ClassModel:UserController"));
    }

    [Fact]
    public void IsHandled_WhenNotMarked_ReturnsFalse()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IGenerationContext>();

        Assert.False(context.IsHandled("ClassModel:UserController"));
    }

    #endregion

    #region DD-29: Resource Tracking - GetHandler

    [Fact]
    public void GetHandler_ReturnsHandlerName()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IGenerationContext>();

        context.MarkHandled("ClassModel:UserController", "ControllerStrategy");

        Assert.Equal("ControllerStrategy", context.GetHandler("ClassModel:UserController"));
    }

    [Fact]
    public void GetHandler_WhenNotHandled_ThrowsKeyNotFound()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IGenerationContext>();

        Assert.Throws<KeyNotFoundException>(() => context.GetHandler("ClassModel:Missing"));
    }

    #endregion

    #region DD-29: Resource Tracking - Double MarkHandled

    [Fact]
    public void MarkHandled_Twice_ThrowsInvalidOperation()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IGenerationContext>();

        context.MarkHandled("ClassModel:UserController", "Strategy1");

        Assert.Throws<InvalidOperationException>(() =>
            context.MarkHandled("ClassModel:UserController", "Strategy2"));
    }

    #endregion

    #region DD-29: Resource Tracking - GetAllHandled

    [Fact]
    public void GetAllHandled_ReturnsSnapshot()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IGenerationContext>();

        context.MarkHandled("ClassModel:A", "S1");
        context.MarkHandled("InterfaceModel:B", "S2");
        context.MarkHandled("ComponentModel:C", "S3");

        var all = context.GetAllHandled();

        Assert.Equal(3, all.Count);
        Assert.Equal("S1", all["ClassModel:A"]);
        Assert.Equal("S2", all["InterfaceModel:B"]);
        Assert.Equal("S3", all["ComponentModel:C"]);
    }

    #endregion

    #region DD-29: Resource Tracking - ResourceId Helper

    [Fact]
    public void ResourceId_ForGeneric_FormatsCorrectly()
    {
        var id = ResourceId.For<string>("Foo");

        Assert.Equal("String:Foo", id);
    }

    [Fact]
    public void ResourceId_ForObject_FormatsCorrectly()
    {
        var model = new object();
        var id = ResourceId.For(model, "Bar");

        Assert.Equal("Object:Bar", id);
    }

    #endregion
}
