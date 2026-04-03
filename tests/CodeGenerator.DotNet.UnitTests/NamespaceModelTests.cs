using CodeGenerator.Core.Syntax;
using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Namespaces;

namespace CodeGenerator.DotNet.UnitTests;

public class NamespaceModelTests
{
    [Fact]
    public void DefaultConstructor_PropertiesAreNull()
    {
        var model = new NamespaceModel();

        Assert.Null(model.Name);
        Assert.Null(model.SyntaxModels);
    }

    [Fact]
    public void Constructor_WithNameAndModels_SetsBoth()
    {
        var models = new List<SyntaxModel>
        {
            new ClassModel("MyClass"),
        };

        var ns = new NamespaceModel("MyApp.Models", models);

        Assert.Equal("MyApp.Models", ns.Name);
        Assert.Single(ns.SyntaxModels);
    }

    [Fact]
    public void Constructor_WithEmptyList_SetsEmptyList()
    {
        var ns = new NamespaceModel("MyApp", new List<SyntaxModel>());

        Assert.Equal("MyApp", ns.Name);
        Assert.NotNull(ns.SyntaxModels);
        Assert.Empty(ns.SyntaxModels);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        var model = new NamespaceModel { Name = "MyApp.Services" };

        Assert.Equal("MyApp.Services", model.Name);
    }

    [Fact]
    public void SyntaxModels_CanBeReassigned()
    {
        var model = new NamespaceModel("MyApp", new List<SyntaxModel>());
        var newModels = new List<SyntaxModel>
        {
            new ClassModel("A"),
            new ClassModel("B"),
        };

        model.SyntaxModels = newModels;

        Assert.Equal(2, model.SyntaxModels.Count);
    }

    [Fact]
    public void Constructor_MultipleModels()
    {
        var models = new List<SyntaxModel>
        {
            new ClassModel("ClassA"),
            new ClassModel("ClassB"),
            new ClassModel("ClassC"),
        };

        var ns = new NamespaceModel("MyApp.Models", models);

        Assert.Equal(3, ns.SyntaxModels.Count);
    }
}
