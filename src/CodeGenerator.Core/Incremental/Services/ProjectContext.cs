// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;

namespace CodeGenerator.Core.Incremental.Services;

public class ProjectContext : IProjectContext
{
    private readonly IFileSystem _fileSystem;

    public ProjectContext(string projectDirectory, ProjectType type, IFileSystem fileSystem)
    {
        ProjectDirectory = projectDirectory;
        Type = type;
        _fileSystem = fileSystem;
    }

    public string ProjectDirectory { get; }

    public ProjectType Type { get; }

    public bool FileExists(string relativePath)
    {
        var fullPath = _fileSystem.Path.Combine(ProjectDirectory, relativePath);
        return _fileSystem.File.Exists(fullPath);
    }

    public string[] FindFiles(string pattern)
    {
        if (!_fileSystem.Directory.Exists(ProjectDirectory))
            return [];

        return _fileSystem.Directory.GetFiles(ProjectDirectory, pattern, SearchOption.AllDirectories)
            .Select(f => _fileSystem.Path.GetRelativePath(ProjectDirectory, f))
            .ToArray();
    }

    public string ReadFile(string relativePath)
    {
        var fullPath = _fileSystem.Path.Combine(ProjectDirectory, relativePath);
        return _fileSystem.File.ReadAllText(fullPath);
    }
}
