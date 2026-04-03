// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Core.UnitTests;

public class SkipFileExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessage()
    {
        var ex = new SkipFileException();
        Assert.Equal("Template signaled to skip file generation.", ex.Message);
    }

    [Fact]
    public void ReasonConstructor_SetsCustomMessage()
    {
        var ex = new SkipFileException("file already exists");
        Assert.Equal("file already exists", ex.Message);
    }

    [Fact]
    public void IsException()
    {
        var ex = new SkipFileException();
        Assert.IsAssignableFrom<Exception>(ex);
    }
}
