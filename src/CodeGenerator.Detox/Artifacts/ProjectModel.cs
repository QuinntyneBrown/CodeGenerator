// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Detox.Artifacts;

public class ProjectModel : ArtifactModel
{
    public ProjectModel(string name, string rootDirectory, string appName, string platforms = "ios,android")
    {
        Name = name;
        RootDirectory = rootDirectory;
        AppName = appName;
        Platforms = platforms;
        Directory = $"{RootDirectory}{Path.DirectorySeparatorChar}{name}";
    }

    public string Name { get; set; }

    public string Directory { get; set; }

    public string RootDirectory { get; set; }

    public string AppName { get; set; }

    public string Platforms { get; set; } = "ios,android";
}
