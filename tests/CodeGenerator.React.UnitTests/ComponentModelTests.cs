// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.React.Syntax;
using CodeGenerator.Core.Syntax;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.React.UnitTests;

public class ComponentModelTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var model = new ComponentModel("MyComponent");

        Assert.Equal("MyComponent", model.Name);
    }

    [Fact]
    public void Constructor_InitializesEmptyProps()
    {
        var model = new ComponentModel("MyComponent");

        Assert.NotNull(model.Props);
        Assert.Empty(model.Props);
    }

    [Fact]
    public void Constructor_InitializesEmptyHooks()
    {
        var model = new ComponentModel("MyComponent");

        Assert.NotNull(model.Hooks);
        Assert.Empty(model.Hooks);
    }

    [Fact]
    public void Constructor_InitializesEmptyChildren()
    {
        var model = new ComponentModel("MyComponent");

        Assert.NotNull(model.Children);
        Assert.Empty(model.Children);
    }

    [Fact]
    public void Constructor_InitializesEmptyImports()
    {
        var model = new ComponentModel("MyComponent");

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void IsClient_DefaultsFalse()
    {
        var model = new ComponentModel("MyComponent");

        Assert.False(model.IsClient);
    }

    [Fact]
    public void ComponentStyle_DefaultsToForwardRef()
    {
        var model = new ComponentModel("MyComponent");

        Assert.Equal("forwardRef", model.ComponentStyle);
    }

    [Fact]
    public void BodyContent_DefaultsToNull()
    {
        var model = new ComponentModel("MyComponent");

        Assert.Null(model.BodyContent);
    }

    [Fact]
    public void ExportDefault_DefaultsFalse()
    {
        var model = new ComponentModel("MyComponent");

        Assert.False(model.ExportDefault);
    }

    [Fact]
    public void IncludeChildren_DefaultsFalse()
    {
        var model = new ComponentModel("MyComponent");

        Assert.False(model.IncludeChildren);
    }

    [Fact]
    public void RefElementType_DefaultsToHtmlDivElement()
    {
        var model = new ComponentModel("MyComponent");

        Assert.Equal("HTMLDivElement", model.RefElementType);
    }

    [Fact]
    public void UseMemo_DefaultsFalse()
    {
        var model = new ComponentModel("MyComponent");

        Assert.False(model.UseMemo);
    }

    [Fact]
    public void SpreadProps_DefaultsFalse()
    {
        var model = new ComponentModel("MyComponent");

        Assert.False(model.SpreadProps);
    }

    [Fact]
    public void Validate_ValidName_ReturnsValid()
    {
        var model = new ComponentModel("MyComponent");

        var result = model.Validate();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var model = new ComponentModel("temp");
        model.Name = "";

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
        Assert.Contains("required", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Validate_NullName_ReturnsError()
    {
        var model = new ComponentModel("temp");
        model.Name = null!;

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhitespaceName_ReturnsError()
    {
        var model = new ComponentModel("temp");
        model.Name = "   ";

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Props_CanAddProperty()
    {
        var model = new ComponentModel("MyComponent");
        model.Props.Add(new PropertyModel
        {
            Name = "title",
            Type = new TypeModel("string")
        });

        Assert.Single(model.Props);
        Assert.Equal("title", model.Props[0].Name);
    }

    [Fact]
    public void ComponentStyle_CanBeChanged()
    {
        var model = new ComponentModel("MyComponent");
        model.ComponentStyle = "fc";

        Assert.Equal("fc", model.ComponentStyle);
    }

    [Fact]
    public void BodyContent_CanBeSet()
    {
        var model = new ComponentModel("MyComponent");
        model.BodyContent = "<div>Hello</div>";

        Assert.Equal("<div>Hello</div>", model.BodyContent);
    }

    [Fact]
    public void InheritsFromSyntaxModel()
    {
        var model = new ComponentModel("Test");

        Assert.IsAssignableFrom<SyntaxModel>(model);
    }
}
