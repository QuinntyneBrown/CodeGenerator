// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;

namespace CodeGenerator.Core.Validation;

public class FileSystemRules
{
    private readonly IFileSystem _fileSystem;

    public FileSystemRules(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public bool IsWritableDirectory(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            if (!_fileSystem.Directory.Exists(path))
                return false;

            var testFile = _fileSystem.Path.Combine(path, $".codegen-write-test-{Guid.NewGuid():N}");
            _fileSystem.File.WriteAllText(testFile, string.Empty);
            _fileSystem.File.Delete(testFile);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    public bool ParentDirectoryExists(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var parent = _fileSystem.Path.GetDirectoryName(path);
        return parent is not null && _fileSystem.Directory.Exists(parent);
    }
}
