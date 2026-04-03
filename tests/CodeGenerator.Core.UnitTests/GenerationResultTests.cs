// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Core.UnitTests;

public class GenerationResultTests
{
    [Fact]
    public void AddFile_IncrementsCount()
    {
        var result = new GenerationResult();
        result.AddFile("/output/file.cs", "content");
        Assert.Equal(1, result.TotalFileCount);
    }

    [Fact]
    public void AddFile_CalculatesSize()
    {
        var result = new GenerationResult();
        result.AddFile("/output/file.cs", "hello");
        Assert.True(result.TotalSizeBytes > 0);
    }

    [Fact]
    public void AddCommand_RecordsEntry()
    {
        var result = new GenerationResult();
        result.AddCommand("dotnet new sln", "/work");
        Assert.Single(result.Commands);
        Assert.Equal("dotnet new sln", result.Commands[0].Command);
    }

    [Fact]
    public void IsSuccess_DefaultsToTrue()
    {
        var result = new GenerationResult();
        Assert.True(result.IsSuccess);
    }
}
