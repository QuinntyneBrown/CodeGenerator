using CodeGenerator.DotNet.Syntax;
using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Constructors;
using CodeGenerator.DotNet.Syntax.Expressions;
using CodeGenerator.DotNet.Syntax.Params;
using CodeGenerator.DotNet.Syntax.Types;

namespace CodeGenerator.DotNet.UnitTests;

public class ConstructorModelTests
{
    [Fact]
    public void DefaultConstructor_InitializesCollections()
    {
        var model = new ConstructorModel();

        Assert.NotNull(model.Params);
        Assert.Empty(model.Params);
        Assert.NotNull(model.BaseParams);
        Assert.Empty(model.BaseParams);
        Assert.NotNull(model.ThisParams);
        Assert.Empty(model.ThisParams);
    }

    [Fact]
    public void DefaultConstructor_NameIsNull()
    {
        var model = new ConstructorModel();

        Assert.Null(model.Name);
    }

    [Fact]
    public void DefaultConstructor_BodyIsNull()
    {
        var model = new ConstructorModel();

        Assert.Null(model.Body);
    }

    [Fact]
    public void ClassConstructor_SetsParentAndName()
    {
        var classModel = new ClassModel("MyClass");
        var model = new ConstructorModel(classModel, "MyClass");

        Assert.Equal(classModel, model.Parent);
        Assert.Equal("MyClass", model.Name);
    }

    [Fact]
    public void ClassConstructor_InitializesCollections()
    {
        var classModel = new ClassModel("MyClass");
        var model = new ConstructorModel(classModel, "MyClass");

        Assert.NotNull(model.Params);
        Assert.Empty(model.Params);
        Assert.NotNull(model.BaseParams);
        Assert.Empty(model.BaseParams);
        Assert.NotNull(model.ThisParams);
        Assert.Empty(model.ThisParams);
    }

    [Fact]
    public void ClassConstructor_CreatesConstructorExpressionBody()
    {
        var classModel = new ClassModel("MyClass");
        var model = new ConstructorModel(classModel, "MyClass");

        Assert.NotNull(model.Body);
        Assert.IsType<ConstructorExpressionModel>(model.Body);
    }

    [Fact]
    public void ClassConstructor_BodyReferencesClassAndConstructor()
    {
        var classModel = new ClassModel("MyClass");
        var model = new ConstructorModel(classModel, "MyClass");

        var body = (ConstructorExpressionModel)model.Body;
        Assert.Equal(classModel, body.Class);
        Assert.Equal(model, body.Constructor);
    }

    [Fact]
    public void AccessModifier_CanBeSet()
    {
        var model = new ConstructorModel
        {
            AccessModifier = AccessModifier.Public,
        };

        Assert.Equal(AccessModifier.Public, model.AccessModifier);
    }

    [Fact]
    public void GetChildren_ReturnsParamsAndBody()
    {
        var classModel = new ClassModel("MyClass");
        var model = new ConstructorModel(classModel, "MyClass");
        model.Params.Add(new ParamModel { Name = "name", Type = new TypeModel("string") });
        model.Params.Add(new ParamModel { Name = "age", Type = new TypeModel("int") });

        var children = model.GetChildren().ToList();

        Assert.Equal(3, children.Count);
        Assert.IsType<ParamModel>(children[0]);
        Assert.IsType<ParamModel>(children[1]);
        Assert.IsType<ConstructorExpressionModel>(children[2]);
    }

    [Fact]
    public void GetChildren_NoParams_ReturnsBodyOnly()
    {
        var classModel = new ClassModel("MyClass");
        var model = new ConstructorModel(classModel, "MyClass");

        var children = model.GetChildren().ToList();

        Assert.Single(children);
        Assert.IsType<ConstructorExpressionModel>(children[0]);
    }

    [Fact]
    public void BaseParams_CanAddStrings()
    {
        var model = new ConstructorModel();
        model.BaseParams.Add("baseParam1");
        model.BaseParams.Add("baseParam2");

        Assert.Equal(2, model.BaseParams.Count);
    }

    [Fact]
    public void ThisParams_CanAddStrings()
    {
        var model = new ConstructorModel();
        model.ThisParams.Add("thisParam1");

        Assert.Single(model.ThisParams);
    }
}
