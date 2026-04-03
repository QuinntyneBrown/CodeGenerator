using CodeGenerator.DotNet.Builders;
using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Params;
using CodeGenerator.DotNet.Syntax.Types;

namespace CodeGenerator.DotNet.UnitTests;

public class ClassBuilderTests
{
    [Fact]
    public void For_CreatesBuilderWithName()
    {
        var model = ClassBuilder.For("MyClass").Build();

        Assert.Equal("MyClass", model.Name);
    }

    [Fact]
    public void Public_SetsAccessModifier()
    {
        var model = ClassBuilder.For("MyClass").Public().Build();

        Assert.Equal(AccessModifier.Public, model.AccessModifier);
    }

    [Fact]
    public void Internal_SetsAccessModifier()
    {
        var model = ClassBuilder.For("MyClass").Internal().Build();

        Assert.Equal(AccessModifier.Internal, model.AccessModifier);
    }

    [Fact]
    public void Sealed_SetsSealedTrue()
    {
        var model = ClassBuilder.For("MyClass").Sealed().Build();

        Assert.True(model.Sealed);
    }

    [Fact]
    public void Static_SetsStaticTrue()
    {
        var model = ClassBuilder.For("MyClass").Static().Build();

        Assert.True(model.Static);
    }

    [Fact]
    public void WithProperty_AddsPropertyToModel()
    {
        var model = ClassBuilder.For("MyClass")
            .WithProperty("Name", "string")
            .Build();

        Assert.Single(model.Properties);
        Assert.Equal("Name", model.Properties[0].Name);
        Assert.Equal("string", model.Properties[0].Type.Name);
        Assert.Equal(AccessModifier.Public, model.Properties[0].AccessModifier);
    }

    [Fact]
    public void WithProperty_MultipleProperties()
    {
        var model = ClassBuilder.For("MyClass")
            .WithProperty("Name", "string")
            .WithProperty("Age", "int")
            .Build();

        Assert.Equal(2, model.Properties.Count);
        Assert.Equal("Name", model.Properties[0].Name);
        Assert.Equal("Age", model.Properties[1].Name);
    }

    [Fact]
    public void WithProperty_SetsParentToClass()
    {
        var model = ClassBuilder.For("MyClass")
            .WithProperty("Name", "string")
            .Build();

        Assert.Equal(model, model.Properties[0].Parent);
    }

    [Fact]
    public void WithMethod_AddsMethodToModel()
    {
        var model = ClassBuilder.For("MyClass")
            .WithMethod("Execute", "void")
            .Build();

        Assert.Single(model.Methods);
        Assert.Equal("Execute", model.Methods[0].Name);
        Assert.Equal("void", model.Methods[0].ReturnType.Name);
        Assert.Equal(AccessModifier.Public, model.Methods[0].AccessModifier);
    }

    [Fact]
    public void WithMethod_SetsInterfaceFalse()
    {
        var model = ClassBuilder.For("MyClass")
            .WithMethod("DoWork", "void")
            .Build();

        Assert.False(model.Methods[0].Interface);
    }

    [Fact]
    public void WithMethod_WithParams_AddsParamsToMethod()
    {
        var param = new ParamModel { Name = "id", Type = new TypeModel("int") };

        var model = ClassBuilder.For("MyClass")
            .WithMethod("GetById", "string", param)
            .Build();

        Assert.Single(model.Methods[0].Params);
        Assert.Equal("id", model.Methods[0].Params[0].Name);
    }

    [Fact]
    public void WithMethod_SetsParentType()
    {
        var model = ClassBuilder.For("MyClass")
            .WithMethod("Execute", "void")
            .Build();

        Assert.Equal(model, model.Methods[0].ParentType);
    }

    [Fact]
    public void WithField_AddsFieldToModel()
    {
        var model = ClassBuilder.For("MyClass")
            .WithField("_name", "string")
            .Build();

        Assert.Single(model.Fields);
        Assert.Equal("_name", model.Fields[0].Name);
        Assert.Equal("string", model.Fields[0].Type.Name);
        Assert.Equal(AccessModifier.Private, model.Fields[0].AccessModifier);
    }

    [Fact]
    public void WithBaseClass_SetsBaseClassName()
    {
        var model = ClassBuilder.For("MyClass")
            .WithBaseClass("BaseEntity")
            .Build();

        Assert.Equal("BaseEntity", model.BaseClass);
    }

    [Fact]
    public void WithAttribute_AddsAttributeToModel()
    {
        var model = ClassBuilder.For("MyClass")
            .WithAttribute("Serializable")
            .Build();

        Assert.Single(model.Attributes);
        Assert.Equal("Serializable", model.Attributes[0].Name);
    }

    [Fact]
    public void WithAttribute_MultipleAttributes()
    {
        var model = ClassBuilder.For("MyClass")
            .WithAttribute("Serializable")
            .WithAttribute("ApiController")
            .Build();

        Assert.Equal(2, model.Attributes.Count);
    }

    [Fact]
    public void Implements_AddsInterfaceToModel()
    {
        var model = ClassBuilder.For("MyClass")
            .Implements("IDisposable")
            .Build();

        Assert.Single(model.Implements);
        Assert.Equal("IDisposable", model.Implements[0].Name);
    }

    [Fact]
    public void Implements_MultipleInterfaces()
    {
        var model = ClassBuilder.For("MyClass")
            .Implements("IDisposable")
            .Implements("ICloneable")
            .Build();

        Assert.Equal(2, model.Implements.Count);
    }

    [Fact]
    public void FluentApi_ChainsCorrectly()
    {
        var model = ClassBuilder.For("CustomerService")
            .Public()
            .Sealed()
            .WithBaseClass("ServiceBase")
            .Implements("ICustomerService")
            .WithField("_repository", "IRepository")
            .WithProperty("Name", "string")
            .WithMethod("GetAll", "List<Customer>")
            .WithAttribute("Injectable")
            .Build();

        Assert.Equal("CustomerService", model.Name);
        Assert.Equal(AccessModifier.Public, model.AccessModifier);
        Assert.True(model.Sealed);
        Assert.Equal("ServiceBase", model.BaseClass);
        Assert.Single(model.Implements);
        Assert.Single(model.Fields);
        Assert.Single(model.Properties);
        Assert.Single(model.Methods);
        Assert.Single(model.Attributes);
    }

    [Fact]
    public void ApplyDefaults_SetsPublicWhenDefault()
    {
        var model = ClassBuilder.For("MyClass").Build();

        Assert.Equal(AccessModifier.Public, model.AccessModifier);
    }

    [Fact]
    public void Build_WithEmptyName_ThrowsValidationException()
    {
        Assert.Throws<CodeGenerator.Core.Validation.ModelValidationException>(() =>
            ClassBuilder.For("").Build());
    }

    [Fact]
    public void Build_WithWhitespaceName_ThrowsValidationException()
    {
        Assert.Throws<CodeGenerator.Core.Validation.ModelValidationException>(() =>
            ClassBuilder.For("   ").Build());
    }
}
