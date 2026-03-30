// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Python.Artifacts;

public class ProjectFactory : IProjectFactory
{
    public ProjectModel Create(string name, string projectType, string directory)
        => new ProjectModel(name, projectType, directory);
}
