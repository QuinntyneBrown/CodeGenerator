// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;

namespace CodeGenerator.Core.Incremental.Services;

public class ProjectContextFactory : IProjectContextFactory
{
    private readonly IFileSystem _fileSystem;

    public ProjectContextFactory(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public IProjectContext Create(string directory)
    {
        var type = DetectProjectType(directory);
        return new ProjectContext(directory, type, _fileSystem);
    }

    private ProjectType DetectProjectType(string directory)
    {
        if (!_fileSystem.Directory.Exists(directory))
            return ProjectType.Unknown;

        var files = _fileSystem.Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly);
        var fileNames = files.Select(f => _fileSystem.Path.GetFileName(f)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (fileNames.Any(f => f.EndsWith(".csproj") || f.EndsWith(".sln") || f.EndsWith(".slnx")))
            return ProjectType.DotNet;

        if (fileNames.Contains("playwright.config.ts") || fileNames.Contains("playwright.config.js"))
            return ProjectType.Playwright;

        if (fileNames.Contains(".detoxrc.js") || fileNames.Contains(".detoxrc.json"))
            return ProjectType.Detox;

        if (fileNames.Contains("angular.json"))
            return ProjectType.Angular;

        if (fileNames.Contains("package.json"))
        {
            var packageJson = _fileSystem.File.ReadAllText(_fileSystem.Path.Combine(directory, "package.json"));

            if (packageJson.Contains("\"react-native\""))
                return ProjectType.ReactNative;

            if (packageJson.Contains("\"react\""))
                return ProjectType.React;
        }

        if (fileNames.Contains("wsgi.py") || fileNames.Contains("app.py"))
            return ProjectType.Flask;

        if (fileNames.Any(f => f.EndsWith(".py")) || fileNames.Contains("setup.py") || fileNames.Contains("pyproject.toml"))
            return ProjectType.Python;

        return ProjectType.Unknown;
    }
}
