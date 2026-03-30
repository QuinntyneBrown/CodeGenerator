// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Flask.Artifacts;

public class ProjectModel : ArtifactModel
{
    public ProjectModel(string name, string rootDirectory)
    {
        Name = name;
        RootDirectory = rootDirectory;
        Directory = Path.Combine(rootDirectory, name);
        AppDirectory = Path.Combine(Directory, "app");
        Features = [];
    }

    public string Name { get; set; }

    public string Directory { get; set; }

    public string RootDirectory { get; set; }

    public string AppDirectory { get; set; }

    public List<string> Features { get; set; }
}
