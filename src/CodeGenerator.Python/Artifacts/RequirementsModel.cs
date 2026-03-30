// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Python.Artifacts;

public class RequirementsModel : ArtifactModel
{
    public RequirementsModel(string directory)
    {
        Directory = directory;
        Packages = [];
    }

    public string Directory { get; set; }

    public List<PackageModel> Packages { get; set; }
}
