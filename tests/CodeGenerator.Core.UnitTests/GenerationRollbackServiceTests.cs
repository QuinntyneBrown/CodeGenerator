// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Errors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CodeGenerator.Core.UnitTests;

public class GenerationRollbackServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly GenerationRollbackService _service;

    public GenerationRollbackServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"rollback_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        var logger = NullLoggerFactory.Instance.CreateLogger<GenerationRollbackService>();
        _service = new GenerationRollbackService(logger);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [Fact]
    public void TrackFile_DoesNotThrow()
    {
        _service.TrackFile(Path.Combine(_tempDir, "file.cs"));
    }

    [Fact]
    public void TrackDirectory_DoesNotThrow()
    {
        _service.TrackDirectory(_tempDir);
    }

    [Fact]
    public void Commit_PreventsRollback()
    {
        var filePath = Path.Combine(_tempDir, "committed.txt");
        File.WriteAllText(filePath, "content");
        _service.TrackFile(filePath);

        _service.Commit();
        _service.Rollback();

        // File should still exist because rollback was skipped
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void Rollback_DeletesTrackedFiles()
    {
        var filePath = Path.Combine(_tempDir, "rollback_file.txt");
        File.WriteAllText(filePath, "content");
        _service.TrackFile(filePath);

        _service.Rollback();

        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void Rollback_DeletesEmptyTrackedDirectories()
    {
        var subDir = Path.Combine(_tempDir, "emptyDir");
        Directory.CreateDirectory(subDir);
        _service.TrackDirectory(subDir);

        _service.Rollback();

        Assert.False(Directory.Exists(subDir));
    }

    [Fact]
    public void Rollback_DoesNotDeleteNonEmptyDirectories()
    {
        var subDir = Path.Combine(_tempDir, "nonEmptyDir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "keep.txt"), "content");
        _service.TrackDirectory(subDir);

        _service.Rollback();

        // Directory should still exist because it has content
        Assert.True(Directory.Exists(subDir));
    }

    [Fact]
    public void Rollback_SkipsNonExistentFiles()
    {
        _service.TrackFile(Path.Combine(_tempDir, "does_not_exist.txt"));
        // Should not throw
        _service.Rollback();
    }

    [Fact]
    public void Rollback_SkipsNonExistentDirectories()
    {
        _service.TrackDirectory(Path.Combine(_tempDir, "does_not_exist_dir"));
        // Should not throw
        _service.Rollback();
    }

    [Fact]
    public void Rollback_ClearsTrackedItemsAfterRollback()
    {
        var filePath = Path.Combine(_tempDir, "clear_test.txt");
        File.WriteAllText(filePath, "content");
        _service.TrackFile(filePath);

        _service.Rollback();
        Assert.False(File.Exists(filePath));

        // Re-create the file and rollback again - should not delete it
        // because tracked items were cleared
        File.WriteAllText(filePath, "new content");
        _service.Rollback();
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void Rollback_DeletesDeepDirectoriesFirst()
    {
        var parentDir = Path.Combine(_tempDir, "parent");
        var childDir = Path.Combine(parentDir, "child");
        Directory.CreateDirectory(childDir);

        _service.TrackDirectory(parentDir);
        _service.TrackDirectory(childDir);

        _service.Rollback();

        // Both should be deleted since child is empty and deleted first,
        // then parent becomes empty and is also deleted
        Assert.False(Directory.Exists(childDir));
        Assert.False(Directory.Exists(parentDir));
    }
}
