// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Flask.Artifacts;

public class ProjectFactory : IProjectFactory
{
    public ProjectModel Create(string name, string directory)
        => new ProjectModel(name, directory);

    public ProjectModel Create(string name, string directory, List<string> features)
    {
        var model = new ProjectModel(name, directory);
        model.Features = features;
        return model;
    }
}
