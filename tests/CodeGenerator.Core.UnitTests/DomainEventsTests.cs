// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Events;

namespace CodeGenerator.Core.UnitTests;

public class DomainEventsExtendedTests : IDisposable
{
    public DomainEventsTests()
    {
        DomainEvents.ClearCallbacks();
    }

    public void Dispose()
    {
        DomainEvents.ClearCallbacks();
    }

    [Fact]
    public void Register_AndRaise_InvokesCallback()
    {
        string? received = null;
        DomainEvents.Register<string>(x => received = x);

        DomainEvents.Raise("hello");

        Assert.Equal("hello", received);
    }

    [Fact]
    public void Raise_WithoutRegistration_DoesNotThrow()
    {
        DomainEvents.Raise("test");
    }

    [Fact]
    public void Register_SameType_ReplacesOldCallback()
    {
        string? first = null;
        string? second = null;

        DomainEvents.Register<string>(x => first = x);
        DomainEvents.Register<string>(x => second = x);

        DomainEvents.Raise("test");

        Assert.Null(first);
        Assert.Equal("test", second);
    }

    [Fact]
    public void ClearCallbacks_RemovesAll()
    {
        string? received = null;
        DomainEvents.Register<string>(x => received = x);

        DomainEvents.ClearCallbacks();
        DomainEvents.Raise("test");

        Assert.Null(received);
    }

    [Fact]
    public void Register_DifferentTypes_DoNotInterfere()
    {
        string? stringResult = null;
        int intResult = 0;

        DomainEvents.Register<string>(x => stringResult = x);
        DomainEvents.Register<CustomEvent<int>>(x => intResult = x.Payload);

        DomainEvents.Raise("hello");
        DomainEvents.Raise(new CustomEvent<int> { Payload = 42 });

        Assert.Equal("hello", stringResult);
        Assert.Equal(42, intResult);
    }
}
