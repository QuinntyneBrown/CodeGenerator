// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;
using CodeGenerator.Flask.Builders;
using CodeGenerator.Flask.Syntax;

namespace CodeGenerator.Flask.UnitTests;

public class ControllerBuilderTests
{
    [Fact]
    public void For_SetsName()
    {
        var model = ControllerBuilder.For("UserController").Build();

        Assert.Equal("UserController", model.Name);
    }

    [Fact]
    public void For_InitializesEmptyRoutes()
    {
        var model = ControllerBuilder.For("UserController").Build();

        Assert.NotNull(model.Routes);
        Assert.Empty(model.Routes);
    }

    [Fact]
    public void For_InitializesEmptyServiceInstances()
    {
        var model = ControllerBuilder.For("UserController").Build();

        Assert.NotNull(model.ServiceInstances);
        Assert.Empty(model.ServiceInstances);
    }

    [Fact]
    public void WithCrud_AddsFiveRoutes()
    {
        var model = ControllerBuilder
            .For("ItemController")
            .WithCrud("Item")
            .Build();

        Assert.Equal(5, model.Routes.Count);
    }

    [Fact]
    public void WithCrud_AddsGetAllRoute()
    {
        var model = ControllerBuilder
            .For("ItemController")
            .WithCrud("Item")
            .Build();

        var route = model.Routes[0];
        Assert.Equal("/", route.Path);
        Assert.Contains("GET", route.Methods);
        Assert.Equal("get_items", route.HandlerName);
        Assert.Equal("return item_service.get_all()", route.Body);
    }

    [Fact]
    public void WithCrud_AddsGetByIdRoute()
    {
        var model = ControllerBuilder
            .For("ItemController")
            .WithCrud("Item")
            .Build();

        var route = model.Routes[1];
        Assert.Equal("/<int:id>", route.Path);
        Assert.Contains("GET", route.Methods);
        Assert.Equal("get_item_by_id", route.HandlerName);
        Assert.Equal("return item_service.get_by_id(id)", route.Body);
    }

    [Fact]
    public void WithCrud_AddsCreateRoute()
    {
        var model = ControllerBuilder
            .For("ItemController")
            .WithCrud("Item")
            .Build();

        var route = model.Routes[2];
        Assert.Equal("/", route.Path);
        Assert.Contains("POST", route.Methods);
        Assert.Equal("create_item", route.HandlerName);
        Assert.Equal("return item_service.create(request.get_json())", route.Body);
    }

    [Fact]
    public void WithCrud_AddsUpdateRoute()
    {
        var model = ControllerBuilder
            .For("ItemController")
            .WithCrud("Item")
            .Build();

        var route = model.Routes[3];
        Assert.Equal("/<int:id>", route.Path);
        Assert.Contains("PUT", route.Methods);
        Assert.Equal("update_item", route.HandlerName);
        Assert.Equal("return item_service.update(id, request.get_json())", route.Body);
    }

    [Fact]
    public void WithCrud_AddsDeleteRoute()
    {
        var model = ControllerBuilder
            .For("ItemController")
            .WithCrud("Item")
            .Build();

        var route = model.Routes[4];
        Assert.Equal("/<int:id>", route.Path);
        Assert.Contains("DELETE", route.Methods);
        Assert.Equal("delete_item", route.HandlerName);
        Assert.Equal("return item_service.delete(id)", route.Body);
    }

    [Fact]
    public void WithCrud_UsesLowerCaseModelName()
    {
        var model = ControllerBuilder
            .For("OrderController")
            .WithCrud("ORDER")
            .Build();

        Assert.Equal("get_orders", model.Routes[0].HandlerName);
        Assert.Equal("return order_service.get_all()", model.Routes[0].Body);
    }

    [Fact]
    public void WithRoute_AddsSingleRoute()
    {
        var model = ControllerBuilder
            .For("HealthController")
            .WithRoute("/health", "GET", "health_check", "return jsonify({'status': 'ok'})")
            .Build();

        Assert.Single(model.Routes);
        var route = model.Routes[0];
        Assert.Equal("/health", route.Path);
        Assert.Contains("GET", route.Methods);
        Assert.Equal("health_check", route.HandlerName);
        Assert.Equal("return jsonify({'status': 'ok'})", route.Body);
    }

    [Fact]
    public void WithRoute_CanAddMultipleRoutes()
    {
        var model = ControllerBuilder
            .For("TestController")
            .WithRoute("/a", "GET", "get_a", "return a")
            .WithRoute("/b", "POST", "create_b", "return b")
            .Build();

        Assert.Equal(2, model.Routes.Count);
    }

    [Fact]
    public void WithUrlPrefix_SetsUrlPrefix()
    {
        var model = ControllerBuilder
            .For("UserController")
            .WithUrlPrefix("/api/users")
            .Build();

        Assert.Equal("/api/users", model.UrlPrefix);
    }

    [Fact]
    public void WithUrlPrefix_DefaultIsNull()
    {
        var model = ControllerBuilder
            .For("UserController")
            .Build();

        Assert.Null(model.UrlPrefix);
    }

    [Fact]
    public void WithService_AddsServiceInstance()
    {
        var model = ControllerBuilder
            .For("UserController")
            .WithService("user_service", "UserService")
            .Build();

        Assert.Single(model.ServiceInstances);
        Assert.Equal("user_service", model.ServiceInstances[0].VariableName);
        Assert.Equal("UserService", model.ServiceInstances[0].ClassName);
    }

    [Fact]
    public void WithService_CanAddMultipleServices()
    {
        var model = ControllerBuilder
            .For("OrderController")
            .WithService("order_service", "OrderService")
            .WithService("notification_service", "NotificationService")
            .Build();

        Assert.Equal(2, model.ServiceInstances.Count);
    }

    [Fact]
    public void Build_WithEmptyName_ThrowsInvalidOperationException()
    {
        var builder = ControllerBuilder.For("");

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_WithWhitespaceName_ThrowsInvalidOperationException()
    {
        var builder = ControllerBuilder.For("   ");

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void FluentChaining_ReturnsCorrectBuilderType()
    {
        var model = ControllerBuilder
            .For("ProductController")
            .WithUrlPrefix("/api/products")
            .WithService("product_service", "ProductService")
            .WithRoute("/search", "GET", "search_products", "return product_service.search()")
            .WithCrud("Product")
            .Build();

        Assert.Equal("ProductController", model.Name);
        Assert.Equal("/api/products", model.UrlPrefix);
        Assert.Single(model.ServiceInstances);
        Assert.Equal(6, model.Routes.Count);
    }
}
