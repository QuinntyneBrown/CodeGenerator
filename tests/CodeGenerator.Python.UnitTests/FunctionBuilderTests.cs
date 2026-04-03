// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;
using CodeGenerator.Python.Builders;
using CodeGenerator.Python.Syntax;

namespace CodeGenerator.Python.UnitTests;

public class FunctionBuilderTests
{
    [Fact]
    public void For_SetsName()
    {
        var model = FunctionBuilder.For("my_func").Build();
        Assert.Equal("my_func", model.Name);
    }

    [Fact]
    public void WithParam_AddsParamWithoutType()
    {
        var model = FunctionBuilder.For("greet")
            .WithParam("name")
            .Build();
        Assert.Single(model.Params);
        Assert.Equal("name", model.Params[0].Name);
        Assert.Null(model.Params[0].TypeHint);
    }

    [Fact]
    public void WithParam_AddsParamWithType()
    {
        var model = FunctionBuilder.For("add")
            .WithParam("x", "int")
            .Build();
        Assert.Single(model.Params);
        Assert.Equal("x", model.Params[0].Name);
        Assert.NotNull(model.Params[0].TypeHint);
        Assert.Equal("int", model.Params[0].TypeHint!.Name);
    }

    [Fact]
    public void WithParam_AddsMultipleParams()
    {
        var model = FunctionBuilder.For("add")
            .WithParam("a", "int")
            .WithParam("b", "int")
            .Build();
        Assert.Equal(2, model.Params.Count);
        Assert.Equal("a", model.Params[0].Name);
        Assert.Equal("b", model.Params[1].Name);
    }

    [Fact]
    public void WithBody_SetsBody()
    {
        var model = FunctionBuilder.For("hello")
            .WithBody("print('Hello, World!')")
            .Build();
        Assert.Equal("print('Hello, World!')", model.Body);
    }

    [Fact]
    public void WithBody_OverridesPreviousBody()
    {
        var model = FunctionBuilder.For("hello")
            .WithBody("old body")
            .WithBody("new body")
            .Build();
        Assert.Equal("new body", model.Body);
    }

    [Fact]
    public void WithDecorator_AddsDecorator()
    {
        var model = FunctionBuilder.For("handler")
            .WithDecorator("app.route")
            .Build();
        Assert.Single(model.Decorators);
        Assert.Equal("app.route", model.Decorators[0].Name);
    }

    [Fact]
    public void WithDecorator_AddsMultipleDecorators()
    {
        var model = FunctionBuilder.For("handler")
            .WithDecorator("app.route")
            .WithDecorator("login_required")
            .Build();
        Assert.Equal(2, model.Decorators.Count);
    }

    [Fact]
    public void WithReturn_SetsReturnType()
    {
        var model = FunctionBuilder.For("get_count")
            .WithReturn("int")
            .Build();
        Assert.NotNull(model.ReturnType);
        Assert.Equal("int", model.ReturnType.Name);
    }

    [Fact]
    public void WithReturn_OverridesPreviousReturnType()
    {
        var model = FunctionBuilder.For("get_value")
            .WithReturn("int")
            .WithReturn("str")
            .Build();
        Assert.NotNull(model.ReturnType);
        Assert.Equal("str", model.ReturnType.Name);
    }

    [Fact]
    public void Async_SetsIsAsyncTrue()
    {
        var model = FunctionBuilder.For("fetch_data")
            .Async()
            .Build();
        Assert.True(model.IsAsync);
    }

    [Fact]
    public void Default_IsAsyncFalse()
    {
        var model = FunctionBuilder.For("sync_func").Build();
        Assert.False(model.IsAsync);
    }

    [Fact]
    public void Build_WithEmptyName_ThrowsInvalidOperationException()
    {
        var builder = new FunctionBuilder();
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_WithWhitespaceName_ThrowsInvalidOperationException()
    {
        var builder = FunctionBuilder.For("   ");
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void FluentChaining_BuildsCompleteModel()
    {
        var model = FunctionBuilder.For("process")
            .WithParam("data", "dict")
            .WithParam("timeout", "int")
            .WithBody("return await fetch(data)")
            .WithReturn("dict")
            .WithDecorator("retry")
            .Async()
            .Build();

        Assert.Equal("process", model.Name);
        Assert.Equal(2, model.Params.Count);
        Assert.Equal("return await fetch(data)", model.Body);
        Assert.NotNull(model.ReturnType);
        Assert.Equal("dict", model.ReturnType.Name);
        Assert.Single(model.Decorators);
        Assert.True(model.IsAsync);
    }

    [Fact]
    public void Build_WithValidName_DoesNotThrow()
    {
        var model = FunctionBuilder.For("valid_func").Build();
        Assert.Equal("valid_func", model.Name);
    }

    [Fact]
    public void WithParam_NullType_NoTypeHint()
    {
        var model = FunctionBuilder.For("f")
            .WithParam("x", null)
            .Build();
        Assert.Null(model.Params[0].TypeHint);
    }

    [Fact]
    public void DefaultModel_HasEmptyBody()
    {
        var model = FunctionBuilder.For("f").Build();
        Assert.Equal(string.Empty, model.Body);
    }

    [Fact]
    public void DefaultModel_HasNoReturnType()
    {
        var model = FunctionBuilder.For("f").Build();
        Assert.Null(model.ReturnType);
    }
}
