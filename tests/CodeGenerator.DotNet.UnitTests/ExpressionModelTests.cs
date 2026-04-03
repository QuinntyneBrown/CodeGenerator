using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Constructors;
using CodeGenerator.DotNet.Syntax.Expressions;

namespace CodeGenerator.DotNet.UnitTests;

public class ExpressionModelTests
{
    [Fact]
    public void Constructor_SetsBody()
    {
        var model = new ExpressionModel("return x + y;");

        Assert.Equal("return x + y;", model.Body);
    }

    [Fact]
    public void Constructor_WithEmptyString()
    {
        var model = new ExpressionModel(string.Empty);

        Assert.Equal(string.Empty, model.Body);
    }

    [Fact]
    public void Constructor_WithNull()
    {
        var model = new ExpressionModel(null);

        Assert.Null(model.Body);
    }

    [Fact]
    public void Body_CanBeModified()
    {
        var model = new ExpressionModel("original");
        model.Body = "modified";

        Assert.Equal("modified", model.Body);
    }

    [Fact]
    public void Constructor_WithMultilineBody()
    {
        var body = "var x = 1;\nvar y = 2;\nreturn x + y;";
        var model = new ExpressionModel(body);

        Assert.Equal(body, model.Body);
    }
}

public class ConstructorExpressionModelTests
{
    [Fact]
    public void Constructor_SetsClassAndConstructor()
    {
        var classModel = new ClassModel("MyClass");
        var constructorModel = new ConstructorModel(classModel, "MyClass");

        var model = new ConstructorExpressionModel(classModel, constructorModel);

        Assert.Equal(classModel, model.Class);
        Assert.Equal(constructorModel, model.Constructor);
    }

    [Fact]
    public void Constructor_SetsEmptyBody()
    {
        var classModel = new ClassModel("MyClass");
        var constructorModel = new ConstructorModel(classModel, "MyClass");

        var model = new ConstructorExpressionModel(classModel, constructorModel);

        Assert.Equal(string.Empty, model.Body);
    }

    [Fact]
    public void Class_CanBeModified()
    {
        var classModel1 = new ClassModel("ClassA");
        var classModel2 = new ClassModel("ClassB");
        var constructorModel = new ConstructorModel(classModel1, "ClassA");

        var model = new ConstructorExpressionModel(classModel1, constructorModel);
        model.Class = classModel2;

        Assert.Equal(classModel2, model.Class);
    }

    [Fact]
    public void Constructor_CanBeModified()
    {
        var classModel = new ClassModel("MyClass");
        var constructor1 = new ConstructorModel(classModel, "MyClass");
        var constructor2 = new ConstructorModel(classModel, "MyClass");

        var model = new ConstructorExpressionModel(classModel, constructor1);
        model.Constructor = constructor2;

        Assert.Equal(constructor2, model.Constructor);
    }

    [Fact]
    public void InheritsFromExpressionModel()
    {
        var classModel = new ClassModel("MyClass");
        var constructorModel = new ConstructorModel(classModel, "MyClass");
        var model = new ConstructorExpressionModel(classModel, constructorModel);

        Assert.IsAssignableFrom<ExpressionModel>(model);
    }
}
