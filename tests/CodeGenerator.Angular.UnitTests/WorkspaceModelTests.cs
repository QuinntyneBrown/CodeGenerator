// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Angular.Artifacts;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Angular.UnitTests;

public class WorkspaceModelTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var model = new WorkspaceModel("my-workspace", "1.0.0", "/root");

        Assert.Equal("my-workspace", model.Name);
    }

    [Fact]
    public void Constructor_SetsVersion()
    {
        var model = new WorkspaceModel("my-workspace", "2.5.0", "/root");

        Assert.Equal("2.5.0", model.Version);
    }

    [Fact]
    public void Constructor_SetsRootDirectory()
    {
        var model = new WorkspaceModel("my-workspace", "1.0.0", "/root");

        Assert.Equal("/root", model.RootDirectory);
    }

    [Fact]
    public void Constructor_SetsDirectoryAsCombinedPath()
    {
        var model = new WorkspaceModel("my-workspace", "1.0.0", "/root");

        var expected = Path.Combine("/root", "my-workspace");

        Assert.Equal(expected, model.Directory);
    }

    [Fact]
    public void Constructor_InitializesEmptyProjectsList()
    {
        var model = new WorkspaceModel("my-workspace", "1.0.0", "/root");

        Assert.NotNull(model.Projects);
        Assert.Empty(model.Projects);
    }

    [Fact]
    public void Projects_CanAddProjects()
    {
        var model = new WorkspaceModel("my-workspace", "1.0.0", "/root");
        var project = new ProjectModel("my-app", "application", "app", "/root");

        model.Projects.Add(project);

        Assert.Single(model.Projects);
        Assert.Equal("my-app", model.Projects[0].Name);
    }

    [Fact]
    public void Validate_ValidName_ReturnsValid()
    {
        var model = new WorkspaceModel("my-workspace", "1.0.0", "/root");

        var result = model.Validate();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var model = new WorkspaceModel("temp", "1.0.0", "/root");
        model.Name = "";

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
        Assert.Contains("required", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Validate_NullName_ReturnsError()
    {
        var model = new WorkspaceModel("temp", "1.0.0", "/root");
        model.Name = null!;

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhitespaceName_ReturnsError()
    {
        var model = new WorkspaceModel("temp", "1.0.0", "/root");
        model.Name = "   ";

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Directory_CanBeOverwritten()
    {
        var model = new WorkspaceModel("my-workspace", "1.0.0", "/root");
        model.Directory = "/custom/path";

        Assert.Equal("/custom/path", model.Directory);
    }

    [Fact]
    public void Version_CanBeUpdated()
    {
        var model = new WorkspaceModel("my-workspace", "1.0.0", "/root");
        model.Version = "3.0.0";

        Assert.Equal("3.0.0", model.Version);
    }
}
