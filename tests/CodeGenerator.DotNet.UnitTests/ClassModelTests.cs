using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Attributes;
using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Constructors;
using CodeGenerator.DotNet.Syntax.Fields;
using CodeGenerator.DotNet.Syntax.Methods;
using CodeGenerator.DotNet.Syntax.Properties;
using CodeGenerator.DotNet.Syntax.Types;

namespace CodeGenerator.DotNet.UnitTests;

public class ClassModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        var model = new ClassModel();

        Assert.NotNull(model.Fields);
        Assert.Empty(model.Fields);
        Assert.NotNull(model.Constructors);
        Assert.Empty(model.Constructors);
        Assert.NotNull(model.Attributes);
        Assert.Empty(model.Attributes);
        Assert.NotNull(model.PrimaryConstructorParams);
        Assert.Empty(model.PrimaryConstructorParams);
        Assert.Equal(AccessModifier.Public, model.AccessModifier);
    }

    [Fact]
    public void NameConstructor_SetsNameAndDefaults()
    {
        var model = new ClassModel("MyClass");

        Assert.Equal("MyClass", model.Name);
        Assert.NotNull(model.Fields);
        Assert.NotNull(model.Constructors);
        Assert.NotNull(model.Attributes);
        Assert.NotNull(model.PrimaryConstructorParams);
        Assert.Equal(AccessModifier.Public, model.AccessModifier);
    }

    [Fact]
    public void Validate_WithName_ReturnsValid()
    {
        var model = new ClassModel("MyClass");

        var result = model.Validate();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithNullName_ReturnsInvalid()
    {
        var model = new ClassModel();

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsInvalid()
    {
        var model = new ClassModel { Name = "" };

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsInvalid()
    {
        var model = new ClassModel { Name = "   " };

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void AddMethod_SetsInterfaceFalse_AndAddsToMethods()
    {
        var model = new ClassModel("MyClass");
        var method = new MethodModel { Name = "DoWork", Interface = true };

        model.AddMethod(method);

        Assert.Single(model.Methods);
        Assert.False(model.Methods[0].Interface);
        Assert.Equal("DoWork", model.Methods[0].Name);
    }

    [Fact]
    public void AddMethod_MultipleMethods_AllHaveInterfaceFalse()
    {
        var model = new ClassModel("MyClass");

        model.AddMethod(new MethodModel { Name = "Method1", Interface = true });
        model.AddMethod(new MethodModel { Name = "Method2", Interface = true });

        Assert.Equal(2, model.Methods.Count);
        Assert.All(model.Methods, m => Assert.False(m.Interface));
    }

    [Fact]
    public void CreateDto_ReturnsClassModelWithDtoSuffix()
    {
        var model = new ClassModel("Customer");
        model.Properties.Add(new PropertyModel(
            model,
            AccessModifier.Public,
            new TypeModel("string"),
            "Name",
            PropertyAccessorModel.GetSet));

        var dto = model.CreateDto();

        Assert.Equal("CustomerDto", dto.Name);
        Assert.Equal(model.Properties, dto.Properties);
    }

    [Fact]
    public void CreateDto_WithNoProperties_ReturnsEmptyDto()
    {
        var model = new ClassModel("Order");

        var dto = model.CreateDto();

        Assert.Equal("OrderDto", dto.Name);
        Assert.Empty(dto.Properties);
    }

    [Fact]
    public void GetChildren_ReturnsAllChildTypes()
    {
        var model = new ClassModel("TestClass");

        var field = new FieldModel { Name = "_field", Type = new TypeModel("string") };
        model.Fields.Add(field);

        var constructor = new ConstructorModel();
        model.Constructors.Add(constructor);

        var property = new PropertyModel(
            model,
            AccessModifier.Public,
            new TypeModel("int"),
            "Id",
            PropertyAccessorModel.GetSet);
        model.Properties.Add(property);

        var method = new MethodModel { Name = "Execute" };
        model.AddMethod(method);

        var attribute = new AttributeModel { Name = "Serializable" };
        model.Attributes.Add(attribute);

        model.Implements.Add(new TypeModel("IDisposable"));

        var innerClass = new ClassModel("InnerClass");
        model.InnerClasses.Add(innerClass);

        var children = model.GetChildren().ToList();

        Assert.Contains(field, children);
        Assert.Contains(constructor, children);
        Assert.Contains(property, children);
        Assert.Contains(method, children);
        Assert.Contains(attribute, children);
        Assert.Contains(innerClass, children);
        Assert.Equal(7, children.Count);
    }

    [Fact]
    public void GetChildren_EmptyClass_ReturnsEmpty()
    {
        var model = new ClassModel("EmptyClass");

        var children = model.GetChildren().ToList();

        Assert.Empty(children);
    }

    [Fact]
    public void Properties_SetAndGet()
    {
        var model = new ClassModel("MyClass")
        {
            Static = true,
            Abstract = true,
            Sealed = true,
            BaseClass = "BaseEntity",
        };

        Assert.True(model.Static);
        Assert.True(model.Abstract);
        Assert.True(model.Sealed);
        Assert.Equal("BaseEntity", model.BaseClass);
    }

    [Fact]
    public void GenericTypeParameters_DefaultsToEmpty()
    {
        var model = new ClassModel("GenericClass");

        Assert.NotNull(model.GenericTypeParameters);
        Assert.Empty(model.GenericTypeParameters);
    }

    [Fact]
    public void GenericConstraints_DefaultsToEmpty()
    {
        var model = new ClassModel("GenericClass");

        Assert.NotNull(model.GenericConstraints);
        Assert.Empty(model.GenericConstraints);
    }

    [Fact]
    public void InnerClasses_DefaultsToEmpty()
    {
        var model = new ClassModel("OuterClass");

        Assert.NotNull(model.InnerClasses);
        Assert.Empty(model.InnerClasses);
    }
}
