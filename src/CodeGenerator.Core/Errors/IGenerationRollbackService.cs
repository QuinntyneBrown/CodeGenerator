// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Errors;

public interface IGenerationRollbackService
{
    // Legacy methods for backward compatibility
    void TrackFile(string filePath);
    void TrackDirectory(string directoryPath);

    // New granular tracking methods
    void TrackFileCreated(string filePath);
    void TrackFileModified(string filePath);
    void TrackFileDeleted(string filePath, string backupPath);
    void TrackDirectoryCreated(string directoryPath);

    void Commit();
    void Rollback();
    RollbackReport RollbackWithReport();
}

public class GenerationRollbackService : IGenerationRollbackService
{
    private readonly List<string> _trackedFiles = new();
    private readonly List<string> _trackedDirectories = new();
    private readonly List<RollbackAction> _actions = new();
    private readonly ILogger<GenerationRollbackService> _logger;
    private bool _committed;

    public GenerationRollbackService(ILogger<GenerationRollbackService> logger)
    {
        _logger = logger;
    }

    public void TrackFile(string filePath) => _trackedFiles.Add(filePath);

    public void TrackDirectory(string directoryPath) => _trackedDirectories.Add(directoryPath);

    public void TrackFileCreated(string filePath)
    {
        _actions.Add(new RollbackAction(RollbackActionType.FileCreated, filePath));
    }

    public void TrackFileModified(string filePath)
    {
        // Create a backup of the current file content
        string? backupPath = null;
        if (File.Exists(filePath))
        {
            backupPath = filePath + ".bak." + Guid.NewGuid().ToString("N")[..8];
            File.Copy(filePath, backupPath, overwrite: true);
        }

        _actions.Add(new RollbackAction(RollbackActionType.FileModified, filePath, backupPath));
    }

    public void TrackFileDeleted(string filePath, string backupPath)
    {
        _actions.Add(new RollbackAction(RollbackActionType.FileDeleted, filePath, backupPath));
    }

    public void TrackDirectoryCreated(string directoryPath)
    {
        _actions.Add(new RollbackAction(RollbackActionType.DirectoryCreated, directoryPath));
    }

    public void Commit()
    {
        _committed = true;

        // Clean up backup files on commit
        foreach (var action in _actions)
        {
            if (action.BackupPath != null && File.Exists(action.BackupPath))
            {
                try
                {
                    File.Delete(action.BackupPath);
                }
                catch
                {
                    // Best effort cleanup
                }
            }
        }

        _actions.Clear();
        _trackedFiles.Clear();
        _trackedDirectories.Clear();
        _logger.LogDebug("Generation committed. Rollback disabled.");
    }

    public void Rollback()
    {
        RollbackWithReport();
    }

    public RollbackReport RollbackWithReport()
    {
        var report = new RollbackReport();

        if (_committed)
        {
            _logger.LogDebug("Rollback skipped: generation already committed.");
            return report;
        }

        // Process new-style actions in LIFO order
        for (int i = _actions.Count - 1; i >= 0; i--)
        {
            var action = _actions[i];
            try
            {
                switch (action.Type)
                {
                    case RollbackActionType.FileCreated:
                        if (File.Exists(action.Path))
                        {
                            File.Delete(action.Path);
                            report.FilesDeleted.Add(action.Path);
                            _logger.LogDebug("Rolled back created file: {File}", action.Path);
                        }
                        break;

                    case RollbackActionType.FileModified:
                        if (action.BackupPath != null && File.Exists(action.BackupPath))
                        {
                            File.Copy(action.BackupPath, action.Path, overwrite: true);
                            File.Delete(action.BackupPath);
                            report.FilesRestored.Add(action.Path);
                            _logger.LogDebug("Restored modified file: {File}", action.Path);
                        }
                        break;

                    case RollbackActionType.FileDeleted:
                        if (action.BackupPath != null && File.Exists(action.BackupPath))
                        {
                            File.Copy(action.BackupPath, action.Path, overwrite: true);
                            File.Delete(action.BackupPath);
                            report.FilesRestored.Add(action.Path);
                            _logger.LogDebug("Restored deleted file: {File}", action.Path);
                        }
                        break;

                    case RollbackActionType.DirectoryCreated:
                        if (Directory.Exists(action.Path) && !Directory.EnumerateFileSystemEntries(action.Path).Any())
                        {
                            Directory.Delete(action.Path);
                            report.DirectoriesDeleted.Add(action.Path);
                            _logger.LogDebug("Rolled back directory: {Dir}", action.Path);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                report.Failures.Add(new RollbackFailure(action, ex));
                _logger.LogWarning(ex, "Failed to rollback action {Type} for {Path}", action.Type, action.Path);
            }
        }

        // Also handle legacy tracked files
        foreach (var file in _trackedFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
                report.FilesDeleted.Add(file);
                _logger.LogDebug("Rolled back file: {File}", file);
            }
        }

        foreach (var dir in _trackedDirectories.OrderByDescending(d => d.Length))
        {
            if (Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any())
            {
                Directory.Delete(dir);
                report.DirectoriesDeleted.Add(dir);
                _logger.LogDebug("Rolled back directory: {Dir}", dir);
            }
        }

        _actions.Clear();
        _trackedFiles.Clear();
        _trackedDirectories.Clear();

        return report;
    }
}
