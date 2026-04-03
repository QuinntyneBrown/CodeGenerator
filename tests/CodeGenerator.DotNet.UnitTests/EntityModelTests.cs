using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Entities;
using CodeGenerator.DotNet.Syntax.Properties;
using CodeGenerator.DotNet.Syntax.Types;

namespace CodeGenerator.DotNet.UnitTests;

public class EntityModelTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var entity = new EntityModel("Customer");

        Assert.Equal("Customer", entity.Name);
    }

    [Fact]
    public void Constructor_InheritsClassModelDefaults()
    {
        var entity = new EntityModel("Order");

        Assert.NotNull(entity.Fields);
        Assert.NotNull(entity.Constructors);
        Assert.NotNull(entity.Attributes);
        Assert.NotNull(entity.PrimaryConstructorParams);
        Assert.NotNull(entity.Properties);
        Assert.Equal(AccessModifier.Public, entity.AccessModifier);
    }

    [Fact]
    public void Validate_WithName_ReturnsValid()
    {
        var entity = new EntityModel("Product");

        var result = entity.Validate();

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithNullName_ReturnsInvalid()
    {
        var entity = new EntityModel(null);

        var result = entity.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsInvalid()
    {
        var entity = new EntityModel("");

        var result = entity.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsInvalid()
    {
        var entity = new EntityModel("   ");

        var result = entity.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateDto_ReturnsClassModelWithDtoSuffix()
    {
        var entity = new EntityModel("Customer");

        var dto = entity.CreateDto();

        Assert.IsType<ClassModel>(dto);
        Assert.Equal("CustomerDto", dto.Name);
    }

    [Fact]
    public void CreateDto_ReturnsEmptyClassModel()
    {
        var entity = new EntityModel("Product");
        entity.Properties.Add(new PropertyModel(
            entity,
            AccessModifier.Public,
            new TypeModel("string"),
            "Name",
            PropertyAccessorModel.GetSet));

        var dto = entity.CreateDto();

        Assert.Empty(dto.Properties);
    }

    [Fact]
    public void AggregateRootName_DefaultsNull()
    {
        var entity = new EntityModel("Item");

        Assert.Null(entity.AggregateRootName);
    }

    [Fact]
    public void IsEntityModel_IsClassModel()
    {
        var entity = new EntityModel("TestEntity");

        Assert.IsAssignableFrom<ClassModel>(entity);
    }

    [Fact]
    public void AddMethod_InheritedFromClassModel_WorksCorrectly()
    {
        var entity = new EntityModel("Customer");
        var method = new CodeGenerator.DotNet.Syntax.Methods.MethodModel { Name = "GetName", Interface = true };

        entity.AddMethod(method);

        Assert.Single(entity.Methods);
        Assert.False(entity.Methods[0].Interface);
    }
}
