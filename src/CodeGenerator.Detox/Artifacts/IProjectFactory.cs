// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Detox.Artifacts;

public interface IProjectFactory
{
    ProjectModel Create(string name, string directory, string appName, string platforms = "ios,android");
}
