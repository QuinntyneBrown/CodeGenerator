// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.DotNet.Artifacts.Projects;
using CodeGenerator.DotNet.Artifacts.Solutions;

namespace CodeGenerator.DotNet.Artifacts.FullStack;

public class FullStackModel
{
    public SolutionModel Solution { get; init; } = new();

    public ProjectModel? FrontendProject { get; init; }

    public ProjectModel? BackendProject { get; init; }
}
