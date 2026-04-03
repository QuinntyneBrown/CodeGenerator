// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Events;

namespace CodeGenerator.Core.UnitTests;

public class CustomEventTests
{
    [Fact]
    public void DefaultConstructor_CreatesInstance()
    {
        var evt = new CustomEvent<string>();
        Assert.NotNull(evt);
    }

    [Fact]
    public void Name_CanBeSetAndRetrieved()
    {
        var evt = new CustomEvent<string> { Name = "TestEvent" };
        Assert.Equal("TestEvent", evt.Name);
    }

    [Fact]
    public void Payload_CanBeSetAndRetrieved()
    {
        var evt = new CustomEvent<int> { Payload = 42 };
        Assert.Equal(42, evt.Payload);
    }

    [Fact]
    public void Payload_DefaultsToDefault()
    {
        var evt = new CustomEvent<string>();
        Assert.Null(evt.Payload);
    }

    [Fact]
    public void Payload_DefaultsToDefaultForValueType()
    {
        var evt = new CustomEvent<int>();
        Assert.Equal(0, evt.Payload);
    }

    [Fact]
    public void Name_DefaultsToNull()
    {
        var evt = new CustomEvent<string>();
        Assert.Null(evt.Name);
    }

    [Fact]
    public void Payload_ComplexType()
    {
        var data = new List<string> { "a", "b" };
        var evt = new CustomEvent<List<string>> { Payload = data };
        Assert.Same(data, evt.Payload);
    }
}
