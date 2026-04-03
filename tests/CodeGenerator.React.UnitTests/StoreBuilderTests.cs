// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.React.Builders;
using CodeGenerator.React.Syntax;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.React.UnitTests;

public class StoreBuilderTests
{
    [Fact]
    public void For_CreatesBuilderWithName()
    {
        var model = StoreBuilder.For("counterStore").Build();

        Assert.Equal("counterStore", model.Name);
    }

    [Fact]
    public void For_CreatesModelWithEmptyCollections()
    {
        var model = StoreBuilder.For("counterStore").Build();

        Assert.Empty(model.StateProperties);
        Assert.Empty(model.Actions);
        Assert.Empty(model.ActionImplementations);
    }

    [Fact]
    public void WithState_AddsStateProperty()
    {
        var model = StoreBuilder
            .For("counterStore")
            .WithState("count", "number", "0")
            .Build();

        Assert.Single(model.StateProperties);
        Assert.Equal("count", model.StateProperties[0].Name);
        Assert.Equal("number", model.StateProperties[0].Type.Name);
    }

    [Fact]
    public void WithState_AddsDefaultValueToActionImplementations()
    {
        var model = StoreBuilder
            .For("counterStore")
            .WithState("count", "number", "0")
            .Build();

        Assert.True(model.ActionImplementations.ContainsKey("count"));
        Assert.Equal("0", model.ActionImplementations["count"]);
    }

    [Fact]
    public void WithState_MultipleStates()
    {
        var model = StoreBuilder
            .For("appStore")
            .WithState("count", "number", "0")
            .WithState("name", "string", "''")
            .WithState("isLoading", "boolean", "false")
            .Build();

        Assert.Equal(3, model.StateProperties.Count);
        Assert.Equal(3, model.ActionImplementations.Count);
    }

    [Fact]
    public void WithAction_AddsActionName()
    {
        var model = StoreBuilder
            .For("counterStore")
            .WithAction("increment", "set({ count: get().count + 1 })")
            .Build();

        Assert.Single(model.Actions);
        Assert.Equal("increment", model.Actions[0]);
    }

    [Fact]
    public void WithAction_AddsBodyToActionImplementations()
    {
        var model = StoreBuilder
            .For("counterStore")
            .WithAction("increment", "set({ count: get().count + 1 })")
            .Build();

        Assert.True(model.ActionImplementations.ContainsKey("increment"));
        Assert.Equal("set({ count: get().count + 1 })", model.ActionImplementations["increment"]);
    }

    [Fact]
    public void WithAction_MultipleActions()
    {
        var model = StoreBuilder
            .For("counterStore")
            .WithAction("increment", "set({ count: get().count + 1 })")
            .WithAction("decrement", "set({ count: get().count - 1 })")
            .WithAction("reset", "set({ count: 0 })")
            .Build();

        Assert.Equal(3, model.Actions.Count);
        Assert.Equal(3, model.ActionImplementations.Count);
    }

    [Fact]
    public void FluentChain_StateAndActions()
    {
        var model = StoreBuilder
            .For("todoStore")
            .WithState("items", "Todo[]", "[]")
            .WithState("filter", "string", "'all'")
            .WithAction("addTodo", "set({ items: [...get().items, todo] })")
            .WithAction("removeTodo", "set({ items: get().items.filter(t => t.id !== id) })")
            .Build();

        Assert.Equal("todoStore", model.Name);
        Assert.Equal(2, model.StateProperties.Count);
        Assert.Equal(2, model.Actions.Count);
        Assert.Equal(4, model.ActionImplementations.Count);
    }

    [Fact]
    public void Build_ReturnsStoreModel()
    {
        var model = StoreBuilder.For("testStore").Build();

        Assert.IsType<StoreModel>(model);
    }

    [Fact]
    public void Build_WithEmptyName_ThrowsModelValidationException()
    {
        Assert.Throws<ModelValidationException>(() =>
            StoreBuilder.For("").Build());
    }

    [Fact]
    public void FluentChain_WithState_ReturnsSameBuilder()
    {
        var builder = StoreBuilder.For("test");
        var result = builder.WithState("count", "number", "0");

        Assert.Same(builder, result);
    }

    [Fact]
    public void FluentChain_WithAction_ReturnsSameBuilder()
    {
        var builder = StoreBuilder.For("test");
        var result = builder.WithAction("increment", "body");

        Assert.Same(builder, result);
    }

    [Fact]
    public void WithAction_PreservesOrder()
    {
        var model = StoreBuilder
            .For("store")
            .WithAction("first", "body1")
            .WithAction("second", "body2")
            .WithAction("third", "body3")
            .Build();

        Assert.Equal("first", model.Actions[0]);
        Assert.Equal("second", model.Actions[1]);
        Assert.Equal("third", model.Actions[2]);
    }
}
