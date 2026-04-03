// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;

namespace CodeGenerator.Core.Artifacts;

public class FileModel : ArtifactModel
{
    private readonly IFileSystem _fileSystem;

    public FileModel(string name, string directory, string extension, IFileSystem? fileSystem = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
        Name = name;
        Directory = directory;
        Extension = extension;
        Path = _fileSystem.Path.Combine(Directory, $"{Name}{Extension}");
    }

    public string Body { get; set; }

    public string Name { get; set; }

    public string Directory { get; set; }

    public string Extension { get; set; }

    public string Path { get; set; }
}
