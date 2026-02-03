// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Syntax.VisualStudio;

public class ProjectModel
{
    public ProjectModel()
    {
        ProjectGuid = $"{{{Guid.NewGuid()}}}";
    }

    public string ProjectGuid { get; set; }

    public string Name { get; set; }

    public string Path { get; set; }

    public string ProjectTypeGuid { get; set; } = ProjectTypeGuids.TypeScriptProjectTypeGuid;
}
