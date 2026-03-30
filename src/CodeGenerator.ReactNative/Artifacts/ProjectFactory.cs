// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.ReactNative.Artifacts;

public class ProjectFactory : IProjectFactory
{
    public ProjectModel Create(string name, string directory, string platform = "both")
        => new ProjectModel(name, directory, platform);
}
