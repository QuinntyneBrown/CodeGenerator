// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions.TestingHelpers;
using CodeGenerator.Core.Incremental.Services;

namespace CodeGenerator.Core.UnitTests;

public class ProjectContextTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var fs = new MockFileSystem();
        var ctx = new ProjectContext("/project", ProjectType.DotNet, fs);

        Assert.Equal("/project", ctx.ProjectDirectory);
        Assert.Equal(ProjectType.DotNet, ctx.Type);
    }

    [Fact]
    public void FileExists_ExistingFile_ReturnsTrue()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\src\MyFile.cs"] = new MockFileData("content")
        });
        var ctx = new ProjectContext(@"C:\project", ProjectType.DotNet, fs);

        Assert.True(ctx.FileExists(@"src\MyFile.cs"));
    }

    [Fact]
    public void FileExists_NonExistingFile_ReturnsFalse()
    {
        var fs = new MockFileSystem();
        fs.AddDirectory(@"C:\project");
        var ctx = new ProjectContext(@"C:\project", ProjectType.DotNet, fs);

        Assert.False(ctx.FileExists(@"src\Missing.cs"));
    }

    [Fact]
    public void ReadFile_ReturnsContent()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\test.cs"] = new MockFileData("public class Test {}")
        });
        var ctx = new ProjectContext(@"C:\project", ProjectType.DotNet, fs);

        var content = ctx.ReadFile("test.cs");
        Assert.Equal("public class Test {}", content);
    }

    [Fact]
    public void FindFiles_MatchingPattern_ReturnsRelativePaths()
    {
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\project\src\File1.cs"] = new MockFileData(""),
            [@"C:\project\src\File2.cs"] = new MockFileData(""),
            [@"C:\project\src\readme.md"] = new MockFileData("")
        });
        var ctx = new ProjectContext(@"C:\project", ProjectType.DotNet, fs);

        var files = ctx.FindFiles("*.cs");
        Assert.Equal(2, files.Length);
    }

    [Fact]
    public void FindFiles_NonExistingDirectory_ReturnsEmpty()
    {
        var fs = new MockFileSystem();
        var ctx = new ProjectContext(@"C:\nonexistent", ProjectType.Unknown, fs);

        var files = ctx.FindFiles("*.cs");
        Assert.Empty(files);
    }

    [Fact]
    public void ImplementsIProjectContext()
    {
        var fs = new MockFileSystem();
        var ctx = new ProjectContext("/project", ProjectType.Unknown, fs);
        Assert.IsAssignableFrom<IProjectContext>(ctx);
    }
}
