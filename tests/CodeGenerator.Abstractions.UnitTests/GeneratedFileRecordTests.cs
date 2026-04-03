// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Services;

namespace CodeGenerator.Abstractions.UnitTests;

public class GeneratedFileRecordTests
{
    [Fact]
    public void Constructor_ShouldSetPath()
    {
        var record = new GeneratedFileRecord("/src/file.cs", "TestGenerator", DateTime.UtcNow);

        Assert.Equal("/src/file.cs", record.Path);
    }

    [Fact]
    public void Constructor_ShouldSetGeneratorName()
    {
        var record = new GeneratedFileRecord("/src/file.cs", "TestGenerator", DateTime.UtcNow);

        Assert.Equal("TestGenerator", record.GeneratorName);
    }

    [Fact]
    public void Constructor_ShouldSetGeneratedAt()
    {
        var timestamp = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        var record = new GeneratedFileRecord("/src/file.cs", "TestGenerator", timestamp);

        Assert.Equal(timestamp, record.GeneratedAt);
    }

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        var timestamp = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var record1 = new GeneratedFileRecord("/src/file.cs", "Gen", timestamp);
        var record2 = new GeneratedFileRecord("/src/file.cs", "Gen", timestamp);

        Assert.Equal(record1, record2);
    }

    [Fact]
    public void Equality_DifferentPath_ShouldNotBeEqual()
    {
        var timestamp = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var record1 = new GeneratedFileRecord("/src/a.cs", "Gen", timestamp);
        var record2 = new GeneratedFileRecord("/src/b.cs", "Gen", timestamp);

        Assert.NotEqual(record1, record2);
    }

    [Fact]
    public void Equality_DifferentGeneratorName_ShouldNotBeEqual()
    {
        var timestamp = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var record1 = new GeneratedFileRecord("/src/file.cs", "Gen1", timestamp);
        var record2 = new GeneratedFileRecord("/src/file.cs", "Gen2", timestamp);

        Assert.NotEqual(record1, record2);
    }

    [Fact]
    public void Equality_DifferentTimestamp_ShouldNotBeEqual()
    {
        var record1 = new GeneratedFileRecord("/src/file.cs", "Gen", new DateTime(2025, 1, 1));
        var record2 = new GeneratedFileRecord("/src/file.cs", "Gen", new DateTime(2025, 1, 2));

        Assert.NotEqual(record1, record2);
    }

    [Fact]
    public void ToString_ShouldContainValues()
    {
        var timestamp = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var record = new GeneratedFileRecord("/src/file.cs", "TestGenerator", timestamp);

        var str = record.ToString();

        Assert.Contains("/src/file.cs", str);
        Assert.Contains("TestGenerator", str);
    }

    [Fact]
    public void GetHashCode_SameValues_ShouldBeEqual()
    {
        var timestamp = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var record1 = new GeneratedFileRecord("/src/file.cs", "Gen", timestamp);
        var record2 = new GeneratedFileRecord("/src/file.cs", "Gen", timestamp);

        Assert.Equal(record1.GetHashCode(), record2.GetHashCode());
    }

    [Fact]
    public void Deconstruct_ShouldReturnAllValues()
    {
        var timestamp = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var record = new GeneratedFileRecord("/src/file.cs", "TestGenerator", timestamp);

        var (path, generatorName, generatedAt) = record;

        Assert.Equal("/src/file.cs", path);
        Assert.Equal("TestGenerator", generatorName);
        Assert.Equal(timestamp, generatedAt);
    }
}
