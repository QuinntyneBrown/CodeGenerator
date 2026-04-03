// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;
using CodeGenerator.Python.Builders;
using CodeGenerator.Python.Syntax;

namespace CodeGenerator.Python.UnitTests;

public class ClassBuilderTests
{
    [Fact]
    public void For_SetsName()
    {
        var model = ClassBuilder.For("MyClass").Build();
        Assert.Equal("MyClass", model.Name);
    }

    [Fact]
    public void WithBase_AddsSingleBase()
    {
        var model = ClassBuilder.For("Child")
            .WithBase("Parent")
            .Build();
        Assert.Single(model.Bases);
        Assert.Equal("Parent", model.Bases[0]);
    }

    [Fact]
    public void WithBase_AddsMultipleBases()
    {
        var model = ClassBuilder.For("MultiChild")
            .WithBase("Base1")
            .WithBase("Base2")
            .Build();
        Assert.Equal(2, model.Bases.Count);
        Assert.Equal("Base1", model.Bases[0]);
        Assert.Equal("Base2", model.Bases[1]);
    }

    [Fact]
    public void WithMethod_AddsMethod()
    {
        var model = ClassBuilder.For("MyClass")
            .WithMethod("do_work")
            .Build();
        Assert.Single(model.Methods);
        Assert.Equal("do_work", model.Methods[0].Name);
        Assert.Empty(model.Methods[0].Params);
        Assert.Equal(string.Empty, model.Methods[0].Body);
    }

    [Fact]
    public void WithMethod_WithParams()
    {
        var parameters = new List<ParamModel>
        {
            new("self"),
            new("name", new TypeHintModel("str"))
        };

        var model = ClassBuilder.For("MyClass")
            .WithMethod("greet", parameters, "print(name)")
            .Build();

        Assert.Single(model.Methods);
        Assert.Equal(2, model.Methods[0].Params.Count);
        Assert.Equal("print(name)", model.Methods[0].Body);
    }

    [Fact]
    public void WithMethod_WithNullParams_DefaultsToEmpty()
    {
        var model = ClassBuilder.For("MyClass")
            .WithMethod("init", null, "pass")
            .Build();
        Assert.Empty(model.Methods[0].Params);
        Assert.Equal("pass", model.Methods[0].Body);
    }

    [Fact]
    public void WithMethod_WithNullBody_DefaultsToEmpty()
    {
        var model = ClassBuilder.For("MyClass")
            .WithMethod("stub")
            .Build();
        Assert.Equal(string.Empty, model.Methods[0].Body);
    }

    [Fact]
    public void WithProperty_AddsPropertyWithoutType()
    {
        var model = ClassBuilder.For("MyClass")
            .WithProperty("name")
            .Build();
        Assert.Single(model.Properties);
        Assert.Equal("name", model.Properties[0].Name);
        Assert.Null(model.Properties[0].TypeHint);
    }

    [Fact]
    public void WithProperty_AddsPropertyWithType()
    {
        var model = ClassBuilder.For("MyClass")
            .WithProperty("age", "int")
            .Build();
        Assert.Single(model.Properties);
        Assert.Equal("age", model.Properties[0].Name);
        Assert.NotNull(model.Properties[0].TypeHint);
        Assert.Equal("int", model.Properties[0].TypeHint!.Name);
    }

    [Fact]
    public void WithDecorator_AddsDecorator()
    {
        var model = ClassBuilder.For("Config")
            .WithDecorator("dataclass")
            .Build();
        Assert.Single(model.Decorators);
        Assert.Equal("dataclass", model.Decorators[0].Name);
    }

    [Fact]
    public void WithDecorator_AddsMultipleDecorators()
    {
        var model = ClassBuilder.For("Config")
            .WithDecorator("dataclass")
            .WithDecorator("frozen")
            .Build();
        Assert.Equal(2, model.Decorators.Count);
    }

    [Fact]
    public void WithImport_AddsImportWithoutNames()
    {
        var model = ClassBuilder.For("App")
            .WithImport("flask")
            .Build();
        Assert.Single(model.Imports);
        Assert.Equal("flask", model.Imports[0].Module);
        Assert.Empty(model.Imports[0].Names);
    }

    [Fact]
    public void WithImport_AddsImportWithNames()
    {
        var model = ClassBuilder.For("App")
            .WithImport("flask", "Flask", "request")
            .Build();
        Assert.Single(model.Imports);
        Assert.Equal("flask", model.Imports[0].Module);
        Assert.Equal(2, model.Imports[0].Names.Count);
        Assert.Contains("Flask", model.Imports[0].Names);
        Assert.Contains("request", model.Imports[0].Names);
    }

    [Fact]
    public void Build_WithEmptyName_ThrowsInvalidOperationException()
    {
        var builder = new ClassBuilder();
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_WithWhitespaceName_ThrowsInvalidOperationException()
    {
        var builder = ClassBuilder.For("   ");
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void FluentChaining_BuildsCompleteModel()
    {
        var model = ClassBuilder.For("UserService")
            .WithBase("BaseService")
            .WithDecorator("injectable")
            .WithImport("base", "BaseService")
            .WithProperty("db", "Database")
            .WithMethod("get_user")
            .Build();

        Assert.Equal("UserService", model.Name);
        Assert.Single(model.Bases);
        Assert.Single(model.Decorators);
        Assert.Single(model.Imports);
        Assert.Single(model.Properties);
        Assert.Single(model.Methods);
    }

    [Fact]
    public void WithMultipleMethods_AddsAll()
    {
        var model = ClassBuilder.For("Calculator")
            .WithMethod("add")
            .WithMethod("subtract")
            .WithMethod("multiply")
            .Build();
        Assert.Equal(3, model.Methods.Count);
    }

    [Fact]
    public void WithMultipleProperties_AddsAll()
    {
        var model = ClassBuilder.For("Person")
            .WithProperty("name", "str")
            .WithProperty("age", "int")
            .WithProperty("email")
            .Build();
        Assert.Equal(3, model.Properties.Count);
    }
}
