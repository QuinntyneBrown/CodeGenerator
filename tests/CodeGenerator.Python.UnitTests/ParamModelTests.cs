// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Python.Syntax;

namespace CodeGenerator.Python.UnitTests;

public class ParamModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var model = new ParamModel();
        Assert.Equal(string.Empty, model.Name);
        Assert.Null(model.TypeHint);
        Assert.Null(model.DefaultValue);
    }

    [Fact]
    public void NameConstructor_SetsName()
    {
        var model = new ParamModel("x");
        Assert.Equal("x", model.Name);
        Assert.Null(model.TypeHint);
        Assert.Null(model.DefaultValue);
    }

    [Fact]
    public void FullConstructor_SetsAllProperties()
    {
        var typeHint = new TypeHintModel("int");
        var model = new ParamModel("count", typeHint, "0");
        Assert.Equal("count", model.Name);
        Assert.NotNull(model.TypeHint);
        Assert.Equal("int", model.TypeHint.Name);
        Assert.Equal("0", model.DefaultValue);
    }

    [Fact]
    public void Constructor_WithTypeHintOnly()
    {
        var model = new ParamModel("name", new TypeHintModel("str"));
        Assert.Equal("name", model.Name);
        Assert.NotNull(model.TypeHint);
        Assert.Equal("str", model.TypeHint.Name);
        Assert.Null(model.DefaultValue);
    }

    [Fact]
    public void Constructor_WithDefaultValueOnly()
    {
        var model = new ParamModel("flag", null, "True");
        Assert.Equal("flag", model.Name);
        Assert.Null(model.TypeHint);
        Assert.Equal("True", model.DefaultValue);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        var model = new ParamModel();
        model.Name = "updated";
        Assert.Equal("updated", model.Name);
    }

    [Fact]
    public void TypeHint_CanBeSet()
    {
        var model = new ParamModel("x");
        model.TypeHint = new TypeHintModel("float");
        Assert.NotNull(model.TypeHint);
        Assert.Equal("float", model.TypeHint.Name);
    }

    [Fact]
    public void DefaultValue_CanBeSet()
    {
        var model = new ParamModel("x");
        model.DefaultValue = "None";
        Assert.Equal("None", model.DefaultValue);
    }

    [Fact]
    public void TypeHint_CanBeSetToNull()
    {
        var model = new ParamModel("x", new TypeHintModel("int"));
        model.TypeHint = null;
        Assert.Null(model.TypeHint);
    }
}
