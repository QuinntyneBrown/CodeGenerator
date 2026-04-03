// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.UnitTests;

public class StringExtensionsTests
{
    // ── RemoveTrivia ──

    [Fact]
    public void RemoveTrivia_RemovesSpaces()
    {
        var result = "Hello World".RemoveTrivia();
        Assert.Equal("HelloWorld", result);
    }

    [Fact]
    public void RemoveTrivia_RemovesNewLines()
    {
        var input = $"Line1{Environment.NewLine}Line2";
        var result = input.RemoveTrivia();
        Assert.Equal("Line1Line2", result);
    }

    [Fact]
    public void RemoveTrivia_RemovesBothSpacesAndNewLines()
    {
        var input = $"Hello World{Environment.NewLine}Foo Bar";
        var result = input.RemoveTrivia();
        Assert.Equal("HelloWorldFooBar", result);
    }

    [Fact]
    public void RemoveTrivia_EmptyString_ReturnsEmpty()
    {
        var result = "".RemoveTrivia();
        Assert.Equal("", result);
    }

    // ── Indent ──

    [Fact]
    public void Indent_SingleLine_IndentsCorrectly()
    {
        var result = "Hello".Indent(1);
        Assert.Equal("    Hello", result);
    }

    [Fact]
    public void Indent_SingleLine_IndentTwo()
    {
        var result = "Hello".Indent(2);
        Assert.Equal("        Hello", result);
    }

    [Fact]
    public void Indent_SingleLine_CustomSpaces()
    {
        var result = "Hello".Indent(1, 2);
        Assert.Equal("  Hello", result);
    }

    [Fact]
    public void Indent_MultiLine_IndentsAllNonEmptyLines()
    {
        var input = $"Line1{Environment.NewLine}Line2{Environment.NewLine}Line3";
        var result = input.Indent(1);
        var lines = result.Split(Environment.NewLine);
        Assert.Equal("    Line1", lines[0]);
        Assert.Equal("    Line2", lines[1]);
        Assert.Equal("    Line3", lines[2]);
    }

    [Fact]
    public void Indent_MultiLine_EmptyLinesAreNotIndented()
    {
        var input = $"Line1{Environment.NewLine}{Environment.NewLine}Line3";
        var result = input.Indent(1);
        var lines = result.Split(Environment.NewLine);
        Assert.Equal("    Line1", lines[0]);
        Assert.Equal("", lines[1]);
        Assert.Equal("    Line3", lines[2]);
    }

    [Fact]
    public void Indent_EmptyString_ReturnsEmpty()
    {
        var result = "".Indent(1);
        Assert.Equal("", result);
    }

    [Fact]
    public void Indent_ZeroIndent_ReturnsOriginal()
    {
        var result = "Hello".Indent(0);
        Assert.Equal("Hello", result);
    }

    // ── Remove ──

    [Fact]
    public void Remove_RemovesSubstring()
    {
        var result = "Hello World".Remove("World");
        Assert.Equal("Hello ", result);
    }

    [Fact]
    public void Remove_EmptyItem_ReturnsOriginal()
    {
        var result = "Hello World".Remove("");
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void Remove_NullItem_ReturnsOriginal()
    {
        var result = "Hello World".Remove(null!);
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void Remove_NoMatch_ReturnsOriginal()
    {
        var result = "Hello World".Remove("xyz");
        Assert.Equal("Hello World", result);
    }

    // ── GetResourceName ──

    [Fact]
    public void GetResourceName_SingleExactMatch_ReturnsMatch()
    {
        var collection = new[] { "Namespace.Templates.MyTemplate" };
        var result = collection.GetResourceName("MyTemplate");
        Assert.Equal("Namespace.Templates.MyTemplate", result);
    }

    [Fact]
    public void GetResourceName_MultipleExactMatches_ReturnsShortest()
    {
        var collection = new[]
        {
            "A.B.C.MyTemplate",
            "A.MyTemplate"
        };
        var result = collection.GetResourceName("MyTemplate");
        Assert.Equal("A.MyTemplate", result);
    }

    [Fact]
    public void GetResourceName_NoExactMatch_FallsToTxtMatch()
    {
        var collection = new[] { "Namespace.Templates.MyTemplate.txt" };
        var result = collection.GetResourceName("MyTemplate");
        Assert.Equal("Namespace.Templates.MyTemplate.txt", result);
    }

    [Fact]
    public void GetResourceName_MultipleTxtMatches_ReturnsShortest()
    {
        var collection = new[]
        {
            "A.B.C.MyTemplate.txt",
            "A.MyTemplate.txt"
        };
        var result = collection.GetResourceName("MyTemplate");
        Assert.Equal("A.MyTemplate.txt", result);
    }

    [Fact]
    public void GetResourceName_NoMatch_ReturnsNull()
    {
        var collection = new[] { "Namespace.Templates.Other" };
        var result = collection.GetResourceName("MyTemplate");
        Assert.Null(result);
    }

    [Fact]
    public void GetResourceName_EmptyCollection_ReturnsNull()
    {
        var collection = Array.Empty<string>();
        var result = collection.GetResourceName("MyTemplate");
        Assert.Null(result);
    }

    // ── GetNameAndType ──

    [Fact]
    public void GetNameAndType_WithColon_SplitsCorrectly()
    {
        var result = "Name:string".GetNameAndType();
        Assert.Equal("Name", result.Item1);
        Assert.Equal("string", result.Item2);
    }

    [Fact]
    public void GetNameAndType_WithoutColon_ReturnsNameAndEmptyType()
    {
        var result = "Name".GetNameAndType();
        Assert.Equal("Name", result.Item1);
        Assert.Equal(string.Empty, result.Item2);
    }

    [Fact]
    public void GetNameAndType_WithMultipleColons_ReturnsFirstTwo()
    {
        var result = "Name:Type:Extra".GetNameAndType();
        Assert.Equal("Name", result.Item1);
        Assert.Equal("Type", result.Item2);
    }
}
