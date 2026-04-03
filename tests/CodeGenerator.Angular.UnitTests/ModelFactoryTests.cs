// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Angular.Syntax;

namespace CodeGenerator.Angular.UnitTests;

public class ModelFactoryTests
{
    private readonly ModelFactory _factory = new();

    [Fact]
    public void CreateType_ReturnsTypeScriptTypeModel()
    {
        var model = _factory.CreateType("UserDto");

        Assert.IsType<TypeScriptTypeModel>(model);
    }

    [Fact]
    public void CreateType_SetsName()
    {
        var model = _factory.CreateType("UserDto");

        Assert.Equal("UserDto", model.Name);
    }

    [Fact]
    public void CreateType_InitializesEmptyProperties()
    {
        var model = _factory.CreateType("UserDto");

        Assert.NotNull(model.Properties);
        Assert.Empty(model.Properties);
    }

    [Fact]
    public void CreateType_DifferentNames_CreateDistinctInstances()
    {
        var model1 = _factory.CreateType("TypeA");
        var model2 = _factory.CreateType("TypeB");

        Assert.NotSame(model1, model2);
        Assert.Equal("TypeA", model1.Name);
        Assert.Equal("TypeB", model2.Name);
    }

    [Fact]
    public void CreateFunction_ReturnsFunctionModel()
    {
        var model = _factory.CreateFunction("getData");

        Assert.IsType<FunctionModel>(model);
    }

    [Fact]
    public void CreateFunction_SetsName()
    {
        var model = _factory.CreateFunction("getData");

        Assert.Equal("getData", model.Name);
    }

    [Fact]
    public void CreateFunction_InitializesEmptyImports()
    {
        var model = _factory.CreateFunction("getData");

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void CreateFunction_BodyIsEmptyString()
    {
        var model = _factory.CreateFunction("getData");

        Assert.Equal(string.Empty, model.Body);
    }

    [Fact]
    public void ImplementsIModelFactory()
    {
        Assert.IsAssignableFrom<IModelFactory>(_factory);
    }

    [Fact]
    public void CreateFunction_DifferentNames_CreateDistinctInstances()
    {
        var model1 = _factory.CreateFunction("fetchData");
        var model2 = _factory.CreateFunction("saveData");

        Assert.NotSame(model1, model2);
        Assert.Equal("fetchData", model1.Name);
        Assert.Equal("saveData", model2.Name);
    }
}
