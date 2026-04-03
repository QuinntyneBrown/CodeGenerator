// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Incremental.Models;
using CodeGenerator.Core.Incremental.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Incremental.Strategies;

public class AddFileStrategy : IArtifactGenerationStrategy<AddFileModel>
{
    private readonly IFileSystem _fileSystem;
    private readonly IConflictResolver _conflictResolver;
    private readonly ILogger<AddFileStrategy> _logger;

    public AddFileStrategy(IFileSystem fileSystem, IConflictResolver conflictResolver, ILogger<AddFileStrategy> logger)
    {
        _fileSystem = fileSystem;
        _conflictResolver = conflictResolver;
        _logger = logger;
    }

    public int GetPriority() => 0;

    public async Task GenerateAsync(AddFileModel model)
    {
        var fullPath = _fileSystem.Path.Combine(model.ProjectDirectory, model.RelativePath);

        if (_fileSystem.File.Exists(fullPath))
        {
            var existingContent = await _fileSystem.File.ReadAllTextAsync(fullPath);
            var action = _conflictResolver.Resolve(fullPath, existingContent, model.Content);

            switch (action)
            {
                case ConflictAction.Skip:
                    _logger.LogInformation("Skipping existing file: {Path}", model.RelativePath);
                    return;

                case ConflictAction.Error:
                    throw new IOException($"File already exists and conflict behavior is Error: {fullPath}");

                case ConflictAction.Overwrite:
                    _logger.LogInformation("Overwriting existing file: {Path}", model.RelativePath);
                    break;
            }
        }

        var directory = _fileSystem.Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
        {
            _fileSystem.Directory.CreateDirectory(directory);
        }

        await _fileSystem.File.WriteAllTextAsync(fullPath, model.Content);

        _logger.LogInformation("Added file: {Path}", model.RelativePath);
    }
}
