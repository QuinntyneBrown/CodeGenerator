// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Python.Syntax;

namespace CodeGenerator.Python.UnitTests;

public class DecoratorModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var model = new DecoratorModel();
        Assert.Equal(string.Empty, model.Name);
        Assert.NotNull(model.Arguments);
        Assert.Empty(model.Arguments);
    }

    [Fact]
    public void NameConstructor_SetsName()
    {
        var model = new DecoratorModel("property");
        Assert.Equal("property", model.Name);
        Assert.NotNull(model.Arguments);
        Assert.Empty(model.Arguments);
    }

    [Fact]
    public void NameAndArgumentsConstructor_SetsBoth()
    {
        var args = new List<string> { "methods=['GET', 'POST']" };
        var model = new DecoratorModel("app.route", args);
        Assert.Equal("app.route", model.Name);
        Assert.Single(model.Arguments);
        Assert.Equal("methods=['GET', 'POST']", model.Arguments[0]);
    }

    [Fact]
    public void NameConstructor_WithNullArguments_DefaultsToEmpty()
    {
        var model = new DecoratorModel("staticmethod", null);
        Assert.NotNull(model.Arguments);
        Assert.Empty(model.Arguments);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        var model = new DecoratorModel();
        model.Name = "classmethod";
        Assert.Equal("classmethod", model.Name);
    }

    [Fact]
    public void Arguments_CanBeModified()
    {
        var model = new DecoratorModel("route");
        model.Arguments.Add("'/api/users'");
        model.Arguments.Add("methods=['GET']");
        Assert.Equal(2, model.Arguments.Count);
    }

    [Fact]
    public void NameConstructor_WithMultipleArguments()
    {
        var args = new List<string> { "arg1", "arg2", "arg3" };
        var model = new DecoratorModel("custom", args);
        Assert.Equal(3, model.Arguments.Count);
    }

    [Fact]
    public void Arguments_CanBeReplaced()
    {
        var model = new DecoratorModel("test");
        model.Arguments = new List<string> { "new_arg" };
        Assert.Single(model.Arguments);
        Assert.Equal("new_arg", model.Arguments[0]);
    }
}
