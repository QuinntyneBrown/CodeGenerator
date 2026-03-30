// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Playwright.Artifacts;

public class ProjectFactory : IProjectFactory
{
    public ProjectModel Create(string name, string directory, string baseUrl, List<string>? browsers = null)
        => new ProjectModel(name, directory, baseUrl, browsers);
}
