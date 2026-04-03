using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Interfaces;
using CodeGenerator.DotNet.Syntax.Properties;
using CodeGenerator.DotNet.Syntax.Types;

namespace CodeGenerator.DotNet.UnitTests;

public class PropertyModelTests
{
    [Fact]
    public void DefaultConstructor_InitializesEmptyCollections()
    {
        var model = new PropertyModel();

        Assert.NotNull(model.Accessors);
        Assert.Empty(model.Accessors);
        Assert.NotNull(model.Attributes);
        Assert.Empty(model.Attributes);
        Assert.Null(model.Name);
        Assert.Null(model.Type);
        Assert.False(model.Required);
        Assert.False(model.Id);
        Assert.False(model.Interface);
    }

    [Fact]
    public void FullConstructor_WithClassParent_SetsAllProperties()
    {
        var parent = new ClassModel("MyClass");
        var type = new TypeModel("string");
        var accessors = PropertyAccessorModel.GetSet;

        var model = new PropertyModel(parent, AccessModifier.Public, type, "Name", accessors, required: true, key: true);

        Assert.Equal(AccessModifier.Public, model.AccessModifier);
        Assert.Equal(type, model.Type);
        Assert.Equal("Name", model.Name);
        Assert.Equal(accessors, model.Accessors);
        Assert.True(model.Required);
        Assert.True(model.Id);
        Assert.Equal(parent, model.Parent);
        // ClassModel inherits InterfaceModel, so "parent is InterfaceModel" is true
        Assert.True(model.Interface);
    }

    [Fact]
    public void FullConstructor_WithInterfaceParent_SetsInterfaceTrue()
    {
        var parent = new InterfaceModel("IEntity");
        var type = new TypeModel("int");
        var accessors = PropertyAccessorModel.GetSet;

        var model = new PropertyModel(parent, AccessModifier.Public, type, "Id", accessors);

        Assert.True(model.Interface);
        Assert.Equal(parent, model.Parent);
    }

    [Fact]
    public void InterfaceConstructor_SetsInterfaceTrue()
    {
        var type = new TypeModel("string");
        var accessor = PropertyAccessorModel.Get;

        var model = new PropertyModel(type, "Value", accessor);

        Assert.True(model.Interface);
        Assert.Equal("Value", model.Name);
        Assert.Equal(type, model.Type);
        Assert.Single(model.Accessors);
    }

    [Fact]
    public void TypeScriptProperty_CreatesPropertyWithTypeAndName()
    {
        var model = PropertyModel.TypeScriptProperty("firstName", "string");

        Assert.Equal("firstName", model.Name);
        Assert.Equal("string", model.Type.Name);
        Assert.Null(model.Parent);
        Assert.Null(model.Accessors);
    }

    [Fact]
    public void ToTs_GuidType_ConvertedToString()
    {
        var parent = new ClassModel("Entity");
        var type = new TypeModel("Guid");
        var model = new PropertyModel(parent, AccessModifier.Public, type, "Id", PropertyAccessorModel.GetSet);

        var tsModel = model.ToTs();

        Assert.Equal("string", tsModel.Type.Name);
        Assert.Equal("Id", tsModel.Name);
    }

    [Fact]
    public void ToTs_IntType_ConvertedToNumber()
    {
        var parent = new ClassModel("Entity");
        var type = new TypeModel("int");
        var model = new PropertyModel(parent, AccessModifier.Public, type, "Count", PropertyAccessorModel.GetSet);

        var tsModel = model.ToTs();

        Assert.Equal("number", tsModel.Type.Name);
        Assert.Equal("Count", tsModel.Name);
    }

    [Fact]
    public void ToTs_StringType_RemainsString()
    {
        var parent = new ClassModel("Entity");
        var type = new TypeModel("string");
        var model = new PropertyModel(parent, AccessModifier.Public, type, "Name", PropertyAccessorModel.GetSet);

        var tsModel = model.ToTs();

        Assert.Equal("string", tsModel.Type.Name);
    }

    [Fact]
    public void IsClassProperty_WithClassParent_ReturnsTrue()
    {
        var parent = new ClassModel("MyClass");
        var model = new PropertyModel(parent, AccessModifier.Public, new TypeModel("string"), "Name", PropertyAccessorModel.GetSet);

        Assert.True(model.IsClassProperty);
    }

    [Fact]
    public void IsClassProperty_WithInterfaceParent_ReturnsFalse()
    {
        var parent = new InterfaceModel("IEntity");
        var model = new PropertyModel(parent, AccessModifier.Public, new TypeModel("string"), "Name", PropertyAccessorModel.GetSet);

        Assert.False(model.IsClassProperty);
    }

    [Fact]
    public void IsClassProperty_WithNullParent_ReturnsFalse()
    {
        var model = new PropertyModel();

        Assert.False(model.IsClassProperty);
    }

    [Fact]
    public void Static_DefaultsFalse()
    {
        var model = new PropertyModel();

        Assert.False(model.Static);
    }

    [Fact]
    public void DefaultValue_CanBeSetAndRetrieved()
    {
        var model = new PropertyModel
        {
            DefaultValue = "\"hello\"",
        };

        Assert.Equal("\"hello\"", model.DefaultValue);
    }

    [Fact]
    public void ForceAccessModifier_DefaultsFalse()
    {
        var model = new PropertyModel();

        Assert.False(model.ForceAccessModifier);
    }

    [Fact]
    public void FullConstructor_DefaultValues_RequiredFalseKeyFalse()
    {
        var parent = new ClassModel("MyClass");
        var model = new PropertyModel(parent, AccessModifier.Public, new TypeModel("string"), "Prop", PropertyAccessorModel.GetSet);

        Assert.False(model.Required);
        Assert.False(model.Id);
    }
}
