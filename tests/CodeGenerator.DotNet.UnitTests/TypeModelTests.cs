using CodeGenerator.DotNet.Syntax.Types;

namespace CodeGenerator.DotNet.UnitTests;

public class TypeModelTests
{
    [Fact]
    public void Constructor_WithName_SetsName()
    {
        var model = new TypeModel("string");

        Assert.Equal("string", model.Name);
    }

    [Fact]
    public void Constructor_WithNull_SetsNameNull()
    {
        var model = new TypeModel();

        Assert.Null(model.Name);
    }

    [Fact]
    public void Constructor_InitializesGenericTypeParameters()
    {
        var model = new TypeModel("List");

        Assert.NotNull(model.GenericTypeParameters);
        Assert.Empty(model.GenericTypeParameters);
    }

    [Fact]
    public void Task_StaticField_HasCorrectName()
    {
        var task = TypeModel.Task;

        Assert.Equal("Task", task.Name);
    }

    [Fact]
    public void TaskOf_ReturnsTaskWithGenericParameter()
    {
        var model = TypeModel.TaskOf("string");

        Assert.Equal("Task", model.Name);
        Assert.Single(model.GenericTypeParameters);
        Assert.Equal("string", model.GenericTypeParameters[0].Name);
    }

    [Fact]
    public void TaskOf_DifferentTypes_ReturnCorrectModels()
    {
        var stringTask = TypeModel.TaskOf("string");
        var intTask = TypeModel.TaskOf("int");

        Assert.Equal("string", stringTask.GenericTypeParameters[0].Name);
        Assert.Equal("int", intTask.GenericTypeParameters[0].Name);
    }

    [Fact]
    public void DbSetOf_ReturnsDbSetWithGenericParameter()
    {
        var model = TypeModel.DbSetOf("Customer");

        Assert.Equal("DbSet", model.Name);
        Assert.Single(model.GenericTypeParameters);
        Assert.Equal("Customer", model.GenericTypeParameters[0].Name);
    }

    [Fact]
    public void LoggerOf_ReturnsILoggerWithGenericParameter()
    {
        var model = TypeModel.LoggerOf("MyService");

        Assert.Equal("ILogger", model.Name);
        Assert.Single(model.GenericTypeParameters);
        Assert.Equal("MyService", model.GenericTypeParameters[0].Name);
    }

    [Fact]
    public void ListOf_ReturnsListWithGenericParameter()
    {
        var model = TypeModel.ListOf("Order");

        Assert.Equal("List", model.Name);
        Assert.Single(model.GenericTypeParameters);
        Assert.Equal("Order", model.GenericTypeParameters[0].Name);
    }

    [Fact]
    public void CreateTaskOfActionResultOf_ReturnsNestedGenericType()
    {
        var model = TypeModel.CreateTaskOfActionResultOf("CustomerDto");

        Assert.Equal("Task", model.Name);
        Assert.Single(model.GenericTypeParameters);

        var actionResult = model.GenericTypeParameters[0];
        Assert.Equal("ActionResult", actionResult.Name);
        Assert.Single(actionResult.GenericTypeParameters);
        Assert.Equal("CustomerDto", actionResult.GenericTypeParameters[0].Name);
    }

    [Fact]
    public void Nullable_DefaultsFalse()
    {
        var model = new TypeModel("string");

        Assert.False(model.Nullable);
    }

    [Fact]
    public void Nullable_CanBeSetTrue()
    {
        var model = new TypeModel("string") { Nullable = true };

        Assert.True(model.Nullable);
    }

    [Fact]
    public void Interface_DefaultsFalse()
    {
        var model = new TypeModel("IDisposable");

        Assert.False(model.Interface);
    }

    [Fact]
    public void Interface_CanBeSetTrue()
    {
        var model = new TypeModel("IDisposable") { Interface = true };

        Assert.True(model.Interface);
    }

    [Fact]
    public void Class_DefaultsNull()
    {
        var model = new TypeModel("MyType");

        Assert.Null(model.Class);
    }
}
