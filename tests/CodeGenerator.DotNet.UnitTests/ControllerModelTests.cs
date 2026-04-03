using CodeGenerator.Core.Services;
using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Attributes;
using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Controllers;

namespace CodeGenerator.DotNet.UnitTests;

public class ControllerModelTests
{
    private readonly INamingConventionConverter _converter = new NamingConventionConverter();

    [Fact]
    public void Constructor_SetsNameWithControllerSuffix()
    {
        var entity = new ClassModel("Customer");

        var controller = new ControllerModel(_converter, entity);

        Assert.Equal("CustomerController", controller.Name);
    }

    [Fact]
    public void Constructor_AddsApiControllerAttribute()
    {
        var entity = new ClassModel("Customer");

        var controller = new ControllerModel(_converter, entity);

        Assert.Contains(controller.Attributes, a => a.Name == "ApiController");
        Assert.Contains(controller.Attributes, a => a.Type == AttributeType.ApiController);
    }

    [Fact]
    public void Constructor_AddsControllerBaseImplementation()
    {
        var entity = new ClassModel("Customer");

        var controller = new ControllerModel(_converter, entity);

        Assert.Contains(controller.Implements, i => i.Name == "ControllerBase");
    }

    [Fact]
    public void Constructor_AddsLoggerField()
    {
        var entity = new ClassModel("Customer");

        var controller = new ControllerModel(_converter, entity);

        Assert.Contains(controller.Fields, f => f.Name == "_logger");
        Assert.Contains(controller.Fields, f => f.Type.Name == "ILogger");
    }

    [Fact]
    public void Constructor_AddsMediatorField()
    {
        var entity = new ClassModel("Customer");

        var controller = new ControllerModel(_converter, entity);

        Assert.Contains(controller.Fields, f => f.Name == "_mediator");
        Assert.Contains(controller.Fields, f => f.Type.Name == "IMediator");
    }

    [Fact]
    public void Constructor_AddsConstructorWithParams()
    {
        var entity = new ClassModel("Customer");

        var controller = new ControllerModel(_converter, entity);

        Assert.Single(controller.Constructors);
        var ctor = controller.Constructors[0];
        Assert.Equal("CustomerController", ctor.Name);
        Assert.Contains(ctor.Params, p => p.Name == "logger");
        Assert.Contains(ctor.Params, p => p.Name == "mediator");
    }

    [Fact]
    public void Constructor_AddsCrudMethods()
    {
        var entity = new ClassModel("Customer");

        var controller = new ControllerModel(_converter, entity);

        Assert.True(controller.Methods.Count >= 4);
    }

    [Fact]
    public void Validate_WithValidName_ReturnsValid()
    {
        var entity = new ClassModel("Product");

        var controller = new ControllerModel(_converter, entity);
        var result = controller.Validate();

        Assert.True(result.IsValid);
    }

    [Fact]
    public void InheritsFromClassModel()
    {
        var entity = new ClassModel("Order");

        var controller = new ControllerModel(_converter, entity);

        Assert.IsAssignableFrom<ClassModel>(controller);
    }

    [Fact]
    public void Constructor_LoggerGenericTypeIsControllerName()
    {
        var entity = new ClassModel("Invoice");

        var controller = new ControllerModel(_converter, entity);

        var loggerField = controller.Fields.First(f => f.Name == "_logger");
        Assert.Single(loggerField.Type.GenericTypeParameters);
        Assert.Equal("InvoiceController", loggerField.Type.GenericTypeParameters[0].Name);
    }
}
