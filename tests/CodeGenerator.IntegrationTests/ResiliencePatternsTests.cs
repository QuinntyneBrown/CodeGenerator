// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Errors;
using CodeGenerator.Core.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class ResiliencePatternsTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<GenerationRollbackService> _logger;

    public ResiliencePatternsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "resilience_tests_" + Guid.NewGuid().ToString("N")[..8]);
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
    // RetryOptions defaults
    // =====================================================================

    [Fact]
    public void RetryOptions_HasCorrectDefaults()
    {
        var options = new RetryOptions();

        Assert.Equal(3, options.MaxAttempts);
        Assert.Equal(TimeSpan.FromMilliseconds(100), options.InitialDelay);
        Assert.Equal(2.0, options.BackoffMultiplier);
        Assert.Equal(TimeSpan.FromSeconds(5), options.MaxDelay);
    }

    // =====================================================================
    // RetryOptions predefined profiles
    // =====================================================================

    [Fact]
    public void RetryOptions_FileWrite_Profile()
    {
        var options = RetryOptions.FileWrite;

        Assert.Equal(3, options.MaxAttempts);
        Assert.Equal(TimeSpan.FromMilliseconds(100), options.InitialDelay);
        Assert.Equal(2.0, options.BackoffMultiplier);
        Assert.Equal(TimeSpan.FromSeconds(5), options.MaxDelay);
    }

    [Fact]
    public void RetryOptions_DirectoryCreate_Profile()
    {
        var options = RetryOptions.DirectoryCreate;

        Assert.Equal(3, options.MaxAttempts);
        Assert.Equal(TimeSpan.FromMilliseconds(50), options.InitialDelay);
        Assert.Equal(2.0, options.BackoffMultiplier);
        Assert.Equal(TimeSpan.FromSeconds(2), options.MaxDelay);
    }

    [Fact]
    public void RetryOptions_TemplateLoad_Profile()
    {
        var options = RetryOptions.TemplateLoad;

        Assert.Equal(4, options.MaxAttempts);
        Assert.Equal(TimeSpan.FromMilliseconds(200), options.InitialDelay);
        Assert.Equal(2.0, options.BackoffMultiplier);
        Assert.Equal(TimeSpan.FromSeconds(10), options.MaxDelay);
    }

    [Fact]
    public void RetryOptions_None_Profile()
    {
        var options = RetryOptions.None;

        Assert.Equal(1, options.MaxAttempts);
        Assert.Equal(TimeSpan.Zero, options.InitialDelay);
        Assert.Equal(1.0, options.BackoffMultiplier);
        Assert.Equal(TimeSpan.Zero, options.MaxDelay);
    }

    // =====================================================================
    // Retry.ExecuteAsync tests
    // =====================================================================

    [Fact]
    public async Task Retry_ExecuteAsync_SucceedsOnFirstTry_ReturnsResult()
    {
        var result = await Retry.ExecuteAsync(
            () => Task.FromResult(42),
            RetryOptions.None);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task Retry_ExecuteAsync_RetriesTransientIOException_SucceedsOnSecondTry()
    {
        int attempt = 0;

        var result = await Retry.ExecuteAsync(async () =>
        {
            attempt++;
            if (attempt == 1)
            {
                throw new IOException("File locked");
            }

            return "success";
        }, new RetryOptions { InitialDelay = TimeSpan.FromMilliseconds(1) });

        Assert.Equal("success", result);
        Assert.Equal(2, attempt);
    }

    [Fact]
    public async Task Retry_ExecuteAsync_DoesNotRetry_NonTransientException()
    {
        int attempt = 0;

        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await Retry.ExecuteAsync<string>(async () =>
            {
                attempt++;
                throw new ArgumentException("bad arg");
            }, new RetryOptions { InitialDelay = TimeSpan.FromMilliseconds(1) });
        });

        Assert.Equal(1, attempt);
    }

    [Fact]
    public async Task Retry_ExecuteAsync_ExhaustsAllAttempts_ThrowsLastException()
    {
        int attempt = 0;

        var ex = await Assert.ThrowsAsync<IOException>(async () =>
        {
            await Retry.ExecuteAsync<string>(async () =>
            {
                attempt++;
                throw new IOException($"Attempt {attempt}");
            }, new RetryOptions { MaxAttempts = 3, InitialDelay = TimeSpan.FromMilliseconds(1) });
        });

        Assert.Equal(3, attempt);
        Assert.Equal("Attempt 3", ex.Message);
    }

    [Fact]
    public void Retry_IsTransient_ReturnsTrue_ForIOException()
    {
        Assert.True(Retry.IsTransient(new IOException("locked")));
    }

    [Fact]
    public void Retry_IsTransient_ReturnsFalse_ForFileNotFoundException()
    {
        Assert.False(Retry.IsTransient(new FileNotFoundException("missing")));
    }

    [Fact]
    public async Task Retry_VoidVariant_Works()
    {
        int callCount = 0;

        await Retry.ExecuteAsync(async () =>
        {
            callCount++;
            await Task.CompletedTask;
        }, RetryOptions.None);

        Assert.Equal(1, callCount);
    }

    // =====================================================================
    // RollbackReport properties
    // =====================================================================

    [Fact]
    public void RollbackReport_Properties()
    {
        var report = new RollbackReport();

        Assert.Empty(report.FilesDeleted);
        Assert.Empty(report.FilesRestored);
        Assert.Empty(report.DirectoriesDeleted);
        Assert.Empty(report.Failures);
        Assert.True(report.FullyRolledBack);
    }

    [Fact]
    public void RollbackReport_WithFailures_NotFullyRolledBack()
    {
        var report = new RollbackReport();
        report.Failures.Add(new RollbackFailure(
            new RollbackAction(RollbackActionType.FileCreated, "/tmp/test.cs"),
            new IOException("access denied")));

        Assert.False(report.FullyRolledBack);
    }

    // =====================================================================
    // RollbackFailure properties
    // =====================================================================

    [Fact]
    public void RollbackFailure_Properties()
    {
        var action = new RollbackAction(RollbackActionType.FileCreated, "/tmp/test.cs");
        var ex = new IOException("fail");
        var failure = new RollbackFailure(action, ex);

        Assert.Same(action, failure.Action);
        Assert.Same(ex, failure.Exception);
    }

    // =====================================================================
    // RollbackAction types
    // =====================================================================

    [Fact]
    public void RollbackAction_Types()
    {
        Assert.Equal(0, (int)RollbackActionType.FileCreated);
        Assert.Equal(1, (int)RollbackActionType.FileModified);
        Assert.Equal(2, (int)RollbackActionType.FileDeleted);
        Assert.Equal(3, (int)RollbackActionType.DirectoryCreated);
    }

    [Fact]
    public void RollbackAction_Properties()
    {
        var action = new RollbackAction(RollbackActionType.FileModified, "/tmp/x.cs", "/tmp/x.cs.bak");

        Assert.Equal(RollbackActionType.FileModified, action.Type);
        Assert.Equal("/tmp/x.cs", action.Path);
        Assert.Equal("/tmp/x.cs.bak", action.BackupPath);
    }

    // =====================================================================
    // Rollback tracking with real temp files
    // =====================================================================

    [Fact]
    public void TrackFileCreated_Rollback_DeletesFile()
    {
        var svc = new GenerationRollbackService(_logger);
        var filePath = Path.Combine(_tempDir, "created.cs");
        File.WriteAllText(filePath, "content");

        svc.TrackFileCreated(filePath);
        var report = svc.RollbackWithReport();

        Assert.False(File.Exists(filePath));
        Assert.Contains(filePath, report.FilesDeleted);
        Assert.True(report.FullyRolledBack);
    }

    [Fact]
    public void TrackFileModified_Rollback_RestoresOriginalContent()
    {
        var svc = new GenerationRollbackService(_logger);
        var filePath = Path.Combine(_tempDir, "modified.cs");
        File.WriteAllText(filePath, "original content");

        svc.TrackFileModified(filePath);

        // Simulate modification
        File.WriteAllText(filePath, "modified content");

        var report = svc.RollbackWithReport();

        Assert.Equal("original content", File.ReadAllText(filePath));
        Assert.Contains(filePath, report.FilesRestored);
        Assert.True(report.FullyRolledBack);
    }

    [Fact]
    public void TrackFileDeleted_Rollback_RestoresFile()
    {
        var svc = new GenerationRollbackService(_logger);
        var filePath = Path.Combine(_tempDir, "deleted.cs");
        File.WriteAllText(filePath, "precious data");

        // Create backup before deleting
        var backupPath = filePath + ".bak";
        File.Copy(filePath, backupPath);
        File.Delete(filePath);

        svc.TrackFileDeleted(filePath, backupPath);
        var report = svc.RollbackWithReport();

        Assert.True(File.Exists(filePath));
        Assert.Equal("precious data", File.ReadAllText(filePath));
        Assert.Contains(filePath, report.FilesRestored);
        Assert.True(report.FullyRolledBack);
    }

    [Fact]
    public void TrackDirectoryCreated_Rollback_DeletesEmptyDirectory()
    {
        var svc = new GenerationRollbackService(_logger);
        var dirPath = Path.Combine(_tempDir, "newdir");
        Directory.CreateDirectory(dirPath);

        svc.TrackDirectoryCreated(dirPath);
        var report = svc.RollbackWithReport();

        Assert.False(Directory.Exists(dirPath));
        Assert.Contains(dirPath, report.DirectoriesDeleted);
        Assert.True(report.FullyRolledBack);
    }

    // =====================================================================
    // LIFO ordering
    // =====================================================================

    [Fact]
    public void Rollback_ReversesInLIFOOrder()
    {
        var svc = new GenerationRollbackService(_logger);

        // Create a directory, then a file inside it
        var dirPath = Path.Combine(_tempDir, "lifo_dir");
        Directory.CreateDirectory(dirPath);
        svc.TrackDirectoryCreated(dirPath);

        var filePath = Path.Combine(dirPath, "inner.cs");
        File.WriteAllText(filePath, "test");
        svc.TrackFileCreated(filePath);

        // LIFO: file deleted first, then directory
        var report = svc.RollbackWithReport();

        Assert.False(File.Exists(filePath));
        Assert.False(Directory.Exists(dirPath));
        Assert.Contains(filePath, report.FilesDeleted);
        Assert.Contains(dirPath, report.DirectoriesDeleted);
    }

    // =====================================================================
    // Commit clears tracking
    // =====================================================================

    [Fact]
    public void Commit_ClearsTracking_RollbackReturnsEmptyReport()
    {
        var svc = new GenerationRollbackService(_logger);
        var filePath = Path.Combine(_tempDir, "committed.cs");
        File.WriteAllText(filePath, "content");

        svc.TrackFileCreated(filePath);
        svc.Commit();

        var report = svc.RollbackWithReport();

        // File should still exist after commit + rollback
        Assert.True(File.Exists(filePath));
        Assert.Empty(report.FilesDeleted);
        Assert.Empty(report.FilesRestored);
        Assert.Empty(report.DirectoriesDeleted);
        Assert.Empty(report.Failures);
        Assert.True(report.FullyRolledBack);
    }
}
