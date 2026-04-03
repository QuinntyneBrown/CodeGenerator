using CodeGenerator.DotNet.Syntax.Interfaces;
using CodeGenerator.DotNet.Syntax.Methods;

namespace CodeGenerator.DotNet.UnitTests;

public class InterfaceModelTests
{
    [Fact]
    public void DefaultConstructor_InitializesCollections()
    {
        var model = new InterfaceModel();

        Assert.NotNull(model.Implements);
        Assert.Empty(model.Implements);
        Assert.NotNull(model.Methods);
        Assert.Empty(model.Methods);
        Assert.NotNull(model.Properties);
        Assert.Empty(model.Properties);
    }

    [Fact]
    public void NameConstructor_SetsName()
    {
        var model = new InterfaceModel("IRepository");

        Assert.Equal("IRepository", model.Name);
    }

    [Fact]
    public void Validate_WithName_ReturnsValid()
    {
        var model = new InterfaceModel("IService");

        var result = model.Validate();

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithNullName_ReturnsInvalid()
    {
        var model = new InterfaceModel();

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsInvalid()
    {
        var model = new InterfaceModel { Name = "" };

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsInvalid()
    {
        var model = new InterfaceModel { Name = "   " };

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void AddMethod_SetsInterfaceTrue()
    {
        var model = new InterfaceModel("IService");
        var method = new MethodModel { Name = "Execute", Interface = false };

        model.AddMethod(method);

        Assert.Single(model.Methods);
        Assert.True(model.Methods[0].Interface);
    }

    [Fact]
    public void AddMethod_MultipleMethods_AllHaveInterfaceTrue()
    {
        var model = new InterfaceModel("IService");

        model.AddMethod(new MethodModel { Name = "Method1", Interface = false });
        model.AddMethod(new MethodModel { Name = "Method2", Interface = false });
        model.AddMethod(new MethodModel { Name = "Method3", Interface = false });

        Assert.Equal(3, model.Methods.Count);
        Assert.All(model.Methods, m => Assert.True(m.Interface));
    }

    [Fact]
    public void AddMethod_PreservesMethodName()
    {
        var model = new InterfaceModel("IService");
        var method = new MethodModel { Name = "GetAll" };

        model.AddMethod(method);

        Assert.Equal("GetAll", model.Methods[0].Name);
    }

    [Fact]
    public void Implements_CanAddTypeModels()
    {
        var model = new InterfaceModel("IRepository");
        model.Implements.Add(new CodeGenerator.DotNet.Syntax.Types.TypeModel("IDisposable"));

        Assert.Single(model.Implements);
        Assert.Equal("IDisposable", model.Implements[0].Name);
    }
}
