// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Flask.Syntax;

namespace CodeGenerator.Flask.UnitTests;

public class ModelFactoryTests
{
    private readonly ModelFactory _factory = new();

    [Fact]
    public void CreateController_ReturnsControllerModel()
    {
        var result = _factory.CreateController("UserController");

        Assert.IsType<ControllerModel>(result);
    }

    [Fact]
    public void CreateController_SetsName()
    {
        var result = _factory.CreateController("UserController");

        Assert.Equal("UserController", result.Name);
    }

    [Fact]
    public void CreateController_InitializesEmptyCollections()
    {
        var result = _factory.CreateController("UserController");

        Assert.NotNull(result.Routes);
        Assert.Empty(result.Routes);
        Assert.NotNull(result.Imports);
        Assert.Empty(result.Imports);
        Assert.NotNull(result.ServiceInstances);
        Assert.Empty(result.ServiceInstances);
    }

    [Fact]
    public void CreateModel_ReturnsModelModel()
    {
        var result = _factory.CreateModel("User");

        Assert.IsType<ModelModel>(result);
    }

    [Fact]
    public void CreateModel_SetsName()
    {
        var result = _factory.CreateModel("User");

        Assert.Equal("User", result.Name);
    }

    [Fact]
    public void CreateModel_InitializesEmptyCollections()
    {
        var result = _factory.CreateModel("User");

        Assert.NotNull(result.Columns);
        Assert.Empty(result.Columns);
        Assert.NotNull(result.Relationships);
        Assert.Empty(result.Relationships);
        Assert.NotNull(result.Imports);
        Assert.Empty(result.Imports);
    }

    [Fact]
    public void CreateSchema_ReturnsSchemaModel()
    {
        var result = _factory.CreateSchema("UserSchema");

        Assert.IsType<SchemaModel>(result);
    }

    [Fact]
    public void CreateSchema_SetsName()
    {
        var result = _factory.CreateSchema("UserSchema");

        Assert.Equal("UserSchema", result.Name);
    }

    [Fact]
    public void CreateSchema_InitializesEmptyCollections()
    {
        var result = _factory.CreateSchema("UserSchema");

        Assert.NotNull(result.Fields);
        Assert.Empty(result.Fields);
        Assert.NotNull(result.Imports);
        Assert.Empty(result.Imports);
    }

    [Fact]
    public void CreateService_ReturnsServiceModel()
    {
        var result = _factory.CreateService("UserService");

        Assert.IsType<ServiceModel>(result);
    }

    [Fact]
    public void CreateService_SetsName()
    {
        var result = _factory.CreateService("UserService");

        Assert.Equal("UserService", result.Name);
    }

    [Fact]
    public void CreateService_InitializesEmptyCollections()
    {
        var result = _factory.CreateService("UserService");

        Assert.NotNull(result.RepositoryReferences);
        Assert.Empty(result.RepositoryReferences);
        Assert.NotNull(result.Methods);
        Assert.Empty(result.Methods);
        Assert.NotNull(result.Imports);
        Assert.Empty(result.Imports);
    }

    [Fact]
    public void Factory_ImplementsIModelFactory()
    {
        Assert.IsAssignableFrom<IModelFactory>(_factory);
    }
}
