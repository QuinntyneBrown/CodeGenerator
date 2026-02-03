// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;

namespace CodeGenerator.DotNet.Services;

public class SolutionNamespaceProvider : ISolutionNamespaceProvider
{
    private readonly IFileProvider fileProvider;
    private readonly IFileSystem _fileSystem;

    public SolutionNamespaceProvider(IFileProvider fileProvider, IFileSystem fileSystem)
    {
        this.fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public string Get(string directory)
    {
        if (!_fileSystem.Directory.Exists(directory))
        {
            return "SolutionNamespaceNotFound";
        }

        return _fileSystem.Path.GetFileNameWithoutExtension(fileProvider.Get("*.sln", directory));
    }
}
