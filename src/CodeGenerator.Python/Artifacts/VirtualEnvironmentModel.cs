// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Python.Artifacts;

public class VirtualEnvironmentModel : ArtifactModel
{
    public VirtualEnvironmentModel(string name, string directory)
    {
        Name = name;
        Directory = directory;
        Path = System.IO.Path.Combine(directory, name);
    }

    public string Name { get; set; } = ".venv";

    public string Directory { get; set; }

    public string Path { get; set; }
}
