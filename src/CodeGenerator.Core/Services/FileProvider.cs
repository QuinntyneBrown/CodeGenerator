// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.Linq;

namespace CodeGenerator.Core.Services;

public class FileProvider : IFileProvider
{
    private readonly IFileSystem _fileSystem;

    public FileProvider(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public string Get(string searchPattern, string directory, int depth = 0)
    {
        var parts = directory.Split(_fileSystem.Path.DirectorySeparatorChar);

        if (parts.Length == depth)
        {
            return CodeGenerator.Core.Constants.FileNotFound;
        }

        var searchDirectory = string.Join(_fileSystem.Path.DirectorySeparatorChar, parts.Take(parts.Length - depth));
        var file = _fileSystem.Directory.GetFiles(searchDirectory, searchPattern).FirstOrDefault();

        // If searching for *.sln and not found, also try *.slnx
        if (file == null && searchPattern == "*.sln")
        {
            file = _fileSystem.Directory.GetFiles(searchDirectory, "*.slnx").FirstOrDefault();
        }

        return file ?? Get(searchPattern, directory, depth + 1);
    }
}