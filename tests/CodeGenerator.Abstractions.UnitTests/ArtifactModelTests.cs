// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Abstractions.UnitTests;

public class ArtifactModelTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        var model = new ArtifactModel();

        Assert.NotNull(model);
    }

    [Fact]
    public void Parent_ShouldBeNullByDefault()
    {
        var model = new ArtifactModel();

        Assert.Null(model.Parent);
    }

    [Fact]
    public void Parent_ShouldBeSettable()
    {
        var parent = new ArtifactModel();
        var child = new ArtifactModel { Parent = parent };

        Assert.Same(parent, child.Parent);
    }

    [Fact]
    public void Validate_ShouldReturnSuccess()
    {
        var model = new ArtifactModel();

        var result = model.Validate();

        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void GetChildren_ShouldReturnEmpty()
    {
        var model = new ArtifactModel();

        var children = model.GetChildren().ToList();

        Assert.Empty(children);
    }

    [Fact]
    public void GetDescendants_WithNoChildren_ShouldReturnOnlySelf()
    {
        var model = new ArtifactModel();

        var descendants = model.GetDescendants();

        Assert.Single(descendants);
        Assert.Same(model, descendants[0]);
    }

    [Fact]
    public void GetDescendants_WithNullRoot_ShouldDefaultToThis()
    {
        var model = new ArtifactModel();

        var descendants = model.GetDescendants(null, null);

        Assert.Single(descendants);
        Assert.Same(model, descendants[0]);
    }

    [Fact]
    public void GetDescendants_WithExplicitRoot_ShouldUseProvidedRoot()
    {
        var model = new ArtifactModel();
        var root = new ArtifactModel();

        var descendants = model.GetDescendants(root);

        Assert.Single(descendants);
        Assert.Same(root, descendants[0]);
    }

    [Fact]
    public void GetDescendants_WithExplicitChildrenList_ShouldAppendToIt()
    {
        var model = new ArtifactModel();
        var existingList = new List<ArtifactModel>();

        var descendants = model.GetDescendants(null, existingList);

        Assert.Same(existingList, descendants);
        Assert.Single(descendants);
        Assert.Same(model, descendants[0]);
    }

    [Fact]
    public void GetDescendants_WithChildrenReturningChildren_ShouldReturnAll()
    {
        var grandchild = new ArtifactModelWithChildren(new List<ArtifactModel>());
        var child = new ArtifactModelWithChildren(new List<ArtifactModel> { grandchild });
        var root = new ArtifactModelWithChildren(new List<ArtifactModel> { child });

        var descendants = root.GetDescendants();

        Assert.Equal(3, descendants.Count);
        Assert.Same(root, descendants[0]);
        Assert.Same(child, descendants[1]);
        Assert.Same(grandchild, descendants[2]);
    }

    [Fact]
    public void GetDescendants_WithNullChild_ShouldSkipNullChildren()
    {
        var child = new ArtifactModelWithChildren(new List<ArtifactModel>());
        var root = new ArtifactModelWithChildren(new List<ArtifactModel> { null!, child });

        var descendants = root.GetDescendants();

        Assert.Equal(2, descendants.Count);
        Assert.Same(root, descendants[0]);
        Assert.Same(child, descendants[1]);
    }

    private class ArtifactModelWithChildren : ArtifactModel
    {
        private readonly List<ArtifactModel> _children;

        public ArtifactModelWithChildren(List<ArtifactModel> children)
        {
            _children = children;
        }

        public override IEnumerable<ArtifactModel> GetChildren()
        {
            return _children;
        }
    }
}
