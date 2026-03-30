// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.ReactNative.Artifacts;

public class ProjectModel : ArtifactModel
{
    public ProjectModel(string name, string rootDirectory, string platform = "both")
    {
        Name = name;
        RootDirectory = rootDirectory;
        Platform = platform;
        Directory = $"{RootDirectory}{Path.DirectorySeparatorChar}{name}";
    }

    public string Name { get; set; }

    public string Directory { get; set; }

    public string RootDirectory { get; set; }

    public string Platform { get; set; } = "both";
}
