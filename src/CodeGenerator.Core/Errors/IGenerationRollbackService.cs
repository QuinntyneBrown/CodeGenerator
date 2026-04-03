// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Errors;

public interface IGenerationRollbackService
{
    void TrackFile(string filePath);
    void TrackDirectory(string directoryPath);
    void Rollback();
    void Commit();
}

public class GenerationRollbackService : IGenerationRollbackService
{
    private readonly List<string> _trackedFiles = new();
    private readonly List<string> _trackedDirectories = new();
    private readonly ILogger<GenerationRollbackService> _logger;
    private bool _committed;

    public GenerationRollbackService(ILogger<GenerationRollbackService> logger)
    {
        _logger = logger;
    }

    public void TrackFile(string filePath) => _trackedFiles.Add(filePath);

    public void TrackDirectory(string directoryPath) => _trackedDirectories.Add(directoryPath);

    public void Commit()
    {
        _committed = true;
        _logger.LogDebug("Generation committed. Rollback disabled.");
    }

    public void Rollback()
    {
        if (_committed)
        {
            _logger.LogDebug("Rollback skipped: generation already committed.");
            return;
        }

        foreach (var file in _trackedFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
                _logger.LogDebug("Rolled back file: {File}", file);
            }
        }

        foreach (var dir in _trackedDirectories.OrderByDescending(d => d.Length))
        {
            if (Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any())
            {
                Directory.Delete(dir);
                _logger.LogDebug("Rolled back directory: {Dir}", dir);
            }
        }

        _trackedFiles.Clear();
        _trackedDirectories.Clear();
    }
}
