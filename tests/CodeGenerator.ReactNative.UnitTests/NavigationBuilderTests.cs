// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.ReactNative.Builders;
using CodeGenerator.ReactNative.Syntax;

namespace CodeGenerator.ReactNative.UnitTests;

public class NavigationBuilderTests
{
    [Fact]
    public void For_SetsName()
    {
        var model = NavigationBuilder.For("AppNav")
            .WithScreen("Home")
            .Build();
        Assert.Equal("AppNav", model.Name);
    }

    [Fact]
    public void For_DefaultsToStackNavigator()
    {
        var model = NavigationBuilder.For("Nav")
            .WithScreen("Home")
            .Build();
        Assert.Equal("stack", model.NavigatorType);
    }

    [Fact]
    public void WithScreen_AddsSingleScreen()
    {
        var model = NavigationBuilder.For("Nav")
            .WithScreen("HomeScreen")
            .Build();
        Assert.Single(model.Screens);
        Assert.Equal("HomeScreen", model.Screens[0]);
    }

    [Fact]
    public void WithScreen_AddsMultipleScreens()
    {
        var model = NavigationBuilder.For("Nav")
            .WithScreen("Home")
            .WithScreen("Profile")
            .WithScreen("Settings")
            .Build();
        Assert.Equal(3, model.Screens.Count);
        Assert.Equal("Home", model.Screens[0]);
        Assert.Equal("Profile", model.Screens[1]);
        Assert.Equal("Settings", model.Screens[2]);
    }

    [Fact]
    public void AsStack_SetsNavigatorTypeToStack()
    {
        var model = NavigationBuilder.For("Nav")
            .WithScreen("Home")
            .AsStack()
            .Build();
        Assert.Equal("stack", model.NavigatorType);
    }

    [Fact]
    public void AsTab_SetsNavigatorTypeToTab()
    {
        var model = NavigationBuilder.For("TabNav")
            .WithScreen("Home")
            .AsTab()
            .Build();
        Assert.Equal("tab", model.NavigatorType);
    }

    [Fact]
    public void AsDrawer_SetsNavigatorTypeToDrawer()
    {
        var model = NavigationBuilder.For("DrawerNav")
            .WithScreen("Home")
            .AsDrawer()
            .Build();
        Assert.Equal("drawer", model.NavigatorType);
    }

    [Fact]
    public void NavigatorType_CanBeChangedMultipleTimes()
    {
        var model = NavigationBuilder.For("Nav")
            .WithScreen("Home")
            .AsStack()
            .AsTab()
            .AsDrawer()
            .Build();
        Assert.Equal("drawer", model.NavigatorType);
    }

    [Fact]
    public void AsTab_OverridesDefaultStack()
    {
        var model = NavigationBuilder.For("Nav")
            .WithScreen("Home")
            .AsTab()
            .Build();
        Assert.Equal("tab", model.NavigatorType);
    }

    [Fact]
    public void FluentChaining_BuildsCompleteModel()
    {
        var model = NavigationBuilder.For("MainNavigation")
            .WithScreen("HomeScreen")
            .WithScreen("ProfileScreen")
            .WithScreen("SettingsScreen")
            .AsTab()
            .Build();

        Assert.Equal("MainNavigation", model.Name);
        Assert.Equal(3, model.Screens.Count);
        Assert.Equal("tab", model.NavigatorType);
    }

    [Fact]
    public void Build_ReturnsNavigationModel()
    {
        var model = NavigationBuilder.For("Nav")
            .WithScreen("Home")
            .Build();
        Assert.IsType<NavigationModel>(model);
    }

    [Fact]
    public void WithScreen_PreservesOrder()
    {
        var model = NavigationBuilder.For("Nav")
            .WithScreen("First")
            .WithScreen("Second")
            .WithScreen("Third")
            .Build();
        Assert.Equal("First", model.Screens[0]);
        Assert.Equal("Second", model.Screens[1]);
        Assert.Equal("Third", model.Screens[2]);
    }

    [Fact]
    public void AsDrawer_WithMultipleScreens_BuildsCorrectly()
    {
        var model = NavigationBuilder.For("SideMenu")
            .AsDrawer()
            .WithScreen("Dashboard")
            .WithScreen("Reports")
            .Build();
        Assert.Equal("drawer", model.NavigatorType);
        Assert.Equal(2, model.Screens.Count);
    }

    [Fact]
    public void AsStack_AfterAsTab_OverridesType()
    {
        var model = NavigationBuilder.For("Nav")
            .WithScreen("Home")
            .AsTab()
            .AsStack()
            .Build();
        Assert.Equal("stack", model.NavigatorType);
    }
}
