using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Methods;

namespace CodeGenerator.DotNet.UnitTests;

public class MethodModelTests
{
    [Fact]
    public void DefaultConstructor_InitializesParams()
    {
        var model = new MethodModel();

        Assert.NotNull(model.Params);
        Assert.Empty(model.Params);
    }

    [Fact]
    public void DefaultConstructor_InitializesAttributes()
    {
        var model = new MethodModel();

        Assert.NotNull(model.Attributes);
        Assert.Empty(model.Attributes);
    }

    [Fact]
    public void DefaultConstructor_SetsReturnTypeToVoid()
    {
        var model = new MethodModel();

        Assert.NotNull(model.ReturnType);
        Assert.Equal("void", model.ReturnType.Name);
    }

    [Fact]
    public void DefaultConstructor_GenericConstraints_Empty()
    {
        var model = new MethodModel();

        Assert.NotNull(model.GenericConstraints);
        Assert.Empty(model.GenericConstraints);
    }

    [Fact]
    public void DefaultConstructor_GenericTypeParameters_Empty()
    {
        var model = new MethodModel();

        Assert.NotNull(model.GenericTypeParameters);
        Assert.Empty(model.GenericTypeParameters);
    }

    [Fact]
    public void DefaultConstructor_GenericTypeParameterConstraints_Empty()
    {
        var model = new MethodModel();

        Assert.NotNull(model.GenericTypeParameterConstraints);
        Assert.Empty(model.GenericTypeParameterConstraints);
    }

    [Fact]
    public void Interface_DefaultsFalse()
    {
        var model = new MethodModel();

        Assert.False(model.Interface);
    }

    [Fact]
    public void Virtual_DefaultsFalse()
    {
        var model = new MethodModel();

        Assert.False(model.Virtual);
    }

    [Fact]
    public void Override_DefaultsFalse()
    {
        var model = new MethodModel();

        Assert.False(model.Override);
    }

    [Fact]
    public void Async_DefaultsFalse()
    {
        var model = new MethodModel();

        Assert.False(model.Async);
    }

    [Fact]
    public void Static_DefaultsFalse()
    {
        var model = new MethodModel();

        Assert.False(model.Static);
    }

    [Fact]
    public void DefaultMethod_DefaultsFalse()
    {
        var model = new MethodModel();

        Assert.False(model.DefaultMethod);
    }

    [Fact]
    public void ImplicitOperator_DefaultsFalse()
    {
        var model = new MethodModel();

        Assert.False(model.ImplicitOperator);
    }

    [Fact]
    public void ExplicitOperator_DefaultsFalse()
    {
        var model = new MethodModel();

        Assert.False(model.ExplicitOperator);
    }

    [Fact]
    public void ExpressionBody_DefaultsFalse()
    {
        var model = new MethodModel();

        Assert.False(model.ExpressionBody);
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var model = new MethodModel
        {
            Name = "Execute",
            Virtual = true,
            Override = true,
            Interface = true,
            DefaultMethod = true,
            Async = true,
            Static = true,
            ImplicitOperator = true,
            ExplicitOperator = true,
            ExpressionBody = true,
            AccessModifier = AccessModifier.Protected,
        };

        Assert.Equal("Execute", model.Name);
        Assert.True(model.Virtual);
        Assert.True(model.Override);
        Assert.True(model.Interface);
        Assert.True(model.DefaultMethod);
        Assert.True(model.Async);
        Assert.True(model.Static);
        Assert.True(model.ImplicitOperator);
        Assert.True(model.ExplicitOperator);
        Assert.True(model.ExpressionBody);
        Assert.Equal(AccessModifier.Protected, model.AccessModifier);
    }

    [Fact]
    public void ParentType_CanBeSet()
    {
        var model = new MethodModel();

        Assert.Null(model.ParentType);
    }

    [Fact]
    public void Body_DefaultsNull()
    {
        var model = new MethodModel();

        Assert.Null(model.Body);
    }
}
