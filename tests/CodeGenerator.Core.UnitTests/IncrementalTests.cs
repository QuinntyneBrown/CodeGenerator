// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Incremental.Models;
using CodeGenerator.Core.Incremental.Services;

namespace CodeGenerator.Core.UnitTests;

public class IncrementalTests
{
    [Fact]
    public void AddFileModel_SetsProperties()
    {
        var model = new AddFileModel("/project", "src/MyFile.cs", "content", ConflictBehavior.Skip);
        Assert.Equal("/project", model.ProjectDirectory);
        Assert.Equal("src/MyFile.cs", model.RelativePath);
        Assert.Equal("content", model.Content);
        Assert.Equal(ConflictBehavior.Skip, model.OnConflict);
    }

    [Fact]
    public void DefaultConflictResolver_ReturnsError()
    {
        var resolver = new DefaultConflictResolver();
        var action = resolver.Resolve("file.cs", "old", "new");
        Assert.Equal(ConflictAction.Error, action);
    }

    [Fact]
    public void IncrementalProjectModel_HasFiles()
    {
        var model = new IncrementalProjectModel
        {
            ProjectDirectory = "/project",
            Files = [new AddFileModel("/project", "test.cs", "code")]
        };
        Assert.Single(model.GetChildren());
    }
}
