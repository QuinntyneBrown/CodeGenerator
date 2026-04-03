// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.ReactNative.Syntax;

namespace CodeGenerator.ReactNative.UnitTests;

public class NavigationModelTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var model = new NavigationModel("MainNav");
        Assert.Equal("MainNav", model.Name);
    }

    [Fact]
    public void Constructor_DefaultsToStackNavigator()
    {
        var model = new NavigationModel("MainNav");
        Assert.Equal("stack", model.NavigatorType);
    }

    [Fact]
    public void Constructor_WithNavigatorType_SetsType()
    {
        var model = new NavigationModel("TabNav", "tab");
        Assert.Equal("TabNav", model.Name);
        Assert.Equal("tab", model.NavigatorType);
    }

    [Fact]
    public void Constructor_InitializesEmptyScreens()
    {
        var model = new NavigationModel("Nav");
        Assert.NotNull(model.Screens);
        Assert.Empty(model.Screens);
    }

    [Fact]
    public void Validate_WithValidNameAndScreens_ReturnsValid()
    {
        var model = new NavigationModel("AppNav");
        model.Screens.Add("HomeScreen");
        var result = model.Validate();
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsError()
    {
        var model = new NavigationModel("");
        model.Screens.Add("HomeScreen");
        var result = model.Validate();
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Navigation name is required.");
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsError()
    {
        var model = new NavigationModel("   ");
        model.Screens.Add("HomeScreen");
        var result = model.Validate();
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithNullName_ReturnsError()
    {
        var model = new NavigationModel(null!);
        model.Screens.Add("Home");
        var result = model.Validate();
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithNoScreens_ReturnsError()
    {
        var model = new NavigationModel("AppNav");
        var result = model.Validate();
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Screens");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "At least one screen is required.");
    }

    [Fact]
    public void Validate_WithNullScreens_ReturnsError()
    {
        var model = new NavigationModel("AppNav");
        model.Screens = null!;
        var result = model.Validate();
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Screens");
    }

    [Fact]
    public void Validate_WithEmptyNameAndNoScreens_ReturnsTwoErrors()
    {
        var model = new NavigationModel("");
        var result = model.Validate();
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void Screens_CanBePopulated()
    {
        var model = new NavigationModel("AppNav");
        model.Screens.Add("HomeScreen");
        model.Screens.Add("ProfileScreen");
        model.Screens.Add("SettingsScreen");
        Assert.Equal(3, model.Screens.Count);
    }

    [Fact]
    public void NavigatorType_CanBeChanged()
    {
        var model = new NavigationModel("Nav");
        model.NavigatorType = "drawer";
        Assert.Equal("drawer", model.NavigatorType);
    }

    [Fact]
    public void Validate_WithSingleScreen_ReturnsValid()
    {
        var model = new NavigationModel("SimpleNav");
        model.Screens.Add("OnlyScreen");
        var result = model.Validate();
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Name_CanBeSetAfterConstruction()
    {
        var model = new NavigationModel("Old");
        model.Name = "NewNav";
        Assert.Equal("NewNav", model.Name);
    }
}
