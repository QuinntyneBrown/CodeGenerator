// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using CodeGenerator.Core.Validation;

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

    public override ValidationResult Validate()
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(Name))
            result.AddError(nameof(Name), "File name is required.");

        if (string.IsNullOrWhiteSpace(Directory))
            result.AddError(nameof(Directory), "File directory is required.");

        if (string.IsNullOrWhiteSpace(Extension) || !Extension.StartsWith('.'))
            result.AddError(nameof(Extension), "File extension is required and must start with '.'.");

        return result;
    }
}
