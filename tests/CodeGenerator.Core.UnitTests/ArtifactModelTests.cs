// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Core.UnitTests;

public class ArtifactModelTests
{
    private class TestArtifact : ArtifactModel
    {
        public string Name { get; set; } = string.Empty;
        public List<ArtifactModel> Children { get; set; } = new();

        public override IEnumerable<ArtifactModel> GetChildren() => Children;
    }

    [Fact]
    public void GetChildren_Default_YieldsNothing()
    {
        var model = new ArtifactModel();
        Assert.Empty(model.GetChildren());
    }

    [Fact]
    public void GetDescendants_SingleNode_ReturnsSelf()
    {
        var model = new TestArtifact { Name = "Root" };
        var descendants = model.GetDescendants();
        Assert.Single(descendants);
        Assert.Same(model, descendants[0]);
    }

    [Fact]
    public void GetDescendants_WithChildren_ReturnsAllDescendants()
    {
        var grandchild = new TestArtifact { Name = "GrandChild" };
        var child1 = new TestArtifact { Name = "Child1", Children = [grandchild] };
        var child2 = new TestArtifact { Name = "Child2" };
        var root = new TestArtifact { Name = "Root", Children = [child1, child2] };

        var descendants = root.GetDescendants();

        Assert.Equal(4, descendants.Count);
        Assert.Contains(root, descendants);
        Assert.Contains(child1, descendants);
        Assert.Contains(child2, descendants);
        Assert.Contains(grandchild, descendants);
    }

    [Fact]
    public void GetDescendants_NoChildren_DoesNotRecurseInfinitely()
    {
        var model = new TestArtifact { Name = "Leaf" };
        var result = model.GetDescendants();
        Assert.Single(result);
    }
}
