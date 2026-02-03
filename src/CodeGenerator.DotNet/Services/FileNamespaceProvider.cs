// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.Linq;

namespace CodeGenerator.DotNet.Services;

public class FileNamespaceProvider : IFileNamespaceProvider
{
    private readonly IFileProvider fileProvider;
    private readonly IFileSystem _fileSystem;

    public FileNamespaceProvider(IFileProvider fileProvider, IFileSystem fileSystem)
    {
        this.fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public string Get(string directory)
    {
        var projectNamespace = _fileSystem.Path.GetFileNameWithoutExtension(fileProvider.Get("*.csproj", directory));

        var projectDirectoryParts = _fileSystem.Path.GetDirectoryName(fileProvider.Get("*.csproj", directory)).Split(_fileSystem.Path.DirectorySeparatorChar);

        var fileDirectoryParts = directory.Split(_fileSystem.Path.DirectorySeparatorChar).Skip(projectDirectoryParts.Length);

        return fileDirectoryParts.Any() ? $"{projectNamespace}.{string.Join(".", fileDirectoryParts)}" : projectNamespace;
    }
}
