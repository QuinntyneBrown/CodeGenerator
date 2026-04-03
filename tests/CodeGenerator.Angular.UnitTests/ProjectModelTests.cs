// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Angular.Artifacts;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Angular.UnitTests;

public class ProjectModelTests
{
    [Fact]
    public void Constructor_SetsNameProperty()
    {
        var model = new ProjectModel("my-app", "application", "app", "/root");

        Assert.Equal("my-app", model.Name);
    }

    [Fact]
    public void Constructor_SetsProjectType()
    {
        var model = new ProjectModel("my-app", "library", "lib", "/root");

        Assert.Equal("library", model.ProjectType);
    }

    [Fact]
    public void Constructor_SetsPrefix()
    {
        var model = new ProjectModel("my-app", "application", "app", "/root");

        Assert.Equal("app", model.Prefix);
    }

    [Fact]
    public void Constructor_SetsRootDirectory()
    {
        var model = new ProjectModel("my-app", "application", "app", "/root");

        Assert.Equal("/root", model.RootDirectory);
    }

    [Fact]
    public void Constructor_NonScopedName_BuildsDirectoryWithProjects()
    {
        var model = new ProjectModel("my-app", "application", "app", "/root");

        var expected = $"/root{Path.DirectorySeparatorChar}projects{Path.DirectorySeparatorChar}my-app";

        Assert.Equal(expected, model.Directory);
    }

    [Fact]
    public void Constructor_ScopedName_BuildsDirectoryWithScopeAndProject()
    {
        var model = new ProjectModel("@myorg/my-lib", "library", "lib", "/root");

        var sep = Path.DirectorySeparatorChar;
        var expected = $"/root{sep}projects{sep}myorg{sep}my-lib";

        Assert.Equal(expected, model.Directory);
    }

    [Fact]
    public void Constructor_ScopedName_StripsAtSymbolFromScope()
    {
        var model = new ProjectModel("@scope/package", "application", "app", "/workspace");

        Assert.DoesNotContain("@", model.Directory);
    }

    [Fact]
    public void Constructor_ScopedName_PreservesPackageName()
    {
        var model = new ProjectModel("@scope/my-package", "application", "app", "/workspace");

        Assert.EndsWith("my-package", model.Directory);
    }

    [Fact]
    public void Validate_ValidName_ReturnsValid()
    {
        var model = new ProjectModel("my-app", "application", "app", "/root");

        var result = model.Validate();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var model = new ProjectModel("temp", "application", "app", "/root");
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
        var model = new ProjectModel("temp", "application", "app", "/root");
        model.Name = null!;

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhitespaceName_ReturnsError()
    {
        var model = new ProjectModel("temp", "application", "app", "/root");
        model.Name = "   ";

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ProjectType_DefaultsToApplication()
    {
        var model = new ProjectModel("my-app", "application", "app", "/root");

        Assert.Equal("application", model.ProjectType);
    }

    [Fact]
    public void Root_CanBeSetAndRetrieved()
    {
        var model = new ProjectModel("my-app", "application", "app", "/root");
        model.Root = "src/app";

        Assert.Equal("src/app", model.Root);
    }
}
