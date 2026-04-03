using CodeGenerator.DotNet.Builders;
using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Entities;

namespace CodeGenerator.DotNet.UnitTests;

public class EntityBuilderTests
{
    [Fact]
    public void For_CreatesEntityWithName()
    {
        var model = EntityBuilder.For("Customer").Build();

        Assert.IsType<EntityModel>(model);
        Assert.Equal("Customer", model.Name);
    }

    [Fact]
    public void WithProperty_AddsPropertyToEntity()
    {
        var model = EntityBuilder.For("Customer")
            .WithProperty("Name", "string")
            .Build();

        Assert.Single(model.Properties);
        Assert.Equal("Name", model.Properties[0].Name);
        Assert.Equal("string", model.Properties[0].Type.Name);
        Assert.Equal(AccessModifier.Public, model.Properties[0].AccessModifier);
    }

    [Fact]
    public void WithProperty_SetsParentToEntity()
    {
        var model = EntityBuilder.For("Customer")
            .WithProperty("Name", "string")
            .Build();

        Assert.Equal(model, model.Properties[0].Parent);
    }

    [Fact]
    public void WithProperty_MultipleProperties()
    {
        var model = EntityBuilder.For("Customer")
            .WithProperty("FirstName", "string")
            .WithProperty("LastName", "string")
            .WithProperty("Age", "int")
            .Build();

        Assert.Equal(3, model.Properties.Count);
    }

    [Fact]
    public void WithKey_DefaultParameters_AddsGuidIdProperty()
    {
        var model = EntityBuilder.For("Customer")
            .WithKey()
            .Build();

        Assert.Single(model.Properties);
        var key = model.Properties[0];
        Assert.Equal("Id", key.Name);
        Assert.Equal("Guid", key.Type.Name);
        Assert.True(key.Id);
    }

    [Fact]
    public void WithKey_CustomNameAndType()
    {
        var model = EntityBuilder.For("Customer")
            .WithKey("CustomerId", "int")
            .Build();

        Assert.Single(model.Properties);
        var key = model.Properties[0];
        Assert.Equal("CustomerId", key.Name);
        Assert.Equal("int", key.Type.Name);
        Assert.True(key.Id);
    }

    [Fact]
    public void WithKey_SetsKeyTrue()
    {
        var model = EntityBuilder.For("Order")
            .WithKey()
            .Build();

        Assert.True(model.Properties[0].Id);
    }

    [Fact]
    public void WithTimestamps_AddsTwoDateTimeProperties()
    {
        var model = EntityBuilder.For("Customer")
            .WithTimestamps()
            .Build();

        Assert.Equal(2, model.Properties.Count);
        Assert.Equal("CreatedAt", model.Properties[0].Name);
        Assert.Equal("DateTime", model.Properties[0].Type.Name);
        Assert.Equal("UpdatedAt", model.Properties[1].Name);
        Assert.Equal("DateTime", model.Properties[1].Type.Name);
    }

    [Fact]
    public void FluentApi_ChainsCorrectly()
    {
        var model = EntityBuilder.For("Product")
            .WithKey()
            .WithProperty("Name", "string")
            .WithProperty("Price", "decimal")
            .WithTimestamps()
            .Build();

        Assert.Equal("Product", model.Name);
        Assert.Equal(5, model.Properties.Count);
        Assert.True(model.Properties[0].Id);
        Assert.Equal("Name", model.Properties[1].Name);
        Assert.Equal("Price", model.Properties[2].Name);
        Assert.Equal("CreatedAt", model.Properties[3].Name);
        Assert.Equal("UpdatedAt", model.Properties[4].Name);
    }

    [Fact]
    public void Build_WithEmptyName_ThrowsValidationException()
    {
        Assert.Throws<CodeGenerator.Core.Validation.ModelValidationException>(() =>
            EntityBuilder.For("").Build());
    }

    [Fact]
    public void Build_WithWhitespaceName_ThrowsValidationException()
    {
        Assert.Throws<CodeGenerator.Core.Validation.ModelValidationException>(() =>
            EntityBuilder.For("   ").Build());
    }

    [Fact]
    public void WithKey_HasPublicAccessModifier()
    {
        var model = EntityBuilder.For("Customer")
            .WithKey()
            .Build();

        Assert.Equal(AccessModifier.Public, model.Properties[0].AccessModifier);
    }
}
