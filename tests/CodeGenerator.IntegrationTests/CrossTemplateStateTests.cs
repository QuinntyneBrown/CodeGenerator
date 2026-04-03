// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using IGenerationContext = CodeGenerator.Core.Services.IGenerationContext;

namespace CodeGenerator.IntegrationTests;

public class CrossTemplateStateTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public CrossTemplateStateTests()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddCoreServices(typeof(CrossTemplateStateTests).Assembly);
        services.AddDotNetServices();

        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    #region DD-28: Cross-Template State - Push/GetStack

    [Fact]
    public void Push_GetStack_RoundTrip()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IGenerationContext>();

        context.Push("items", 1);
        context.Push("items", 2);
        context.Push("items", 3);

        var stack = context.GetStack("items");

        Assert.Equal(3, stack.Count);
        Assert.Equal(1, stack[0]);
        Assert.Equal(2, stack[1]);
        Assert.Equal(3, stack[2]);
    }

    [Fact]
    public void GetStack_MissingName_ReturnsEmpty()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IGenerationContext>();

        var stack = context.GetStack("nonexistent");

        Assert.NotNull(stack);
        Assert.Empty(stack);
    }

    #endregion

    #region DD-28: Cross-Template State - Set/Get/TryGet

    [Fact]
    public void Set_Get_RoundTrip()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IGenerationContext>();

        context.Set("projectName", "MyApp");

        var value = context.Get<string>("projectName");

        Assert.Equal("MyApp", value);
    }

    [Fact]
    public void Get_MissingKey_ThrowsKeyNotFound()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IGenerationContext>();

        Assert.Throws<KeyNotFoundException>(() => context.Get<string>("missing"));
    }

    [Fact]
    public void TryGet_MissingKey_ReturnsFalse()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IGenerationContext>();

        var found = context.TryGet<string>("missing", out var value);

        Assert.False(found);
        Assert.Null(value);
    }

    [Fact]
    public void TryGet_ExistingKey_ReturnsTrue()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IGenerationContext>();

        context.Set("count", 42);

        var found = context.TryGet<int>("count", out var value);

        Assert.True(found);
        Assert.Equal(42, value);
    }

    #endregion

    #region DD-28: Cross-Template State - Generated Files

    [Fact]
    public void RecordGeneratedFile_AppearsInList()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IGenerationContext>();

        context.RecordGeneratedFile("src/Domain/Order.cs", "ClassFileStrategy");
        context.RecordGeneratedFile("src/Domain/Customer.cs", "ClassFileStrategy");

        var files = context.GeneratedFiles;

        Assert.Equal(2, files.Count);
        Assert.Equal("src/Domain/Order.cs", files[0].Path);
        Assert.Equal("ClassFileStrategy", files[0].GeneratorName);
        Assert.Equal("src/Domain/Customer.cs", files[1].Path);
    }

    [Fact]
    public void GeneratedFiles_InitiallyEmpty()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IGenerationContext>();

        Assert.Empty(context.GeneratedFiles);
    }

    #endregion

    #region DD-28: Cross-Template State - Thread Safety

    [Fact]
    public void Push_ThreadSafe_ParallelWrites()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IGenerationContext>();

        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() => context.Push("parallel", i)))
            .ToArray();

        Task.WaitAll(tasks);

        var stack = context.GetStack("parallel");

        Assert.Equal(100, stack.Count);
        // All values 0-99 should be present (order may vary)
        var values = stack.Cast<int>().OrderBy(x => x).ToList();
        Assert.Equal(Enumerable.Range(0, 100).ToList(), values);
    }

    [Fact]
    public void RecordGeneratedFile_ThreadSafe_ParallelWrites()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IGenerationContext>();

        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() => context.RecordGeneratedFile($"file{i}.cs", "Strategy")))
            .ToArray();

        Task.WaitAll(tasks);

        Assert.Equal(100, context.GeneratedFiles.Count);
    }

    #endregion

    #region DD-28: Cross-Template State - Scoped Isolation

    [Fact]
    public void ScopedIsolation_IndependentContexts()
    {
        using var scope1 = _serviceProvider.CreateScope();
        using var scope2 = _serviceProvider.CreateScope();

        var context1 = scope1.ServiceProvider.GetRequiredService<IGenerationContext>();
        var context2 = scope2.ServiceProvider.GetRequiredService<IGenerationContext>();

        context1.Set("key", "value1");
        context2.Set("key", "value2");

        Assert.Equal("value1", context1.Get<string>("key"));
        Assert.Equal("value2", context2.Get<string>("key"));
    }

    [Fact]
    public void ScopedIsolation_FilesNotShared()
    {
        using var scope1 = _serviceProvider.CreateScope();
        using var scope2 = _serviceProvider.CreateScope();

        var context1 = scope1.ServiceProvider.GetRequiredService<IGenerationContext>();
        var context2 = scope2.ServiceProvider.GetRequiredService<IGenerationContext>();

        context1.RecordGeneratedFile("a.cs", "S1");

        Assert.Single(context1.GeneratedFiles);
        Assert.Empty(context2.GeneratedFiles);
    }

    #endregion
}
