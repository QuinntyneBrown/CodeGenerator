// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.React.Syntax;

namespace CodeGenerator.React.UnitTests;

public class ModelFactoryTests
{
    private readonly ModelFactory _factory = new();

    [Fact]
    public void CreateComponent_ReturnsComponentModel()
    {
        var model = _factory.CreateComponent("MyComponent");

        Assert.IsType<ComponentModel>(model);
    }

    [Fact]
    public void CreateComponent_SetsName()
    {
        var model = _factory.CreateComponent("MyComponent");

        Assert.Equal("MyComponent", model.Name);
    }

    [Fact]
    public void CreateComponent_InitializesEmptyProps()
    {
        var model = _factory.CreateComponent("MyComponent");

        Assert.NotNull(model.Props);
        Assert.Empty(model.Props);
    }

    [Fact]
    public void CreateComponent_InitializesEmptyHooks()
    {
        var model = _factory.CreateComponent("MyComponent");

        Assert.NotNull(model.Hooks);
        Assert.Empty(model.Hooks);
    }

    [Fact]
    public void CreateComponent_InitializesEmptyChildren()
    {
        var model = _factory.CreateComponent("MyComponent");

        Assert.NotNull(model.Children);
        Assert.Empty(model.Children);
    }

    [Fact]
    public void CreateComponent_InitializesEmptyImports()
    {
        var model = _factory.CreateComponent("MyComponent");

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void CreateComponent_DifferentNames_CreateDistinctInstances()
    {
        var model1 = _factory.CreateComponent("ComponentA");
        var model2 = _factory.CreateComponent("ComponentB");

        Assert.NotSame(model1, model2);
        Assert.Equal("ComponentA", model1.Name);
        Assert.Equal("ComponentB", model2.Name);
    }

    [Fact]
    public void CreateHook_ReturnsHookModel()
    {
        var model = _factory.CreateHook("useAuth");

        Assert.IsType<HookModel>(model);
    }

    [Fact]
    public void CreateHook_SetsName()
    {
        var model = _factory.CreateHook("useAuth");

        Assert.Equal("useAuth", model.Name);
    }

    [Fact]
    public void CreateHook_InitializesEmptyParams()
    {
        var model = _factory.CreateHook("useAuth");

        Assert.NotNull(model.Params);
        Assert.Empty(model.Params);
    }

    [Fact]
    public void CreateHook_InitializesEmptyImports()
    {
        var model = _factory.CreateHook("useAuth");

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void CreateHook_InitializesEmptyEffects()
    {
        var model = _factory.CreateHook("useAuth");

        Assert.NotNull(model.Effects);
        Assert.Empty(model.Effects);
    }

    [Fact]
    public void CreateHook_DifferentNames_CreateDistinctInstances()
    {
        var model1 = _factory.CreateHook("useAuth");
        var model2 = _factory.CreateHook("useCounter");

        Assert.NotSame(model1, model2);
        Assert.Equal("useAuth", model1.Name);
        Assert.Equal("useCounter", model2.Name);
    }

    [Fact]
    public void CreateStore_ReturnsStoreModel()
    {
        var model = _factory.CreateStore("appStore");

        Assert.IsType<StoreModel>(model);
    }

    [Fact]
    public void CreateStore_SetsName()
    {
        var model = _factory.CreateStore("appStore");

        Assert.Equal("appStore", model.Name);
    }

    [Fact]
    public void CreateStore_InitializesEmptyStateProperties()
    {
        var model = _factory.CreateStore("appStore");

        Assert.NotNull(model.StateProperties);
        Assert.Empty(model.StateProperties);
    }

    [Fact]
    public void CreateStore_InitializesEmptyActions()
    {
        var model = _factory.CreateStore("appStore");

        Assert.NotNull(model.Actions);
        Assert.Empty(model.Actions);
    }

    [Fact]
    public void CreateStore_InitializesEmptyActionImplementations()
    {
        var model = _factory.CreateStore("appStore");

        Assert.NotNull(model.ActionImplementations);
        Assert.Empty(model.ActionImplementations);
    }

    [Fact]
    public void CreateStore_DifferentNames_CreateDistinctInstances()
    {
        var model1 = _factory.CreateStore("userStore");
        var model2 = _factory.CreateStore("todoStore");

        Assert.NotSame(model1, model2);
        Assert.Equal("userStore", model1.Name);
        Assert.Equal("todoStore", model2.Name);
    }

    [Fact]
    public void ImplementsIModelFactory()
    {
        Assert.IsAssignableFrom<IModelFactory>(_factory);
    }
}
