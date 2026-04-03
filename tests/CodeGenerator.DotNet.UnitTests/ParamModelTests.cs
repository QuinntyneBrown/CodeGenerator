using CodeGenerator.DotNet.Syntax.Params;

namespace CodeGenerator.DotNet.UnitTests;

public class ParamModelTests
{
    [Fact]
    public void CancellationToken_HasCorrectTypeAndName()
    {
        var param = ParamModel.CancellationToken;

        Assert.Equal("CancellationToken", param.Type.Name);
        Assert.Equal("cancellationToken", param.Name);
    }

    [Fact]
    public void Mediator_HasCorrectTypeAndName()
    {
        var param = ParamModel.Mediator;

        Assert.Equal("IMediator", param.Type.Name);
        Assert.Equal("mediator", param.Name);
    }

    [Fact]
    public void Mediator_ReturnsNewInstanceEachTime()
    {
        var param1 = ParamModel.Mediator;
        var param2 = ParamModel.Mediator;

        Assert.NotSame(param1, param2);
    }

    [Fact]
    public void LoggerOf_HasCorrectTypeAndName()
    {
        var param = ParamModel.LoggerOf("MyController");

        Assert.Equal("ILogger", param.Type.Name);
        Assert.Single(param.Type.GenericTypeParameters);
        Assert.Equal("MyController", param.Type.GenericTypeParameters[0].Name);
        Assert.Equal("logger", param.Name);
    }

    [Fact]
    public void LoggerOf_DifferentNames_ReturnDifferentTypes()
    {
        var param1 = ParamModel.LoggerOf("ServiceA");
        var param2 = ParamModel.LoggerOf("ServiceB");

        Assert.Equal("ServiceA", param1.Type.GenericTypeParameters[0].Name);
        Assert.Equal("ServiceB", param2.Type.GenericTypeParameters[0].Name);
    }

    [Fact]
    public void DefaultValue_DefaultsNull()
    {
        var param = new ParamModel();

        Assert.Null(param.DefaultValue);
    }

    [Fact]
    public void DefaultValue_CanBeSet()
    {
        var param = new ParamModel { DefaultValue = "null" };

        Assert.Equal("null", param.DefaultValue);
    }

    [Fact]
    public void ExtensionMethodParam_DefaultsFalse()
    {
        var param = new ParamModel();

        Assert.False(param.ExtensionMethodParam);
    }

    [Fact]
    public void ExtensionMethodParam_CanBeSetTrue()
    {
        var param = new ParamModel { ExtensionMethodParam = true };

        Assert.True(param.ExtensionMethodParam);
    }

    [Fact]
    public void Attribute_DefaultsNull()
    {
        var param = new ParamModel();

        Assert.Null(param.Attribute);
    }

    [Fact]
    public void Attributes_DefaultsToEmptyList()
    {
        var param = new ParamModel();

        Assert.NotNull(param.Attributes);
        Assert.Empty(param.Attributes);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        var param = new ParamModel { Name = "myParam" };

        Assert.Equal("myParam", param.Name);
    }

    [Fact]
    public void Type_CanBeSet()
    {
        var type = new CodeGenerator.DotNet.Syntax.Types.TypeModel("int");
        var param = new ParamModel { Type = type };

        Assert.Equal("int", param.Type.Name);
    }
}
