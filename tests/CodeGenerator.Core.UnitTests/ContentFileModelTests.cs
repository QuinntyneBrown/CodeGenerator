// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Core.UnitTests;

public class ContentFileModelTests
{
    [Fact]
    public void Constructor_SetsContent()
    {
        var model = new ContentFileModel("public class Foo {}", "MyClass", "/output", ".cs");
        Assert.Equal("public class Foo {}", model.Content);
    }

    [Fact]
    public void Constructor_SetsNameDirectoryExtension()
    {
        var model = new ContentFileModel("content", "MyFile", "/src", ".ts");
        Assert.Equal("MyFile", model.Name);
        Assert.Equal("/src", model.Directory);
        Assert.Equal(".ts", model.Extension);
    }

    [Fact]
    public void Constructor_SetsPathFromBaseClass()
    {
        var model = new ContentFileModel("content", "MyFile", "/src", ".cs");
        Assert.Contains("MyFile.cs", model.Path);
    }

    [Fact]
    public void IsFileModel()
    {
        var model = new ContentFileModel("content", "Test", "/dir", ".cs");
        Assert.IsAssignableFrom<FileModel>(model);
    }

    [Fact]
    public void Validate_ValidModel_Succeeds()
    {
        var model = new ContentFileModel("content", "Test", "/dir", ".cs");
        var result = model.Validate();
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyName_Fails()
    {
        var model = new ContentFileModel("content", "", "/dir", ".cs");
        var result = model.Validate();
        Assert.False(result.IsValid);
    }
}
