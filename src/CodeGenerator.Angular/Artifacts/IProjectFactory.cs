// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Angular.Artifacts;

public interface IProjectFactory
{
    ProjectModel Create(string name, string prefix, string directory);
}
