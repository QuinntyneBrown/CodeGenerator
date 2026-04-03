// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Python.Syntax;

namespace CodeGenerator.Python.UnitTests;

public class FunctionModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var model = new FunctionModel();
        Assert.Equal(string.Empty, model.Name);
        Assert.Equal(string.Empty, model.Body);
        Assert.NotNull(model.Params);
        Assert.Empty(model.Params);
        Assert.NotNull(model.Decorators);
        Assert.Empty(model.Decorators);
        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
        Assert.Null(model.ReturnType);
        Assert.False(model.IsAsync);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        var model = new FunctionModel { Name = "my_function" };
        Assert.Equal("my_function", model.Name);
    }

    [Fact]
    public void IsAsync_DefaultsFalse()
    {
        var model = new FunctionModel();
        Assert.False(model.IsAsync);
    }

    [Fact]
    public void IsAsync_CanBeSetToTrue()
    {
        var model = new FunctionModel { IsAsync = true };
        Assert.True(model.IsAsync);
    }

    [Fact]
    public void ReturnType_CanBeSet()
    {
        var model = new FunctionModel
        {
            Name = "get_value",
            ReturnType = new TypeHintModel("int")
        };
        Assert.NotNull(model.ReturnType);
        Assert.Equal("int", model.ReturnType.Name);
    }

    [Fact]
    public void Validate_WithValidName_ReturnsValid()
    {
        var model = new FunctionModel { Name = "my_func" };
        var result = model.Validate();
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsError()
    {
        var model = new FunctionModel();
        var result = model.Validate();
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
        Assert.Equal("Function name is required.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsError()
    {
        var model = new FunctionModel { Name = "  " };
        var result = model.Validate();
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithNullName_ReturnsError()
    {
        var model = new FunctionModel { Name = null! };
        var result = model.Validate();
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Params_CanBePopulated()
    {
        var model = new FunctionModel { Name = "add" };
        model.Params.Add(new ParamModel("a", new TypeHintModel("int")));
        model.Params.Add(new ParamModel("b", new TypeHintModel("int")));
        Assert.Equal(2, model.Params.Count);
        Assert.Equal("a", model.Params[0].Name);
        Assert.Equal("b", model.Params[1].Name);
    }

    [Fact]
    public void Decorators_CanBePopulated()
    {
        var model = new FunctionModel { Name = "handler" };
        model.Decorators.Add(new DecoratorModel("staticmethod"));
        Assert.Single(model.Decorators);
        Assert.Equal("staticmethod", model.Decorators[0].Name);
    }

    [Fact]
    public void Body_CanBeSet()
    {
        var model = new FunctionModel { Name = "greet", Body = "print('hello')" };
        Assert.Equal("print('hello')", model.Body);
    }

    [Fact]
    public void Imports_CanBePopulated()
    {
        var model = new FunctionModel { Name = "fetch" };
        model.Imports.Add(new ImportModel("asyncio"));
        Assert.Single(model.Imports);
    }
}
