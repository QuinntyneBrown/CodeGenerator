using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Fields;

namespace CodeGenerator.DotNet.UnitTests;

public class FieldModelTests
{
    [Fact]
    public void DefaultConstructor_SetsPrivateAccessModifier()
    {
        var model = new FieldModel();

        Assert.Equal(AccessModifier.Private, model.AccessModifier);
    }

    [Fact]
    public void DefaultConstructor_SetsReadOnlyTrue()
    {
        var model = new FieldModel();

        Assert.True(model.ReadOnly);
    }

    [Fact]
    public void DefaultConstructor_StaticDefaultsFalse()
    {
        var model = new FieldModel();

        Assert.False(model.Static);
    }

    [Fact]
    public void DefaultConstructor_ConstDefaultsFalse()
    {
        var model = new FieldModel();

        Assert.False(model.Const);
    }

    [Fact]
    public void DefaultConstructor_DefaultValueIsNull()
    {
        var model = new FieldModel();

        Assert.Null(model.DefaultValue);
    }

    [Fact]
    public void Mediator_ReturnsFieldWithIMediatorType()
    {
        var field = FieldModel.Mediator;

        Assert.Equal("IMediator", field.Type.Name);
        Assert.Equal("_mediator", field.Name);
        Assert.Equal(AccessModifier.Private, field.AccessModifier);
        Assert.True(field.ReadOnly);
    }

    [Fact]
    public void Mediator_ReturnsNewInstanceEachTime()
    {
        var field1 = FieldModel.Mediator;
        var field2 = FieldModel.Mediator;

        Assert.NotSame(field1, field2);
    }

    [Fact]
    public void LoggerOf_ReturnsFieldWithLoggerType()
    {
        var field = FieldModel.LoggerOf("MyController");

        Assert.Equal("ILogger", field.Type.Name);
        Assert.Single(field.Type.GenericTypeParameters);
        Assert.Equal("MyController", field.Type.GenericTypeParameters[0].Name);
        Assert.Equal("_logger", field.Name);
    }

    [Fact]
    public void LoggerOf_SetsDefaultAccessModifierAndReadOnly()
    {
        var field = FieldModel.LoggerOf("SomeService");

        Assert.Equal(AccessModifier.Private, field.AccessModifier);
        Assert.True(field.ReadOnly);
    }

    [Fact]
    public void LoggerOf_DifferentNames_ReturnDifferentTypes()
    {
        var field1 = FieldModel.LoggerOf("ServiceA");
        var field2 = FieldModel.LoggerOf("ServiceB");

        Assert.Equal("ServiceA", field1.Type.GenericTypeParameters[0].Name);
        Assert.Equal("ServiceB", field2.Type.GenericTypeParameters[0].Name);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var model = new FieldModel
        {
            Name = "_count",
            DefaultValue = "0",
            Const = true,
            Static = true,
            AccessModifier = AccessModifier.Public,
            ReadOnly = false,
        };

        Assert.Equal("_count", model.Name);
        Assert.Equal("0", model.DefaultValue);
        Assert.True(model.Const);
        Assert.True(model.Static);
        Assert.Equal(AccessModifier.Public, model.AccessModifier);
        Assert.False(model.ReadOnly);
    }
}
