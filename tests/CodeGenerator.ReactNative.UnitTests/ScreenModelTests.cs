// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Syntax;
using CodeGenerator.ReactNative.Syntax;

namespace CodeGenerator.ReactNative.UnitTests;

public class ScreenModelTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var model = new ScreenModel("HomeScreen");
        Assert.Equal("HomeScreen", model.Name);
    }

    [Fact]
    public void Constructor_InitializesEmptyCollections()
    {
        var model = new ScreenModel("Home");
        Assert.NotNull(model.Props);
        Assert.Empty(model.Props);
        Assert.NotNull(model.Hooks);
        Assert.Empty(model.Hooks);
        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
        Assert.NotNull(model.NavigationParams);
        Assert.Empty(model.NavigationParams);
    }

    [Fact]
    public void Validate_WithValidName_ReturnsValid()
    {
        var model = new ScreenModel("ProfileScreen");
        var result = model.Validate();
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsError()
    {
        var model = new ScreenModel("");
        var result = model.Validate();
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
        Assert.Equal("Screen name is required.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsError()
    {
        var model = new ScreenModel("  ");
        var result = model.Validate();
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithNullName_ReturnsError()
    {
        var model = new ScreenModel(null!);
        var result = model.Validate();
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Props_CanBePopulated()
    {
        var model = new ScreenModel("Detail");
        model.Props.Add(new PropertyModel { Name = "itemId", Type = new TypeModel("number") });
        Assert.Single(model.Props);
        Assert.Equal("itemId", model.Props[0].Name);
    }

    [Fact]
    public void Hooks_CanBePopulated()
    {
        var model = new ScreenModel("Home");
        model.Hooks.Add("useState");
        model.Hooks.Add("useEffect");
        Assert.Equal(2, model.Hooks.Count);
    }

    [Fact]
    public void Imports_CanBePopulated()
    {
        var model = new ScreenModel("Home");
        model.Imports.Add(new ImportModel("View", "react-native"));
        model.Imports.Add(new ImportModel("Text", "react-native"));
        Assert.Equal(2, model.Imports.Count);
    }

    [Fact]
    public void NavigationParams_CanBePopulated()
    {
        var model = new ScreenModel("Detail");
        model.NavigationParams.Add(new PropertyModel { Name = "userId", Type = new TypeModel("string") });
        model.NavigationParams.Add(new PropertyModel { Name = "postId", Type = new TypeModel("number") });
        Assert.Equal(2, model.NavigationParams.Count);
        Assert.Equal("userId", model.NavigationParams[0].Name);
    }

    [Fact]
    public void Name_CanBeSetAfterConstruction()
    {
        var model = new ScreenModel("Old");
        model.Name = "NewScreen";
        Assert.Equal("NewScreen", model.Name);
        Assert.True(model.Validate().IsValid);
    }

    [Fact]
    public void Name_SetToEmpty_FailsValidation()
    {
        var model = new ScreenModel("Valid");
        model.Name = "";
        Assert.False(model.Validate().IsValid);
    }

    [Fact]
    public void MultipleNavigationParams_CanBeAdded()
    {
        var model = new ScreenModel("Editor");
        model.NavigationParams.Add(new PropertyModel { Name = "documentId", Type = new TypeModel("string") });
        model.NavigationParams.Add(new PropertyModel { Name = "readOnly", Type = new TypeModel("boolean") });
        model.NavigationParams.Add(new PropertyModel { Name = "version", Type = new TypeModel("number") });
        Assert.Equal(3, model.NavigationParams.Count);
    }
}
