// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.IntegrationTests.Helpers;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class ComprehensiveTestSuiteTests
{
    [Fact]
    public void RoslynValidator_ValidCSharp_NoErrors()
    {
        var source = """
            using System;
            namespace Test;
            public class Foo
            {
                public void Bar() { }
            }
            """;

        var errors = RoslynValidator.ValidateCSharpSyntax(source);
        Assert.Empty(errors);
    }

    [Fact]
    public void RoslynValidator_InvalidCSharp_ReturnsErrors()
    {
        var source = "public class { }"; // missing class name

        var errors = RoslynValidator.ValidateCSharpSyntax(source);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void RoslynValidator_AssertValidCSharp_PassesForValidCode()
    {
        var source = """
            public class MyClass
            {
                public string Name { get; set; }
            }
            """;

        // Should not throw
        RoslynValidator.AssertValidCSharp(source, "test class");
    }

    [Fact]
    public void RoslynValidator_AssertValidCSharp_ThrowsForInvalidCode()
    {
        var source = "public class { }";

        Assert.ThrowsAny<Exception>(() =>
            RoslynValidator.AssertValidCSharp(source, "invalid test"));
    }

    [Fact]
    public void TempDirectoryFixture_CreatesDirectory()
    {
        using var fixture = new TempDirectoryFixture();

        Assert.True(Directory.Exists(fixture.Path));
    }

    [Fact]
    public void TempDirectoryFixture_CleansUpOnDispose()
    {
        string path;
        using (var fixture = new TempDirectoryFixture())
        {
            path = fixture.Path;
            Assert.True(Directory.Exists(path));
        }

        Assert.False(Directory.Exists(path));
    }

    [Fact]
    public void TempDirectoryFixture_FileExists_ReturnsCorrectly()
    {
        using var fixture = new TempDirectoryFixture();

        var filePath = Path.Combine(fixture.Path, "test.txt");
        File.WriteAllText(filePath, "hello");

        Assert.True(fixture.FileExists("test.txt"));
        Assert.False(fixture.FileExists("nonexistent.txt"));
    }

    [Fact]
    public void TempDirectoryFixture_ReadFile_ReturnsContent()
    {
        using var fixture = new TempDirectoryFixture();

        var filePath = Path.Combine(fixture.Path, "test.txt");
        File.WriteAllText(filePath, "hello world");

        Assert.Equal("hello world", fixture.ReadFile("test.txt"));
    }

    [Fact]
    public void CliTestResult_RecordProperties()
    {
        var result = new CliTestResult(0, "stdout output", "stderr output");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("stdout output", result.StdOut);
        Assert.Equal("stderr output", result.StdErr);
    }
}
