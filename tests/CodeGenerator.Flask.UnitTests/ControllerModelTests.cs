// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Flask.Syntax;

namespace CodeGenerator.Flask.UnitTests;

public class ControllerModelTests
{
    [Fact]
    public void DefaultConstructor_SetsEmptyName()
    {
        var model = new ControllerModel();

        Assert.Equal(string.Empty, model.Name);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyRoutes()
    {
        var model = new ControllerModel();

        Assert.NotNull(model.Routes);
        Assert.Empty(model.Routes);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyMiddlewareDecorators()
    {
        var model = new ControllerModel();

        Assert.NotNull(model.MiddlewareDecorators);
        Assert.Empty(model.MiddlewareDecorators);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyImports()
    {
        var model = new ControllerModel();

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyServiceInstances()
    {
        var model = new ControllerModel();

        Assert.NotNull(model.ServiceInstances);
        Assert.Empty(model.ServiceInstances);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptySchemaInstances()
    {
        var model = new ControllerModel();

        Assert.NotNull(model.SchemaInstances);
        Assert.Empty(model.SchemaInstances);
    }

    [Fact]
    public void NameConstructor_SetsName()
    {
        var model = new ControllerModel("UserController");

        Assert.Equal("UserController", model.Name);
    }

    [Fact]
    public void NameConstructor_InitializesCollections()
    {
        var model = new ControllerModel("UserController");

        Assert.NotNull(model.Routes);
        Assert.NotNull(model.MiddlewareDecorators);
        Assert.NotNull(model.Imports);
        Assert.NotNull(model.ServiceInstances);
        Assert.NotNull(model.SchemaInstances);
    }

    [Fact]
    public void UrlPrefix_DefaultIsNull()
    {
        var model = new ControllerModel();

        Assert.Null(model.UrlPrefix);
    }

    [Fact]
    public void UrlPrefix_CanBeSet()
    {
        var model = new ControllerModel();
        model.UrlPrefix = "/api/users";

        Assert.Equal("/api/users", model.UrlPrefix);
    }

    [Fact]
    public void Routes_CanAddRoute()
    {
        var model = new ControllerModel("Test");
        model.Routes.Add(new ControllerRouteModel
        {
            Path = "/",
            Methods = ["GET"],
            HandlerName = "index",
            Body = "return 'hello'"
        });

        Assert.Single(model.Routes);
    }

    [Fact]
    public void Validate_WithValidName_ReturnsValidResult()
    {
        var model = new ControllerModel("UserController");

        var result = model.Validate();

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsError()
    {
        var model = new ControllerModel();

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsError()
    {
        var model = new ControllerModel("   ");

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ErrorMessage_ContainsControllerNameRequired()
    {
        var model = new ControllerModel();

        var result = model.Validate();

        Assert.Contains(result.Errors, e => e.ErrorMessage == "Controller name is required.");
    }
}

public class ControllerRouteModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        var route = new ControllerRouteModel();

        Assert.Equal(string.Empty, route.Path);
        Assert.NotNull(route.Methods);
        Assert.Empty(route.Methods);
        Assert.Equal(string.Empty, route.HandlerName);
        Assert.Equal(string.Empty, route.Body);
    }

    [Fact]
    public void RequiresAuth_DefaultIsFalse()
    {
        var route = new ControllerRouteModel();

        Assert.False(route.RequiresAuth);
    }

    [Fact]
    public void WrapInTryCatch_DefaultIsFalse()
    {
        var route = new ControllerRouteModel();

        Assert.False(route.WrapInTryCatch);
    }

    [Fact]
    public void QueryParameters_DefaultIsEmptyList()
    {
        var route = new ControllerRouteModel();

        Assert.NotNull(route.QueryParameters);
        Assert.Empty(route.QueryParameters);
    }
}

public class ControllerInstanceModelTests
{
    [Fact]
    public void DefaultConstructor_SetsEmptyStrings()
    {
        var instance = new ControllerInstanceModel();

        Assert.Equal(string.Empty, instance.VariableName);
        Assert.Equal(string.Empty, instance.ClassName);
    }

    [Fact]
    public void ImportModule_DefaultIsNull()
    {
        var instance = new ControllerInstanceModel();

        Assert.Null(instance.ImportModule);
    }

    [Fact]
    public void ConstructorArgs_DefaultIsNull()
    {
        var instance = new ControllerInstanceModel();

        Assert.Null(instance.ConstructorArgs);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var instance = new ControllerInstanceModel
        {
            VariableName = "user_service",
            ClassName = "UserService",
            ImportModule = "app.services",
            ConstructorArgs = "db"
        };

        Assert.Equal("user_service", instance.VariableName);
        Assert.Equal("UserService", instance.ClassName);
        Assert.Equal("app.services", instance.ImportModule);
        Assert.Equal("db", instance.ConstructorArgs);
    }
}

public class ControllerRouteQueryParameterTests
{
    [Fact]
    public void Name_DefaultIsEmptyString()
    {
        var param = new ControllerRouteQueryParameter();

        Assert.Equal(string.Empty, param.Name);
    }

    [Fact]
    public void DefaultValue_DefaultIsNull()
    {
        var param = new ControllerRouteQueryParameter();

        Assert.Null(param.DefaultValue);
    }

    [Fact]
    public void Type_DefaultIsNull()
    {
        var param = new ControllerRouteQueryParameter();

        Assert.Null(param.Type);
    }
}
