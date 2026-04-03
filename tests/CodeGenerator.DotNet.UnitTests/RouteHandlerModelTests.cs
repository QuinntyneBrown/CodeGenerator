using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Entities;
using CodeGenerator.DotNet.Syntax.RouteHandlers;

namespace CodeGenerator.DotNet.UnitTests;

public class RouteHandlerModelTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var entity = new EntityModel("Customer");

        var model = new RouteHandlerModel(
            "GetCustomers",
            "/api/customers",
            "AppDbContext",
            entity,
            RouteType.Get);

        Assert.Equal("GetCustomers", model.Name);
        Assert.Equal("/api/customers", model.Pattern);
        Assert.Equal("AppDbContext", model.DbContextName);
        Assert.Equal(entity, model.Entity);
        Assert.Equal(RouteType.Get, model.Type);
    }

    [Fact]
    public void Constructor_InitializesProduces()
    {
        var entity = new EntityModel("Order");

        var model = new RouteHandlerModel(
            "GetOrders",
            "/api/orders",
            "AppDbContext",
            entity,
            RouteType.Get);

        Assert.NotNull(model.Produces);
        Assert.Empty(model.Produces);
    }

    [Fact]
    public void RouteType_Get()
    {
        var entity = new EntityModel("Product");

        var model = new RouteHandlerModel("GetProducts", "/api/products", "AppDbContext", entity, RouteType.Get);

        Assert.Equal(RouteType.Get, model.Type);
    }

    [Fact]
    public void RouteType_GetById()
    {
        var entity = new EntityModel("Product");

        var model = new RouteHandlerModel("GetProductById", "/api/products/{id}", "AppDbContext", entity, RouteType.GetById);

        Assert.Equal(RouteType.GetById, model.Type);
    }

    [Fact]
    public void RouteType_Create()
    {
        var entity = new EntityModel("Product");

        var model = new RouteHandlerModel("CreateProduct", "/api/products", "AppDbContext", entity, RouteType.Create);

        Assert.Equal(RouteType.Create, model.Type);
    }

    [Fact]
    public void RouteType_Update()
    {
        var entity = new EntityModel("Product");

        var model = new RouteHandlerModel("UpdateProduct", "/api/products/{id}", "AppDbContext", entity, RouteType.Update);

        Assert.Equal(RouteType.Update, model.Type);
    }

    [Fact]
    public void RouteType_Delete()
    {
        var entity = new EntityModel("Product");

        var model = new RouteHandlerModel("DeleteProduct", "/api/products/{id}", "AppDbContext", entity, RouteType.Delete);

        Assert.Equal(RouteType.Delete, model.Type);
    }

    [Fact]
    public void Name_IsPrivateSet()
    {
        var entity = new EntityModel("Item");
        var model = new RouteHandlerModel("GetItems", "/api/items", "AppDbContext", entity, RouteType.Get);

        Assert.Equal("GetItems", model.Name);
    }

    [Fact]
    public void Pattern_IsPrivateSet()
    {
        var entity = new EntityModel("Item");
        var model = new RouteHandlerModel("GetItems", "/api/items", "AppDbContext", entity, RouteType.Get);

        Assert.Equal("/api/items", model.Pattern);
    }

    [Fact]
    public void DbContextName_IsPrivateSet()
    {
        var entity = new EntityModel("Item");
        var model = new RouteHandlerModel("GetItems", "/api/items", "MyDbContext", entity, RouteType.Get);

        Assert.Equal("MyDbContext", model.DbContextName);
    }

    [Fact]
    public void Entity_IsPrivateSet()
    {
        var entity = new EntityModel("Item");
        var model = new RouteHandlerModel("GetItems", "/api/items", "AppDbContext", entity, RouteType.Get);

        Assert.Same(entity, model.Entity);
    }
}
