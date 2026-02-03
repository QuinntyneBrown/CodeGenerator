// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Angular.Artifacts;

public class WorkspaceModel(string name, string version, string rootDirectory) : ArtifactModel
{
    public string Version { get; set; } = version;

    public string Name { get; set; } = name;

    public string RootDirectory { get; set; } = rootDirectory;

    public string Directory { get; set; } = Path.Combine(rootDirectory, name);

    public List<ProjectModel> Projects { get; set; } = [];
}
