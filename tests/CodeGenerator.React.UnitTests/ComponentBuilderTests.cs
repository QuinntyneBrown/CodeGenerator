// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.React.Builders;
using CodeGenerator.React.Syntax;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.React.UnitTests;

public class ComponentBuilderTests
{
    [Fact]
    public void For_CreatesBuilderWithName()
    {
        var model = ComponentBuilder.For("MyComponent").Build();

        Assert.Equal("MyComponent", model.Name);
    }

    [Fact]
    public void For_CreatesModelWithDefaultProperties()
    {
        var model = ComponentBuilder.For("MyComponent").Build();

        Assert.Empty(model.Props);
        Assert.Empty(model.Imports);
        Assert.False(model.IncludeChildren);
        Assert.False(model.UseMemo);
        Assert.Null(model.BodyContent);
    }

    [Fact]
    public void WithProp_AddsSingleProp()
    {
        var model = ComponentBuilder
            .For("MyComponent")
            .WithProp("title", "string")
            .Build();

        Assert.Single(model.Props);
        Assert.Equal("title", model.Props[0].Name);
        Assert.Equal("string", model.Props[0].Type.Name);
    }

    [Fact]
    public void WithProp_AddsMultipleProps()
    {
        var model = ComponentBuilder
            .For("MyComponent")
            .WithProp("title", "string")
            .WithProp("count", "number")
            .WithProp("isVisible", "boolean")
            .Build();

        Assert.Equal(3, model.Props.Count);
    }

    [Fact]
    public void WithProp_PreservesOrder()
    {
        var model = ComponentBuilder
            .For("MyComponent")
            .WithProp("first", "string")
            .WithProp("second", "number")
            .Build();

        Assert.Equal("first", model.Props[0].Name);
        Assert.Equal("second", model.Props[1].Name);
    }

    [Fact]
    public void WithChildren_SetsIncludeChildrenTrue()
    {
        var model = ComponentBuilder
            .For("MyComponent")
            .WithChildren()
            .Build();

        Assert.True(model.IncludeChildren);
    }

    [Fact]
    public void WithBody_SetsBodyContent()
    {
        var model = ComponentBuilder
            .For("MyComponent")
            .WithBody("<h1>Hello World</h1>")
            .Build();

        Assert.Equal("<h1>Hello World</h1>", model.BodyContent);
    }

    [Fact]
    public void WithImport_AddsSingleImport()
    {
        var model = ComponentBuilder
            .For("MyComponent")
            .WithImport("react", "useState")
            .Build();

        Assert.Single(model.Imports);
        Assert.Equal("react", model.Imports[0].Module);
        Assert.Single(model.Imports[0].Types);
        Assert.Equal("useState", model.Imports[0].Types[0].Name);
    }

    [Fact]
    public void WithImport_MultipleTypes_AddsAllTypes()
    {
        var model = ComponentBuilder
            .For("MyComponent")
            .WithImport("react", "useState", "useEffect", "useRef")
            .Build();

        Assert.Single(model.Imports);
        Assert.Equal(3, model.Imports[0].Types.Count);
        Assert.Equal("useState", model.Imports[0].Types[0].Name);
        Assert.Equal("useEffect", model.Imports[0].Types[1].Name);
        Assert.Equal("useRef", model.Imports[0].Types[2].Name);
    }

    [Fact]
    public void WithImport_MultipleModules_AddsMultipleImports()
    {
        var model = ComponentBuilder
            .For("MyComponent")
            .WithImport("react", "useState")
            .WithImport("react-router-dom", "useNavigate")
            .Build();

        Assert.Equal(2, model.Imports.Count);
        Assert.Equal("react", model.Imports[0].Module);
        Assert.Equal("react-router-dom", model.Imports[1].Module);
    }

    [Fact]
    public void WithMemo_SetsUseMemoTrue()
    {
        var model = ComponentBuilder
            .For("MyComponent")
            .WithMemo()
            .Build();

        Assert.True(model.UseMemo);
    }

    [Fact]
    public void FluentChain_AllMethods()
    {
        var model = ComponentBuilder
            .For("Dashboard")
            .WithProp("userId", "string")
            .WithProp("isAdmin", "boolean")
            .WithChildren()
            .WithBody("<div>{children}</div>")
            .WithImport("react", "useState", "useEffect")
            .WithMemo()
            .Build();

        Assert.Equal("Dashboard", model.Name);
        Assert.Equal(2, model.Props.Count);
        Assert.True(model.IncludeChildren);
        Assert.Equal("<div>{children}</div>", model.BodyContent);
        Assert.Single(model.Imports);
        Assert.True(model.UseMemo);
    }

    [Fact]
    public void Build_ReturnsComponentModel()
    {
        var model = ComponentBuilder.For("TestComponent").Build();

        Assert.IsType<ComponentModel>(model);
    }

    [Fact]
    public void Build_WithEmptyName_ThrowsModelValidationException()
    {
        Assert.Throws<ModelValidationException>(() =>
            ComponentBuilder.For("").Build());
    }

    [Fact]
    public void FluentChain_ReturnsSameBuilder()
    {
        var builder = ComponentBuilder.For("Test");
        var result = builder.WithProp("a", "string");

        Assert.Same(builder, result);
    }

    [Fact]
    public void WithChildren_FluentChain_ReturnsSameBuilder()
    {
        var builder = ComponentBuilder.For("Test");
        var result = builder.WithChildren();

        Assert.Same(builder, result);
    }

    [Fact]
    public void WithBody_FluentChain_ReturnsSameBuilder()
    {
        var builder = ComponentBuilder.For("Test");
        var result = builder.WithBody("content");

        Assert.Same(builder, result);
    }

    [Fact]
    public void WithMemo_FluentChain_ReturnsSameBuilder()
    {
        var builder = ComponentBuilder.For("Test");
        var result = builder.WithMemo();

        Assert.Same(builder, result);
    }

    [Fact]
    public void WithImport_FluentChain_ReturnsSameBuilder()
    {
        var builder = ComponentBuilder.For("Test");
        var result = builder.WithImport("react", "useState");

        Assert.Same(builder, result);
    }
}
