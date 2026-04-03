// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Core.UnitTests;

public class TemplatedFileModelTests
{
    [Fact]
    public void Constructor_SetsTemplate()
    {
        var model = new TemplatedFileModel("MyTemplate", "MyFile", "/output", ".cs");
        Assert.Equal("MyTemplate", model.Template);
    }

    [Fact]
    public void Constructor_SetsNameDirectoryExtension()
    {
        var model = new TemplatedFileModel("tmpl", "Name", "/dir", ".ts");
        Assert.Equal("Name", model.Name);
        Assert.Equal("/dir", model.Directory);
        Assert.Equal(".ts", model.Extension);
    }

    [Fact]
    public void Constructor_NoTokens_HasEmptyDictionary()
    {
        var model = new TemplatedFileModel("tmpl", "Name", "/dir", ".cs");
        Assert.NotNull(model.Tokens);
        Assert.Empty(model.Tokens);
    }

    [Fact]
    public void Constructor_WithTokens_CopiesTokens()
    {
        var tokens = new Dictionary<string, object>
        {
            ["entity"] = "Customer",
            ["namespace"] = "MyApp"
        };

        var model = new TemplatedFileModel("tmpl", "Name", "/dir", ".cs", tokens);

        Assert.Equal(2, model.Tokens.Count);
        Assert.Equal("Customer", model.Tokens["entity"]);
        Assert.Equal("MyApp", model.Tokens["namespace"]);
    }

    [Fact]
    public void Constructor_NullTokens_HasEmptyDictionary()
    {
        var model = new TemplatedFileModel("tmpl", "Name", "/dir", ".cs", null);
        Assert.NotNull(model.Tokens);
        Assert.Empty(model.Tokens);
    }

    [Fact]
    public void IsFileModel()
    {
        var model = new TemplatedFileModel("tmpl", "Test", "/dir", ".cs");
        Assert.IsAssignableFrom<FileModel>(model);
    }

    [Fact]
    public void Constructor_SetsPath()
    {
        var model = new TemplatedFileModel("tmpl", "MyFile", "/output", ".cs");
        Assert.Contains("MyFile.cs", model.Path);
    }
}
