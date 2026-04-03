using CodeGenerator.DotNet.Syntax.Attributes;
using CodeGenerator.DotNet.Syntax.Params;

namespace CodeGenerator.DotNet.UnitTests;

public class AttributeModelTests
{
    [Fact]
    public void DefaultConstructor_InitializesCollections()
    {
        var model = new AttributeModel();

        Assert.NotNull(model.Properties);
        Assert.Empty(model.Properties);
        Assert.NotNull(model.RawProperties);
        Assert.Empty(model.RawProperties);
        Assert.NotNull(model.Params);
        Assert.Empty(model.Params);
    }

    [Fact]
    public void DefaultConstructor_NameIsNull()
    {
        var model = new AttributeModel();

        Assert.Null(model.Name);
    }

    [Fact]
    public void DefaultConstructor_TemplateIsNull()
    {
        var model = new AttributeModel();

        Assert.Null(model.Template);
    }

    [Fact]
    public void DefaultConstructor_OrderIsZero()
    {
        var model = new AttributeModel();

        Assert.Equal(0, model.Order);
    }

    [Fact]
    public void FullConstructor_SetsAllProperties()
    {
        var properties = new Dictionary<string, string>
        {
            { "Name", "\"api/v1\"" },
        };

        var model = new AttributeModel(AttributeType.Route, "Route", properties);

        Assert.Equal(AttributeType.Route, model.Type);
        Assert.Equal("Route", model.Name);
        Assert.Equal(properties, model.Properties);
        Assert.NotNull(model.RawProperties);
        Assert.Empty(model.RawProperties);
    }

    [Fact]
    public void FullConstructor_WithNullProperties()
    {
        var model = new AttributeModel(AttributeType.ApiController, "ApiController", null);

        Assert.Equal(AttributeType.ApiController, model.Type);
        Assert.Equal("ApiController", model.Name);
        Assert.Null(model.Properties);
    }

    [Fact]
    public void Template_CanBeSet()
    {
        var model = new AttributeModel { Template = "[HttpGet(\"{id}\")]" };

        Assert.Equal("[HttpGet(\"{id}\")]", model.Template);
    }

    [Fact]
    public void Params_CanAddParams()
    {
        var model = new AttributeModel();
        model.Params.Add(new ParamModel { Name = "template" });

        Assert.Single(model.Params);
    }

    [Fact]
    public void Type_AllAttributeTypes_CanBeUsed()
    {
        var apiVersion = new AttributeModel(AttributeType.ApiVersion, "ApiVersion", null);
        var authorize = new AttributeModel(AttributeType.Authorize, "Authorize", null);
        var apiController = new AttributeModel(AttributeType.ApiController, "ApiController", null);
        var produces = new AttributeModel(AttributeType.Produces, "Produces", null);
        var fact = new AttributeModel(AttributeType.Fact, "Fact", null);

        Assert.Equal(AttributeType.ApiVersion, apiVersion.Type);
        Assert.Equal(AttributeType.Authorize, authorize.Type);
        Assert.Equal(AttributeType.ApiController, apiController.Type);
        Assert.Equal(AttributeType.Produces, produces.Type);
        Assert.Equal(AttributeType.Fact, fact.Type);
    }

    [Fact]
    public void Order_CanBeSetViaInit()
    {
        var model = new AttributeModel
        {
            Order = 5,
        };

        Assert.Equal(5, model.Order);
    }
}
