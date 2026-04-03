// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Syntax;
using CodeGenerator.ReactNative.Syntax;

namespace CodeGenerator.ReactNative.UnitTests;

public class ComponentModelTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var model = new ComponentModel("Button");
        Assert.Equal("Button", model.Name);
    }

    [Fact]
    public void Constructor_InitializesEmptyCollections()
    {
        var model = new ComponentModel("Card");
        Assert.NotNull(model.Props);
        Assert.Empty(model.Props);
        Assert.NotNull(model.Styles);
        Assert.Empty(model.Styles);
        Assert.NotNull(model.Children);
        Assert.Empty(model.Children);
        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void Validate_WithValidName_ReturnsValid()
    {
        var model = new ComponentModel("Header");
        var result = model.Validate();
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsError()
    {
        var model = new ComponentModel("");
        var result = model.Validate();
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
        Assert.Equal("Component name is required.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsError()
    {
        var model = new ComponentModel("   ");
        var result = model.Validate();
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithNullName_ReturnsError()
    {
        var model = new ComponentModel(null!);
        var result = model.Validate();
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Props_CanBePopulated()
    {
        var model = new ComponentModel("Input");
        model.Props.Add(new PropertyModel
        {
            Name = "placeholder",
            Type = new TypeModel("string")
        });
        Assert.Single(model.Props);
        Assert.Equal("placeholder", model.Props[0].Name);
    }

    [Fact]
    public void Styles_CanBePopulated()
    {
        var model = new ComponentModel("Box");
        model.Styles.Add(new StyleModel("container"));
        Assert.Single(model.Styles);
        Assert.Equal("container", model.Styles[0].Name);
    }

    [Fact]
    public void Children_CanBePopulated()
    {
        var model = new ComponentModel("Layout");
        model.Children.Add("Header");
        model.Children.Add("Footer");
        Assert.Equal(2, model.Children.Count);
        Assert.Equal("Header", model.Children[0]);
        Assert.Equal("Footer", model.Children[1]);
    }

    [Fact]
    public void Imports_CanBePopulated()
    {
        var model = new ComponentModel("App");
        model.Imports.Add(new ImportModel("View", "react-native"));
        Assert.Single(model.Imports);
        Assert.Equal("react-native", model.Imports[0].Module);
    }

    [Fact]
    public void Name_CanBeSetAfterConstruction()
    {
        var model = new ComponentModel("Old");
        model.Name = "New";
        Assert.Equal("New", model.Name);
        Assert.True(model.Validate().IsValid);
    }

    [Fact]
    public void Name_SetToEmpty_FailsValidation()
    {
        var model = new ComponentModel("Valid");
        model.Name = "";
        Assert.False(model.Validate().IsValid);
    }

    [Fact]
    public void MultipleProps_CanBeAdded()
    {
        var model = new ComponentModel("Form");
        model.Props.Add(new PropertyModel { Name = "onSubmit", Type = new TypeModel("Function") });
        model.Props.Add(new PropertyModel { Name = "initialValues", Type = new TypeModel("object") });
        model.Props.Add(new PropertyModel { Name = "disabled", Type = new TypeModel("boolean") });
        Assert.Equal(3, model.Props.Count);
    }
}
