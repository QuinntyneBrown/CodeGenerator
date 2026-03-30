// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Python.Artifacts;

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
        _logger.LogInformation("Creating __init__.py files");

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

    public FileModel CreateRequirementsFile(string directory, List<PackageModel> packages)
    {
        _logger.LogInformation("Creating requirements.txt");

        var lines = packages.Select(p =>
            string.IsNullOrEmpty(p.Version) ? p.Name : $"{p.Name}=={p.Version}");

        return new(Constants.FileNames.Requirements, directory, Constants.FileExtensions.Requirements)
        {
            Body = string.Join(Environment.NewLine, lines)
        };
    }
}
