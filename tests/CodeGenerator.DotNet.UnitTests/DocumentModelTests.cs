using CodeGenerator.Core.Syntax;
using CodeGenerator.DotNet.Syntax.Classes;
using CodeGenerator.DotNet.Syntax.Documents;
using CodeGenerator.DotNet.Syntax.Enums;
using CodeGenerator.DotNet.Syntax.Interfaces;

namespace CodeGenerator.DotNet.UnitTests;

public class DocumentModelTests
{
    [Fact]
    public void DefaultConstructor_InitializesCode()
    {
        var model = new DocumentModel();

        Assert.NotNull(model.Code);
        Assert.Empty(model.Code);
    }

    [Fact]
    public void DefaultConstructor_NameIsNull()
    {
        var model = new DocumentModel();

        Assert.Null(model.Name);
    }

    [Fact]
    public void DefaultConstructor_NamespaceIsNull()
    {
        var model = new DocumentModel();

        Assert.Null(model.Namespace);
    }

    [Fact]
    public void NameNamespaceConstructor_SetsBoth()
    {
        var model = new DocumentModel("MyDocument", "MyApp.Models");

        Assert.Equal("MyDocument", model.Name);
        Assert.Equal("MyApp.Models", model.Namespace);
        Assert.NotNull(model.Code);
        Assert.Empty(model.Code);
    }

    [Fact]
    public void RootNamespace_CanBeSet()
    {
        var model = new DocumentModel { RootNamespace = "MyApp" };

        Assert.Equal("MyApp", model.RootNamespace);
    }

    [Fact]
    public void GetChildren_ReturnsCodeItems()
    {
        var model = new DocumentModel("Doc", "MyApp");
        var classModel = new ClassModel("MyClass");
        var interfaceModel = new InterfaceModel("IMyInterface");

        model.Code.Add(classModel);
        model.Code.Add(interfaceModel);

        var children = model.GetChildren().ToList();

        Assert.Equal(2, children.Count);
        Assert.Contains(classModel, children);
        Assert.Contains(interfaceModel, children);
    }

    [Fact]
    public void GetChildren_EmptyCode_ReturnsEmpty()
    {
        var model = new DocumentModel("Doc", "MyApp");

        var children = model.GetChildren().ToList();

        Assert.Empty(children);
    }

    [Fact]
    public void GetChildren_SingleItem_ReturnsSingle()
    {
        var model = new DocumentModel("Doc", "MyApp");
        var enumModel = new EnumModel("Status");
        model.Code.Add(enumModel);

        var children = model.GetChildren().ToList();

        Assert.Single(children);
        Assert.Equal(enumModel, children[0]);
    }

    [Fact]
    public void Code_CanBeReassigned()
    {
        var model = new DocumentModel("Doc", "MyApp");
        var newCode = new List<SyntaxModel> { new ClassModel("A"), new ClassModel("B") };
        model.Code = newCode;

        Assert.Equal(2, model.Code.Count);
    }
}
