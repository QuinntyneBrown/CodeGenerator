// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Flask.Artifacts;

public class FileFactory : IFileFactory
{
    private readonly ILogger<FileFactory> _logger;
    private readonly IFileSystem _fileSystem;

    public FileFactory(ILogger<FileFactory> logger, IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _logger = logger;
        _fileSystem = fileSystem;
    }

    public async Task<List<FileModel>> CreateInitFiles(string directory)
    {
        _logger.LogInformation("Creating __init__.py files for Flask app directories");

        List<FileModel> result = [];

        foreach (var path in _fileSystem.Directory.GetDirectories(directory))
        {
            var initPath = Path.Combine(path, "__init__.py");

            if (!_fileSystem.File.Exists(initPath))
            {
                result.Add(new(Constants.FileNames.Init, path, Constants.FileExtensions.Python)
                {
                    Body = string.Empty
                });
            }
        }

        return result;
    }

    public FileModel CreateInitFile(string directory, string? content = null)
    {
        _logger.LogInformation("Creating __init__.py file");

        return new(Constants.FileNames.Init, directory, Constants.FileExtensions.Python)
        {
            Body = content ?? string.Empty
        };
    }
}
