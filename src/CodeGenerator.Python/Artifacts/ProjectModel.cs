// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Python.Artifacts;

public class ProjectModel : ArtifactModel
{
    public ProjectModel(string name, string projectType, string rootDirectory)
    {
        Name = name;
        ProjectType = projectType;
        RootDirectory = rootDirectory;
        Directory = Path.Combine(rootDirectory, name);
        VirtualEnvPath = Path.Combine(Directory, ".venv");
    }

    public string Name { get; set; }

    public string Directory { get; set; }

    public string RootDirectory { get; set; }

    public string ProjectType { get; set; } = Constants.ProjectType.Package;

    public string VirtualEnvPath { get; set; }

    public List<PackageModel> Packages { get; set; } = [];
}
