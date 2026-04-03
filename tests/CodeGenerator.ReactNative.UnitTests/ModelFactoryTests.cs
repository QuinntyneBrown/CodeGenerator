// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.ReactNative.Syntax;

namespace CodeGenerator.ReactNative.UnitTests;

public class ModelFactoryTests
{
    private readonly ModelFactory _factory = new();

    [Fact]
    public void CreateScreen_ReturnsScreenModelWithName()
    {
        var model = _factory.CreateScreen("HomeScreen");
        Assert.NotNull(model);
        Assert.Equal("HomeScreen", model.Name);
        Assert.IsType<ScreenModel>(model);
    }

    [Fact]
    public void CreateScreen_InitializesEmptyCollections()
    {
        var model = _factory.CreateScreen("Home");
        Assert.Empty(model.Props);
        Assert.Empty(model.Hooks);
        Assert.Empty(model.Imports);
        Assert.Empty(model.NavigationParams);
    }

    [Fact]
    public void CreateScreen_ReturnsValidModel()
    {
        var model = _factory.CreateScreen("ValidScreen");
        var result = model.Validate();
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateComponent_ReturnsComponentModelWithName()
    {
        var model = _factory.CreateComponent("Button");
        Assert.NotNull(model);
        Assert.Equal("Button", model.Name);
        Assert.IsType<ComponentModel>(model);
    }

    [Fact]
    public void CreateComponent_InitializesEmptyCollections()
    {
        var model = _factory.CreateComponent("Card");
        Assert.Empty(model.Props);
        Assert.Empty(model.Styles);
        Assert.Empty(model.Children);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void CreateComponent_ReturnsValidModel()
    {
        var model = _factory.CreateComponent("ValidComponent");
        var result = model.Validate();
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateNavigation_ReturnsNavigationModelWithName()
    {
        var model = _factory.CreateNavigation("AppNav");
        Assert.NotNull(model);
        Assert.Equal("AppNav", model.Name);
        Assert.IsType<NavigationModel>(model);
    }

    [Fact]
    public void CreateNavigation_DefaultsToStackType()
    {
        var model = _factory.CreateNavigation("Nav");
        Assert.Equal("stack", model.NavigatorType);
    }

    [Fact]
    public void CreateNavigation_WithCustomType()
    {
        var model = _factory.CreateNavigation("TabNav", "tab");
        Assert.Equal("TabNav", model.Name);
        Assert.Equal("tab", model.NavigatorType);
    }

    [Fact]
    public void CreateNavigation_WithDrawerType()
    {
        var model = _factory.CreateNavigation("DrawerNav", "drawer");
        Assert.Equal("drawer", model.NavigatorType);
    }

    [Fact]
    public void CreateNavigation_InitializesEmptyScreens()
    {
        var model = _factory.CreateNavigation("Nav");
        Assert.NotNull(model.Screens);
        Assert.Empty(model.Screens);
    }

    [Fact]
    public void ImplementsIModelFactory()
    {
        Assert.IsAssignableFrom<IModelFactory>(_factory);
    }

    [Fact]
    public void CreateScreen_DifferentNames_ReturnDifferentModels()
    {
        var model1 = _factory.CreateScreen("Home");
        var model2 = _factory.CreateScreen("Profile");
        Assert.NotSame(model1, model2);
        Assert.NotEqual(model1.Name, model2.Name);
    }

    [Fact]
    public void CreateComponent_DifferentNames_ReturnDifferentModels()
    {
        var model1 = _factory.CreateComponent("Header");
        var model2 = _factory.CreateComponent("Footer");
        Assert.NotSame(model1, model2);
        Assert.NotEqual(model1.Name, model2.Name);
    }

    [Fact]
    public void CreateNavigation_DifferentNames_ReturnDifferentModels()
    {
        var model1 = _factory.CreateNavigation("Nav1");
        var model2 = _factory.CreateNavigation("Nav2");
        Assert.NotSame(model1, model2);
        Assert.NotEqual(model1.Name, model2.Name);
    }
}
