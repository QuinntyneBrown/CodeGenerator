// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Syntax;

namespace CodeGenerator.Abstractions.UnitTests;

public class SyntaxModelTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        var model = new SyntaxModel();

        Assert.NotNull(model);
    }

    [Fact]
    public void Constructor_ShouldInitializeUsingsToEmptyList()
    {
        var model = new SyntaxModel();

        Assert.NotNull(model.Usings);
        Assert.Empty(model.Usings);
    }

    [Fact]
    public void Parent_ShouldBeNullByDefault()
    {
        var model = new SyntaxModel();

        Assert.Null(model.Parent);
    }

    [Fact]
    public void Parent_ShouldBeSettable()
    {
        var parent = new SyntaxModel();
        var child = new SyntaxModel { Parent = parent };

        Assert.Same(parent, child.Parent);
    }

    [Fact]
    public void Usings_ShouldBeSettable()
    {
        var model = new SyntaxModel();

        model.Usings.Add(new UsingModel("System"));

        Assert.Single(model.Usings);
        Assert.Equal("System", model.Usings[0].Name);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess()
    {
        var model = new SyntaxModel();

        var result = model.Validate();

        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void GetChildren_ShouldReturnEmpty()
    {
        var model = new SyntaxModel();

        var children = model.GetChildren().ToList();

        Assert.Empty(children);
    }

    [Fact]
    public void GetDescendants_WithNoChildren_ShouldReturnOnlySelf()
    {
        var model = new SyntaxModel();

        var descendants = model.GetDescendants();

        Assert.Single(descendants);
        Assert.Same(model, descendants[0]);
    }

    [Fact]
    public void GetDescendants_WithNullParameters_ShouldDefaultCorrectly()
    {
        var model = new SyntaxModel();

        var descendants = model.GetDescendants(null, null);

        Assert.Single(descendants);
        Assert.Same(model, descendants[0]);
    }

    [Fact]
    public void GetDescendants_WithExplicitRoot_ShouldUseProvidedRoot()
    {
        var model = new SyntaxModel();
        var root = new SyntaxModel();

        var descendants = model.GetDescendants(root);

        Assert.Single(descendants);
        Assert.Same(root, descendants[0]);
    }

    [Fact]
    public void GetDescendants_WithExplicitChildrenList_ShouldAppendToIt()
    {
        var model = new SyntaxModel();
        var existingList = new List<SyntaxModel>();

        var descendants = model.GetDescendants(null, existingList);

        Assert.Same(existingList, descendants);
        Assert.Single(descendants);
    }

    [Fact]
    public void GetDescendants_WithNestedChildren_ShouldReturnAll()
    {
        var grandchild = new SyntaxModelWithChildren(new List<SyntaxModel>());
        var child = new SyntaxModelWithChildren(new List<SyntaxModel> { grandchild });
        var root = new SyntaxModelWithChildren(new List<SyntaxModel> { child });

        var descendants = root.GetDescendants();

        Assert.Equal(3, descendants.Count);
        Assert.Same(root, descendants[0]);
        Assert.Same(child, descendants[1]);
        Assert.Same(grandchild, descendants[2]);
    }

    [Fact]
    public void GetDescendants_WithNullChild_ShouldSkipIt()
    {
        var child = new SyntaxModelWithChildren(new List<SyntaxModel>());
        var root = new SyntaxModelWithChildren(new List<SyntaxModel> { null!, child });

        var descendants = root.GetDescendants();

        Assert.Equal(2, descendants.Count);
        Assert.Same(root, descendants[0]);
        Assert.Same(child, descendants[1]);
    }

    [Fact]
    public void GetDescendants_WithMultipleChildren_ShouldReturnAllDescendants()
    {
        var child1 = new SyntaxModelWithChildren(new List<SyntaxModel>());
        var child2 = new SyntaxModelWithChildren(new List<SyntaxModel>());
        var root = new SyntaxModelWithChildren(new List<SyntaxModel> { child1, child2 });

        var descendants = root.GetDescendants();

        Assert.Equal(3, descendants.Count);
        Assert.Same(root, descendants[0]);
        Assert.Same(child1, descendants[1]);
        Assert.Same(child2, descendants[2]);
    }

    private class SyntaxModelWithChildren : SyntaxModel
    {
        private readonly List<SyntaxModel> _children;

        public SyntaxModelWithChildren(List<SyntaxModel> children)
        {
            _children = children;
        }

        public override IEnumerable<SyntaxModel> GetChildren()
        {
            return _children;
        }
    }
}
