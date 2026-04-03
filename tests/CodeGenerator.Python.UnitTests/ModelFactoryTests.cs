// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Python.Syntax;

namespace CodeGenerator.Python.UnitTests;

public class ModelFactoryTests
{
    private readonly ModelFactory _factory = new();

    [Fact]
    public void CreateClass_ReturnsClassModelWithName()
    {
        var model = _factory.CreateClass("MyClass");
        Assert.NotNull(model);
        Assert.Equal("MyClass", model.Name);
        Assert.IsType<ClassModel>(model);
    }

    [Fact]
    public void CreateClass_InitializesEmptyCollections()
    {
        var model = _factory.CreateClass("Test");
        Assert.Empty(model.Bases);
        Assert.Empty(model.Methods);
        Assert.Empty(model.Properties);
        Assert.Empty(model.Decorators);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void CreateClass_ReturnsValidModel()
    {
        var model = _factory.CreateClass("ValidClass");
        var result = model.Validate();
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateFunction_ReturnsFunctionModelWithName()
    {
        var model = _factory.CreateFunction("my_func");
        Assert.NotNull(model);
        Assert.Equal("my_func", model.Name);
        Assert.IsType<FunctionModel>(model);
    }

    [Fact]
    public void CreateFunction_InitializesDefaults()
    {
        var model = _factory.CreateFunction("test");
        Assert.Equal(string.Empty, model.Body);
        Assert.Empty(model.Params);
        Assert.Empty(model.Decorators);
        Assert.Empty(model.Imports);
        Assert.Null(model.ReturnType);
        Assert.False(model.IsAsync);
    }

    [Fact]
    public void CreateFunction_ReturnsValidModel()
    {
        var model = _factory.CreateFunction("valid_func");
        var result = model.Validate();
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateDataClass_ReturnsClassModelWithName()
    {
        var model = _factory.CreateDataClass("Person");
        Assert.NotNull(model);
        Assert.Equal("Person", model.Name);
    }

    [Fact]
    public void CreateDataClass_WithNoProperties_HasEmptyProperties()
    {
        var model = _factory.CreateDataClass("Empty");
        Assert.Empty(model.Properties);
    }

    [Fact]
    public void CreateDataClass_WithProperties_AddsProperties()
    {
        var model = _factory.CreateDataClass("Person",
            ("name", "str"),
            ("age", "int"),
            ("email", "str"));
        Assert.Equal(3, model.Properties.Count);
    }

    [Fact]
    public void CreateDataClass_PropertiesHaveCorrectNames()
    {
        var model = _factory.CreateDataClass("Person",
            ("name", "str"),
            ("age", "int"));
        Assert.Equal("name", model.Properties[0].Name);
        Assert.Equal("age", model.Properties[1].Name);
    }

    [Fact]
    public void CreateDataClass_PropertiesHaveCorrectTypeHints()
    {
        var model = _factory.CreateDataClass("Person",
            ("name", "str"),
            ("age", "int"));
        Assert.NotNull(model.Properties[0].TypeHint);
        Assert.Equal("str", model.Properties[0].TypeHint!.Name);
        Assert.NotNull(model.Properties[1].TypeHint);
        Assert.Equal("int", model.Properties[1].TypeHint!.Name);
    }

    [Fact]
    public void CreateDataClass_ReturnsValidModel()
    {
        var model = _factory.CreateDataClass("Config", ("key", "str"));
        var result = model.Validate();
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ImplementsIModelFactory()
    {
        Assert.IsAssignableFrom<IModelFactory>(_factory);
    }

    [Fact]
    public void CreateDataClass_SingleProperty()
    {
        var model = _factory.CreateDataClass("Wrapper", ("value", "object"));
        Assert.Single(model.Properties);
        Assert.Equal("value", model.Properties[0].Name);
        Assert.Equal("object", model.Properties[0].TypeHint!.Name);
    }
}
