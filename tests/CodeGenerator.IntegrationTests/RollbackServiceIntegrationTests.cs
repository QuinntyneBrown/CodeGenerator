// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Errors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class RollbackServiceIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<GenerationRollbackService> _logger;

    public RollbackServiceIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "rollback_integration_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _logger = NullLogger<GenerationRollbackService>.Instance;
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    // =====================================================================
    // RollbackService tracks file creation and rolls back
    // =====================================================================

    [Fact]
    public void RollbackService_TracksFileCreation_AndRollsBack()
    {
        var service = new GenerationRollbackService(_logger);

        var filePath = Path.Combine(_tempDir, "tracked-file.cs");
        File.WriteAllText(filePath, "// generated content");
        service.TrackFileCreated(filePath);

        Assert.True(File.Exists(filePath));

        var report = service.RollbackWithReport();

        Assert.False(File.Exists(filePath));
        Assert.Contains(filePath, report.FilesDeleted);
        Assert.True(report.FullyRolledBack);
    }

    // =====================================================================
    // RollbackService tracks directory creation and rolls back
    // =====================================================================

    [Fact]
    public void RollbackService_TracksDirectoryCreation_AndRollsBack()
    {
        var service = new GenerationRollbackService(_logger);

        var dirPath = Path.Combine(_tempDir, "tracked-dir");
        Directory.CreateDirectory(dirPath);
        service.TrackDirectoryCreated(dirPath);

        Assert.True(Directory.Exists(dirPath));

        var report = service.RollbackWithReport();

        Assert.False(Directory.Exists(dirPath));
        Assert.Contains(dirPath, report.DirectoriesDeleted);
        Assert.True(report.FullyRolledBack);
    }

    // =====================================================================
    // Commit prevents rollback
    // =====================================================================

    [Fact]
    public void RollbackService_CommitPreventsRollback()
    {
        var service = new GenerationRollbackService(_logger);

        var filePath = Path.Combine(_tempDir, "committed-file.cs");
        File.WriteAllText(filePath, "// committed content");
        service.TrackFileCreated(filePath);

        service.Commit();

        var report = service.RollbackWithReport();

        Assert.True(File.Exists(filePath));
        Assert.Empty(report.FilesDeleted);
        Assert.Empty(report.FilesRestored);
        Assert.Empty(report.DirectoriesDeleted);
    }

    // =====================================================================
    // Multiple items rolled back in LIFO order
    // =====================================================================

    [Fact]
    public void RollbackService_TracksMultipleItems_RollsBackInLifoOrder()
    {
        var service = new GenerationRollbackService(_logger);
        var rollbackOrder = new List<string>();

        var file1 = Path.Combine(_tempDir, "first.cs");
        var file2 = Path.Combine(_tempDir, "second.cs");
        var file3 = Path.Combine(_tempDir, "third.cs");

        File.WriteAllText(file1, "// first");
        service.TrackFileCreated(file1);

        File.WriteAllText(file2, "// second");
        service.TrackFileCreated(file2);

        File.WriteAllText(file3, "// third");
        service.TrackFileCreated(file3);

        var report = service.RollbackWithReport();

        // All three files should be deleted
        Assert.False(File.Exists(file1));
        Assert.False(File.Exists(file2));
        Assert.False(File.Exists(file3));
        Assert.Equal(3, report.FilesDeleted.Count);

        // LIFO order: third deleted first, first deleted last
        Assert.Equal(file3, report.FilesDeleted[0]);
        Assert.Equal(file2, report.FilesDeleted[1]);
        Assert.Equal(file1, report.FilesDeleted[2]);
    }

    // =====================================================================
    // CommandRollbackWrapper pattern: success commits
    // =====================================================================

    [Fact]
    public async Task CommandRollbackWrapper_OnSuccess_Commits()
    {
        var service = new GenerationRollbackService(_logger);
        var wrapperLogger = NullLogger.Instance;

        var filePath = Path.Combine(_tempDir, "wrapper-success.cs");

        await CommandRollbackWrapper.ExecuteWithRollbackAsync(
            service,
            async (rollback) =>
            {
                File.WriteAllText(filePath, "// generated");
                rollback.TrackFileCreated(filePath);
                await Task.CompletedTask;
            },
            wrapperLogger);

        // File should remain after successful commit
        Assert.True(File.Exists(filePath));

        // Subsequent rollback should be a no-op since already committed
        var report = service.RollbackWithReport();
        Assert.Empty(report.FilesDeleted);
    }

    // =====================================================================
    // CommandRollbackWrapper pattern: failure rolls back and rethrows
    // =====================================================================

    [Fact]
    public async Task CommandRollbackWrapper_OnFailure_RollsBackAndRethrows()
    {
        var service = new GenerationRollbackService(_logger);
        var wrapperLogger = NullLogger.Instance;

        var filePath = Path.Combine(_tempDir, "wrapper-failure.cs");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await CommandRollbackWrapper.ExecuteWithRollbackAsync(
                service,
                async (rollback) =>
                {
                    File.WriteAllText(filePath, "// generated");
                    rollback.TrackFileCreated(filePath);
                    await Task.CompletedTask;
                    throw new InvalidOperationException("Generation failed");
                },
                wrapperLogger);
        });

        Assert.Equal("Generation failed", ex.Message);
        // File should have been rolled back
        Assert.False(File.Exists(filePath));
    }
}
