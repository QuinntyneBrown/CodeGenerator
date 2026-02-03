// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.Linq;

namespace CodeGenerator.DotNet.Services;

public class NamespaceProvider : INamespaceProvider
{
    private readonly IFileSystem _fileSystem;

    public NamespaceProvider(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public string Get(string directory, int depth = 0)
    {
        if (string.IsNullOrEmpty(directory))
        {
            return "NamespaceNotFound";
        }

        var parts = directory.Split(_fileSystem.Path.DirectorySeparatorChar);

        if (parts.Length <= depth)
        {
            return "NamespaceNotFound";
        }

        var path = string.Join(_fileSystem.Path.DirectorySeparatorChar, parts.Take(parts.Length - depth));

        // Check if directory exists before trying to enumerate files
        string? projectFile = null;
        if (_fileSystem.Directory.Exists(path))
        {
            projectFile = _fileSystem.Directory.GetFiles(path, "*.csproj").FirstOrDefault();
        }

        if (!string.IsNullOrEmpty(projectFile))
        {
            var rootNamespace = _fileSystem.Path.GetFileNameWithoutExtension(projectFile);

            if (depth > 0)
            {
                var subNamespace = string.Join('.', parts.Skip(parts.Length - depth));

                return $"{rootNamespace}.{subNamespace}";
            }

            return rootNamespace;
        }
        else
        {
            depth++;

            return Get(directory, depth);
        }
    }
}
