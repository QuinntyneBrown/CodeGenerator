// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.ReactNative.Artifacts;

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

    public async Task<List<FileModel>> IndexCreate(string directory)
    {
        _logger.LogInformation("Index Create");

        List<FileModel> result = [];

        List<string> lines = [];

        foreach (var path in _fileSystem.Directory.GetDirectories(directory))
        {
            var containsIndex = _fileSystem.Directory.GetFiles(path)
                .Select(Path.GetFileNameWithoutExtension)
                .Contains("index");

            if (containsIndex)
            {
                lines.Add($"export * from './{_fileSystem.Path.GetFileNameWithoutExtension(path)}';");
            }
        }

        foreach (var file in _fileSystem.Directory.GetFiles(directory, "*.ts"))
        {
            if (!file.Contains(".spec.") && !file.Contains(".test.") && !file.EndsWith("index.ts"))
            {
                lines.Add($"export * from './{_fileSystem.Path.GetFileNameWithoutExtension(file)}';");
            }
        }

        foreach (var file in _fileSystem.Directory.GetFiles(directory, "*.tsx"))
        {
            if (!file.Contains(".spec.") && !file.Contains(".test.") && !file.EndsWith("index.tsx"))
            {
                lines.Add($"export * from './{_fileSystem.Path.GetFileNameWithoutExtension(file)}';");
            }
        }

        result.Add(new("index", directory, ".ts")
        {
            Body = string.Join(System.Environment.NewLine, lines)
        });

        return result;
    }
}
