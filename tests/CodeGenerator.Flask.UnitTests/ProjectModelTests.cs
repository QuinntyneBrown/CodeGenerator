// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Flask.Artifacts;

namespace CodeGenerator.Flask.UnitTests;

public class ProjectModelTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var model = new ProjectModel("MyApp", "/root");

        Assert.Equal("MyApp", model.Name);
    }

    [Fact]
    public void Constructor_SetsRootDirectory()
    {
        var model = new ProjectModel("MyApp", "/root");

        Assert.Equal("/root", model.RootDirectory);
    }

    [Fact]
    public void Constructor_SetsDirectory_CombinesRootAndName()
    {
        var model = new ProjectModel("MyApp", "/root");

        Assert.Equal(Path.Combine("/root", "MyApp"), model.Directory);
    }

    [Fact]
    public void Constructor_SetsAppDirectory_CombinesDirectoryAndApp()
    {
        var model = new ProjectModel("MyApp", "/root");

        Assert.Equal(Path.Combine("/root", "MyApp", "app"), model.AppDirectory);
    }

    [Fact]
    public void Constructor_InitializesEmptyFeatures()
    {
        var model = new ProjectModel("MyApp", "/root");

        Assert.NotNull(model.Features);
        Assert.Empty(model.Features);
    }

    [Fact]
    public void Validate_WithValidName_ReturnsValidResult()
    {
        var model = new ProjectModel("MyApp", "/root");

        var result = model.Validate();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsError()
    {
        var model = new ProjectModel("", "/root");

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].PropertyName);
    }

    [Fact]
    public void Validate_WithWhitespaceName_ReturnsError()
    {
        var model = new ProjectModel("   ", "/root");

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithNullName_ReturnsError()
    {
        var model = new ProjectModel("MyApp", "/root");
        model.Name = null!;

        var result = model.Validate();

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Features_CanBeModified()
    {
        var model = new ProjectModel("MyApp", "/root");
        model.Features.Add("auth");
        model.Features.Add("cors");

        Assert.Equal(2, model.Features.Count);
        Assert.Contains("auth", model.Features);
        Assert.Contains("cors", model.Features);
    }

    [Fact]
    public void Validate_ErrorMessage_ContainsProjectNameRequired()
    {
        var model = new ProjectModel("", "/root");

        var result = model.Validate();

        Assert.Contains(result.Errors, e => e.ErrorMessage == "Project name is required.");
    }
}
