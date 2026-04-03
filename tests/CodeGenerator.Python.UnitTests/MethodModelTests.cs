// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Python.Syntax;

namespace CodeGenerator.Python.UnitTests;

public class MethodModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var model = new MethodModel();
        Assert.Equal(string.Empty, model.Name);
        Assert.Equal(string.Empty, model.Body);
        Assert.NotNull(model.Params);
        Assert.Empty(model.Params);
        Assert.NotNull(model.Decorators);
        Assert.Empty(model.Decorators);
        Assert.Null(model.ReturnType);
        Assert.False(model.IsAsync);
        Assert.False(model.IsStatic);
        Assert.False(model.IsClassMethod);
    }

    [Fact]
    public void IsStatic_DefaultsFalse()
    {
        var model = new MethodModel();
        Assert.False(model.IsStatic);
    }

    [Fact]
    public void IsStatic_CanBeSetToTrue()
    {
        var model = new MethodModel { IsStatic = true };
        Assert.True(model.IsStatic);
    }

    [Fact]
    public void IsClassMethod_DefaultsFalse()
    {
        var model = new MethodModel();
        Assert.False(model.IsClassMethod);
    }

    [Fact]
    public void IsClassMethod_CanBeSetToTrue()
    {
        var model = new MethodModel { IsClassMethod = true };
        Assert.True(model.IsClassMethod);
    }

    [Fact]
    public void IsAsync_DefaultsFalse()
    {
        var model = new MethodModel();
        Assert.False(model.IsAsync);
    }

    [Fact]
    public void IsAsync_CanBeSetToTrue()
    {
        var model = new MethodModel { IsAsync = true };
        Assert.True(model.IsAsync);
    }

    [Fact]
    public void Validate_WithValidName_ReturnsValid()
    {
        var model = new MethodModel { Name = "do_work" };
        var result = model.Validate();
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsError()
    {
        var model = new MethodModel();
        var result = model.Validate();
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
        Assert.Equal("Method name is required.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsError()
    {
        var model = new MethodModel { Name = "\t" };
        var result = model.Validate();
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithNullName_ReturnsError()
    {
        var model = new MethodModel { Name = null! };
        var result = model.Validate();
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ReturnType_CanBeSet()
    {
        var model = new MethodModel
        {
            Name = "get_name",
            ReturnType = new TypeHintModel("str")
        };
        Assert.NotNull(model.ReturnType);
        Assert.Equal("str", model.ReturnType.Name);
    }

    [Fact]
    public void Params_CanBePopulated()
    {
        var model = new MethodModel { Name = "init" };
        model.Params.Add(new ParamModel("self"));
        model.Params.Add(new ParamModel("name", new TypeHintModel("str")));
        Assert.Equal(2, model.Params.Count);
    }

    [Fact]
    public void Decorators_CanBePopulated()
    {
        var model = new MethodModel { Name = "class_init" };
        model.Decorators.Add(new DecoratorModel("classmethod"));
        Assert.Single(model.Decorators);
    }

    [Fact]
    public void Body_CanBeSet()
    {
        var model = new MethodModel { Name = "run", Body = "self.execute()" };
        Assert.Equal("self.execute()", model.Body);
    }

    [Fact]
    public void AllFlags_CanBeCombined()
    {
        var model = new MethodModel
        {
            Name = "combined",
            IsAsync = true,
            IsStatic = true,
            IsClassMethod = true
        };
        Assert.True(model.IsAsync);
        Assert.True(model.IsStatic);
        Assert.True(model.IsClassMethod);
    }
}
