// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Core.UnitTests;

public class FileModelTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var file = new FileModel("MyClass", "/output", ".cs");
        Assert.Equal("MyClass", file.Name);
        Assert.Equal("/output", file.Directory);
        Assert.Equal(".cs", file.Extension);
        Assert.Contains("MyClass.cs", file.Path);
    }

    [Fact]
    public void Validate_ValidModel_ReturnsSuccess()
    {
        var file = new FileModel("MyClass", "/output", ".cs");
        var result = file.Validate();
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var file = new FileModel("", "/output", ".cs");
        var result = file.Validate();
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_EmptyDirectory_ReturnsError()
    {
        var file = new FileModel("MyClass", "", ".cs");
        var result = file.Validate();
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Directory");
    }

    [Fact]
    public void Validate_InvalidExtension_ReturnsError()
    {
        var file = new FileModel("MyClass", "/output", "cs");
        var result = file.Validate();
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Extension");
    }
}
