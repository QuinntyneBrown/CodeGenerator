// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Angular.Artifacts;

public class ProjectReferenceModel : ArtifactModel
{
    public ProjectReferenceModel(string name, string referencedDirectory, string projectType = "application")
    {
        Name = name;
        ReferencedDirectory = referencedDirectory;
        ProjectType = projectType;
    }

    public string Name { get; set; }

    public string ReferencedDirectory { get; set; }

    public string ProjectType { get; set; }
}
