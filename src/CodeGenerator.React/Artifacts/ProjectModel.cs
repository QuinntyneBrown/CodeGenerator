// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.React.Artifacts;

public class ProjectModel : ArtifactModel
{
    public ProjectModel(string name, string projectType, string rootDirectory)
    {
        Name = name;
        RootDirectory = rootDirectory;
        ProjectType = projectType;

        if (name.StartsWith('@'))
        {
            var parts = name.Split('/');

            Directory = $"{RootDirectory}{Path.DirectorySeparatorChar}packages{Path.DirectorySeparatorChar}{parts[0].Replace("@", string.Empty)}{Path.DirectorySeparatorChar}{parts[1]}";
        }
        else
        {
            Directory = $"{RootDirectory}{Path.DirectorySeparatorChar}packages{Path.DirectorySeparatorChar}{name}";
        }
    }

    public string Name { get; set; }

    public string Directory { get; set; }

    public string RootDirectory { get; set; }

    public string ProjectType { get; set; } = "application";
}
