// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.ReactNative.Artifacts;

public class ProjectReferenceModel : ArtifactModel
{
    public ProjectReferenceModel(string name, string referencedDirectory)
    {
        Name = name;
        ReferencedDirectory = referencedDirectory;
    }

    public string Name { get; set; }

    public string ReferencedDirectory { get; set; }
}
