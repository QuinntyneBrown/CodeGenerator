// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Events;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CodeGenerator.Core.UnitTests;

public class DomainEventsTests
{
    [Fact]
    public void Register_And_Raise_InvokesCallback()
    {
        DomainEvents.ClearCallbacks();

        string? receivedValue = null;
        DomainEvents.Register<CustomEvent<string>>(e => receivedValue = e.Payload);

        var @event = new CustomEvent<string> { Payload = "test" };
        DomainEvents.Raise(@event);

        Assert.Equal("test", receivedValue);
    }

    [Fact]
    public void Register_ReplacesExistingCallback()
    {
        DomainEvents.ClearCallbacks();

        string? value1 = null;
        string? value2 = null;
        DomainEvents.Register<CustomEvent<string>>(e => value1 = e.Payload);
        DomainEvents.Register<CustomEvent<string>>(e => value2 = e.Payload);

        DomainEvents.Raise(new CustomEvent<string> { Payload = "test" });

        Assert.Null(value1);
        Assert.Equal("test", value2);
    }

    [Fact]
    public void ClearCallbacks_RemovesAllRegistrations()
    {
        DomainEvents.ClearCallbacks();

        string? value = null;
        DomainEvents.Register<CustomEvent<string>>(e => value = e.Payload);
        DomainEvents.ClearCallbacks();

        DomainEvents.Raise(new CustomEvent<string> { Payload = "test" });

        Assert.Null(value);
    }
}

public class ContextTests
{
    [Fact]
    public void SetAndGet_RoundTripsValue()
    {
        DomainEvents.ClearCallbacks();

        var context = new Context(NullLogger<Context>.Instance);
        context.Set("hello");

        var result = context.Get<string>();

        Assert.Equal("hello", result);
    }
}

public class ObjectCacheTests
{
    [Fact]
    public void FromCacheOrService_CachesMissResult()
    {
        var cache = new ObjectCache(NullLogger<ObjectCache>.Instance);
        int callCount = 0;

        var result1 = cache.FromCacheOrService(() => { callCount++; return "value"; }, "key1");
        var result2 = cache.FromCacheOrService(() => { callCount++; return "value2"; }, "key1");

        Assert.Equal("value", result1);
        Assert.Equal("value", result2);
        Assert.Equal(1, callCount);
    }
}

public class CommandServiceTests
{
    [Fact]
    public void NoOpCommandService_ReturnsZero()
    {
        var service = new NoOpCommandService();
        var result = service.Start("echo hello");
        Assert.Equal(0, result);
    }

    [Fact]
    public void CommandService_ExecutesCrossplatform()
    {
        var service = new CommandService(NullLogger<CommandService>.Instance);
        var result = service.Start("echo hello");
        Assert.Equal(0, result);
    }
}

public class SyntaxModelTests
{
    private class TestSyntax : CodeGenerator.Core.Syntax.SyntaxModel
    {
        public string Label { get; set; } = string.Empty;
        public List<CodeGenerator.Core.Syntax.SyntaxModel> Kids { get; set; } = new();

        public override IEnumerable<CodeGenerator.Core.Syntax.SyntaxModel> GetChildren() => Kids;
    }

    [Fact]
    public void GetDescendants_ReturnsAllNodes()
    {
        var grandchild = new TestSyntax { Label = "GC" };
        var child = new TestSyntax { Label = "C", Kids = [grandchild] };
        var root = new TestSyntax { Label = "R", Kids = [child] };

        var all = root.GetDescendants();

        Assert.Equal(3, all.Count);
        Assert.Contains(root, all);
        Assert.Contains(child, all);
        Assert.Contains(grandchild, all);
    }
}
