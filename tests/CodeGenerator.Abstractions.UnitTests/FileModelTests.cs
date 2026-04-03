// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions.TestingHelpers;
using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Abstractions.UnitTests;

public class FileModelTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var fileSystem = new MockFileSystem();

        var model = new FileModel("MyFile", "/src", ".cs", fileSystem);

        Assert.Equal("MyFile", model.Name);
        Assert.Equal("/src", model.Directory);
        Assert.Equal(".cs", model.Extension);
    }

    [Fact]
    public void Constructor_ShouldComputePath()
    {
        var fileSystem = new MockFileSystem();

        var model = new FileModel("MyFile", "/src", ".cs", fileSystem);

        Assert.Equal(fileSystem.Path.Combine("/src", "MyFile.cs"), model.Path);
    }

    [Fact]
    public void Constructor_WithNullFileSystem_ShouldUseDefaultFileSystem()
    {
        var model = new FileModel("MyFile", "/src", ".cs");

        Assert.NotNull(model.Path);
    }

    [Fact]
    public void Body_ShouldBeSettable()
    {
        var fileSystem = new MockFileSystem();
        var model = new FileModel("MyFile", "/src", ".cs", fileSystem);

        model.Body = "file content";

        Assert.Equal("file content", model.Body);
    }

    [Fact]
    public void Validate_WithValidInputs_ShouldReturnValid()
    {
        var fileSystem = new MockFileSystem();
        var model = new FileModel("MyFile", "/src", ".cs", fileSystem);

        var result = model.Validate();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithNullName_ShouldReturnError()
    {
        var fileSystem = new MockFileSystem();
        var model = new FileModel(null!, "/src", ".cs", fileSystem);

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldReturnError()
    {
        var fileSystem = new MockFileSystem();
        var model = new FileModel("", "/src", ".cs", fileSystem);

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "File name is required.");
    }

    [Fact]
    public void Validate_WithWhitespaceName_ShouldReturnError()
    {
        var fileSystem = new MockFileSystem();
        var model = new FileModel("   ", "/src", ".cs", fileSystem);

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Constructor_WithNullDirectory_ShouldThrowArgumentNullException()
    {
        var fileSystem = new MockFileSystem();

        Assert.ThrowsAny<ArgumentNullException>(() => new FileModel("MyFile", null!, ".cs", fileSystem));
    }

    [Fact]
    public void Validate_WithEmptyDirectory_ShouldReturnError()
    {
        var fileSystem = new MockFileSystem();
        var model = new FileModel("MyFile", "", ".cs", fileSystem);

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Directory");
    }

    [Fact]
    public void Validate_WithWhitespaceDirectory_ShouldReturnError()
    {
        var fileSystem = new MockFileSystem();
        var model = new FileModel("MyFile", "   ", ".cs", fileSystem);

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Directory");
    }

    [Fact]
    public void Validate_WithNullExtension_ShouldReturnError()
    {
        var fileSystem = new MockFileSystem();
        var model = new FileModel("MyFile", "/src", null!, fileSystem);

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Extension" && e.ErrorMessage == "File extension is required and must start with '.'.");
    }

    [Fact]
    public void Validate_WithEmptyExtension_ShouldReturnError()
    {
        var fileSystem = new MockFileSystem();
        var model = new FileModel("MyFile", "/src", "", fileSystem);

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Extension");
    }

    [Fact]
    public void Validate_WithExtensionMissingDot_ShouldReturnError()
    {
        var fileSystem = new MockFileSystem();
        var model = new FileModel("MyFile", "/src", "cs", fileSystem);

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Extension");
    }

    [Fact]
    public void Validate_WithAllInvalid_ShouldReturnThreeErrors()
    {
        var fileSystem = new MockFileSystem();
        var model = new FileModel("", "", "", fileSystem);

        var result = model.Validate();

        Assert.False(result.IsValid);
        Assert.Equal(3, result.Errors.Count);
    }

    [Fact]
    public void FileModel_ShouldBeArtifactModel()
    {
        var fileSystem = new MockFileSystem();
        var model = new FileModel("MyFile", "/src", ".cs", fileSystem);

        Assert.IsAssignableFrom<ArtifactModel>(model);
    }
}
